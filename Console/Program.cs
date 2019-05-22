using Dapper;
using Generator.Common;
using Generator.Core;
using Generator.Core.Config;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Console
{
    class Program
    {
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
            IProgressBar progress = GetProgressBar();
            BaseParser parser = null;
            switch (config1.DBType)
            {
                case "mssql":
                    parser = new Generator.Core.MSSql.Parser(config1, progress);
                    break;
                case "mysql":
                    parser = new Generator.Core.MySql.Parser(config1, progress);
                    break;
            }
            var meta_data = parser.ParseMetadata();

            Print("解析完毕，生成中间配置文件...");
            // 生成中间配置文件
            var meta_json = JsonConvert.SerializeObject(meta_data);
            SQLMetaDataHelper.OutputConfig(meta_json, config1, progress);

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
                    SQLMetaDataHelper.OutputDAL(meta_data, config1, progress);

                    // 生成Model最终文件
                    Print("生成Model...");
                    SQLMetaDataHelper.OutputModel(meta_data, config1, progress);

                    // 生成Enum最终文件
                    Print("生成Enum...");
                    SQLMetaDataHelper.OutputEnum(meta_data, config1, progress);

                    // 检测partial字段有效性
                    Print("检测partial字段有效性...");
                    SQLMetaDataHelper.DoPartialCheck(meta_data, config1, progress);

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

        static ConsoleProgressBar GetProgressBar()
        {
            return new ConsoleProgressBar(System.Console.CursorLeft, System.Console.CursorTop, 50, ProgressBarType.Character);
        }
    }
}
