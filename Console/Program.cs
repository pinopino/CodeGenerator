using Dapper;
using Generator.Common;
using Generator.Core;
using Generator.Core.Config;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Console
{
    class Program
    {
        public abstract class BaseParser
        {
            private readonly string _connstr;
            private ConsoleProgressBar _progress;

            public string ConnStr { get { return this._connstr; } }

            public BaseParser(string connStr, bool enableProgress = true)
            {
                _connstr = connStr;
                if (enableProgress)
                    _progress = new ConsoleProgressBar(System.Console.CursorLeft, System.Console.CursorTop, 50, ProgressBarType.Character);
            }

            public abstract Dictionary<string, TableMetaData> ParseMetadata();

            protected void ProgressPrint(long index, long total)
            {
                if (_progress != null)
                    _progress.Dispaly(Convert.ToInt32((index / (total * 1.0)) * 100));
            }

            protected string FindDBName(string connStr)
            {
                var db_name = string.Empty;
                var cb = new DbConnectionStringBuilder(false);
                cb.ConnectionString = connStr;
                object database;
                if (cb.TryGetValue("Initial Catalog", out database))
                {
                    db_name = database.ToString();
                }
                else
                {
                    if (cb.TryGetValue("Database", out database))
                    {
                        db_name = database.ToString();
                    }
                }

                return db_name;
            }
        }

        public class Parser_MSSQL : BaseParser
        {
            public Parser_MSSQL(string connStr, bool enableProgress = true)
                : base(connStr, enableProgress)
            { }

            public override Dictionary<string, TableMetaData> ParseMetadata()
            {
                using (SQLServerManagement manage = new SQLServerManagement(base.ConnStr))
                {
                    var db_key = FindDBName(base.ConnStr);
                    var tables = manage.Databases[db_key].Tables;

                    var ret = new Dictionary<string, TableMetaData>();
                    // 解析表
                    var i = 0;
                    for (i = 0; i < tables.Count;)
                    {
                        var table = tables[i];
                        var meta_table = ParseTable(table);
                        // 解析行
                        foreach (Column col in table.Columns)
                        {
                            ParseColumn(meta_table, col);
                        }
                        ret.Add(table.Name, meta_table);

                        // 打印进度
                        ProgressPrint(++i, tables.Count + 1);
                    }

                    // 打印进度
                    ProgressPrint(++i, tables.Count + 1);

                    return ret;
                }
            }

            private TableMetaData ParseTable(Table table)
            {
                var meta_table = new TableMetaData();
                meta_table.Name = table.Name;
                meta_table.Comment = table.ExtendedProperties["MS_Description"]?.Value.ToString().Trim();

                return meta_table;
            }

            private void ParseColumn(TableMetaData metaTable, Column column)
            {
                var meta_col = new ColumnMetaData();
                meta_col.Name = column.Name;
                meta_col.DbType = SQLMetaDataHelper.MapCsharpType(column.DataType.ToString());
                meta_col.Comment = column.ExtendedProperties["MS_Description"]?.Value.ToString().Trim();
                meta_col.IsPrimaryKey = column.InPrimaryKey;
                meta_col.IsIdentity = column.Identity;
                meta_col.Nullable = column.Nullable;
                meta_col.HasDefaultValue = column.DefaultConstraint != null && !string.IsNullOrWhiteSpace(column.DefaultConstraint.Text);
                metaTable.Columns.Add(meta_col);

                // 主键
                if (column.InPrimaryKey)
                {
                    metaTable.PrimaryKey.Add(meta_col);
                    metaTable.ExistPredicate.Add(meta_col);
                    metaTable.WherePredicate.Add(meta_col);
                }

                // 标识
                if (column.Identity)
                {
                    metaTable.Identity = meta_col;
                }
            }
        }

        public class Parser_MYSQL : BaseParser
        {
            class FieldViewModel
            {
                public string Field { get; set; }
                public string Type { get; set; }
                public dynamic Collation { get; set; }
                public string Null { get; set; }
                public string Key { get; set; }
                public dynamic Default { get; set; }
                public string Extra { get; set; }
                public string Privileges { get; set; }
                public string Comment { get; set; }
            }

            public Parser_MYSQL(string connStr, bool enableProgress = true)
                : base(connStr, enableProgress)
            { }

            public override Dictionary<string, TableMetaData> ParseMetadata()
            {
                using (MySqlConnection connection = new MySqlConnection(base.ConnStr))
                {
                    var CommandText = "SHOW TABLES;";
                    connection.Open();
                    var TableNames = connection.Query<string>(CommandText).ToList();

                    foreach (var name in TableNames)
                    {
                        var table = new TableMetaData
                        {
                            Name = name,
                            Columns = new List<ColumnMetaData>(),
                            Comment = "",
                            ExistPredicate = new List<ColumnMetaData>(),
                            Identity = new ColumnMetaData(),
                            PrimaryKey = new List<ColumnMetaData>(),
                            WherePredicate = new List<ColumnMetaData>()
                        };

                        CommandText = $"SHOW FULL FIELDS FROM {name};"; // 得到表结构
                        var result = connection.Query<FieldViewModel>(CommandText).ToList();
                        foreach (var item in result)
                        {
                            item.Type = item.Type.Substring(0, item.Type.IndexOf("("));
                            if (item.Type.Any(p => p.Equals("enum") || p.Equals("set")))
                            {
                                Print($"表{name}存在enum和set字段，暂无对应数据结构，跳过此表的生成！");
                                break;
                            }
                            var ColumData = new ColumnMetaData
                            {
                                Comment = item.Comment,
                                DbType = SQLMetaDataHelper.MapCsharpType(item.Type),
                                HasDefaultValue = item.Default != null ? true : false,
                                IsIdentity = item.Key == "PRI" ? true : false,
                                IsPrimaryKey = item.Key == "PRI" ? true : false,
                                Name = item.Field,
                                Nullable = item.Null == "YES" ? true : false
                            };

                            if (item.Key == "PRI")
                            {
                                table.PrimaryKey.Add(ColumData);
                                table.WherePredicate.Add(ColumData);
                                table.ExistPredicate.Add(ColumData);
                            }
                            if (item.Extra == "auto_increment")
                            {
                                table.Identity = ColumData;
                            }
                            table.Columns.Add(ColumData);
                        }
                    }
                }

                return new Dictionary<string, TableMetaData>();
            }
        }

        static void Main(string[] args)
        {
            var config1 = new GlobalConfiguration();
            config1.Init();

            var conn_str = config1.DBConn;
            if (string.IsNullOrWhiteSpace(conn_str))
            {
                System.Console.WriteLine("未设置数据库连接字符串！");
                System.Console.Read();
                Environment.Exit(0);
            }

            // 根据配置文件确定生成mssql还是mysql
            // todo...

            Print("解析数据库元数据...");
            var parser = new Parser_MSSQL(conn_str);
            var meta_data = parser.ParseMetadata();

            Print("解析完毕，生成中间配置文件...");
            // 生成中间配置文件
            var config_json_str = JsonConvert.SerializeObject(meta_data);
            SQLMetaDataHelper.OutputConfig(config_json_str);

            // 生成最终文件
            Print("按 'y/Y' 继续生成最终操作类文件...");
            var key = string.Empty;
            do
            {
                key = System.Console.ReadLine();
                if (key == "Y" || key == "y")
                {
                    // 生成DAL最终文件
                    Print("生成DAL...");
                    SQLMetaDataHelper.OutputDAL(config1, meta_data);

                    // 生成Model最终文件
                    Print("生成Model...");
                    //SQLMetaDataHelper.OutputModel(config);

                    // 生成Enum最终文件
                    Print("生成Enum...");
                    //SQLMetaDataHelper.OutputEnum(config);

                    // 检测partial字段有效性
                    Print("检测partial字段有效性...");
                    //SQLMetaDataHelper.DoPartialCheck(config);

                    Print("生成完毕！");
                    break;
                }
                System.Console.WriteLine("按‘quit’退出");
            } while (key != "quit");


            Print("结束！");
            System.Console.Read();
            Environment.Exit(0);
        }

        // link: https://stackoverflow.com/questions/18596876/go-statements-blowing-up-sql-execution-in-net
        static void ReCreateDB(string connStr, Encoding encoding)
        {
            var config_path = SQLMetaDataHelper.Config.ReCreateDB.SQLFilePath;
            var db_names = SQLMetaDataHelper.Config.ReCreateDB.DBs;
            if (string.IsNullOrWhiteSpace(config_path) || db_names == null || db_names.Count == 0)
            {
                return;
            }

            var db = FindDBName(connStr);
            if (string.IsNullOrWhiteSpace(db))
            {
                return;
            }

            var files = Directory.GetFiles(config_path, "*.sql", SearchOption.TopDirectoryOnly);
            foreach (var item in db_names)
            {
                System.Console.WriteLine("尝试重新生成数据库[" + item.Name + "]...");
                System.Console.WriteLine("检测是否存在该数据库");
                if (IsExist(connStr, item.Name))
                {
                    connStr = connStr.Replace(db, item.Name);
                    using (var conn = new SqlConnection(connStr))
                    {
                        var svr = new Server(new ServerConnection(conn));
                        foreach (var file_path in files)
                        {
                            var script = File.ReadAllText(file_path, Encoding.GetEncoding("gb2312"));
                            svr.ConnectionContext.ExecuteNonQuery(script);
                        }
                    }
                    System.Console.WriteLine("存在，重新生成数据库[" + item.Name + "]成功");
                }
                else
                {
                    System.Console.WriteLine("不存在该数据库，结束");
                }
                System.Console.WriteLine();
            }
        }

        static string FindDBName(string connStr)
        {
            var db_name = string.Empty;
            var cb = new DbConnectionStringBuilder(false);
            cb.ConnectionString = connStr;
            object database;
            if (cb.TryGetValue("Initial Catalog", out database))
            {
                db_name = database.ToString();
            }
            else
            {
                if (cb.TryGetValue("Database", out database))
                {
                    db_name = database.ToString();
                }
            }

            return db_name;
        }

        static bool IsExist(string connStr, string db)
        {
            using (var conn = GetOpenConnection(connStr))
            {
                return conn.ExecuteScalar<int>("select count(*) From master.dbo.sysdatabases where name='" + db + "'") > 0;
            }
        }

        static SqlConnection GetOpenConnection(string connStr, bool mars = false)
        {
            var cs = connStr;
            if (mars)
            {
                var scsb = new SqlConnectionStringBuilder(cs)
                {
                    MultipleActiveResultSets = true
                };
                cs = scsb.ConnectionString;
            }
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }

        static void Print(string message)
        {
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine(message);
        }
    }
}
