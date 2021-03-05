using Dapper;
using Generator.Common;
using Generator.Core.Config;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Generator.Core.MySql
{
    public class Parser : BaseParser
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

        public Parser(GlobalConfiguration config, IProgressBar progress)
            : base(config, progress)
        { }

        public override Dictionary<string, TableMetaData> ParseMetadata()
        {
            var result = new Dictionary<string, TableMetaData>();
            using (MySqlConnection connection = new MySqlConnection(base.ConnStr))
            {
                var CommandText = "SHOW TABLES;";
                connection.Open();
                var TableNames = connection.Query<string>(CommandText).ToList();
                var count = 0;
                foreach (var name in TableNames)
                {
                    var table = new TableMetaData(name, string.Empty);
                    CommandText = $"SHOW FULL FIELDS FROM {name};"; // 得到表结构
                    var fields = connection.Query<FieldViewModel>(CommandText).ToList();
                    var _fail = false;
                    foreach (var item in fields)
                    {
                        item.Type = item.Type.IndexOf("(") < 0 ? item.Type : item.Type.Substring(0, item.Type.IndexOf("("));
                        if (item.Type.Any(p => p.Equals("enum") || p.Equals("set")))
                        {
                            _fail = true;
                            Console.WriteLine($"表{name}存在enum和set字段，暂无对应数据结构，跳过此表的生成！");
                            break;
                        }
                        var ColumData = new ColumnMetaData
                        {
                            Comment = item.Comment,
                            DbType = MapCsharpType(item.Type),
                            HasDefaultValue = item.Default != null ? true : false,
                            IsIdentity = item.Key == "PRI" ? true : false,
                            IsPrimaryKey = item.Key == "PRI" ? true : false,
                            TableName = table.Name,
                            Name = item.Field,
                            Nullable = item.Null == "YES" ? true : false
                        };

                        if (item.Key == "PRI")
                        {
                            if (table.PrimaryKeyPair.HasValue)
                                throw new InvalidOperationException("最多支持两个列的复合主键!");

                            if (table.PrimaryKey == null)
                            {
                                table.PrimaryKey = ColumData;
                            }
                            else
                            {
                                table.PrimaryKeyPair = new PrimaryPair { Item1 = table.PrimaryKey, Item2 = ColumData };
                                table.PrimaryKey = null;
                            }
                        }
                        if (item.Extra == "auto_increment")
                        {
                            table.Identity = ColumData;
                        }
                        if (!_fail)
                        {
                            table.Columns.Add(ColumData);
                        }
                    }
                    result.Add(table.Name, table);

                    // 打印进度
                    ProgressPrint(++count, TableNames.Count);
                }
            }

            return result;
        }

        protected override string FindDBName(string connStr)
        {
            var db_name = string.Empty;
            var cb = new DbConnectionStringBuilder(false);
            cb.ConnectionString = connStr;
            object database;
            if (cb.TryGetValue("Database", out database))
            {
                db_name = database.ToString();
            }

            return db_name;
        }
        protected override string MapCsharpType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            string csharpType = "object";
            switch (dbtype)
            {
                case "bigint": csharpType = "long"; break;
                case "binary": csharpType = "byte[]"; break;
                case "bool": csharpType = "bool"; break;
                case "boolen": csharpType = "bool"; break;
                case "char": csharpType = "string"; break;
                case "date": csharpType = "DateTime"; break;
                case "datetime": csharpType = "DateTime"; break;
                case "datetimeoffset": csharpType = "DateTimeOffset"; break;
                case "decimal": csharpType = "decimal"; break;
                case "float": csharpType = "float"; break;
                case "double": csharpType = "double"; break;
                case "image": csharpType = "byte[]"; break;
                case "int": csharpType = "int"; break;
                case "nchar": csharpType = "string"; break;
                case "ntext": csharpType = "string"; break;
                case "numeric": csharpType = "decimal"; break;
                case "nvarchar": csharpType = "string"; break;
                case "smallint": csharpType = "short"; break;
                case "sysname": csharpType = "object"; break;
                case "text": csharpType = "string"; break;
                case "time": csharpType = "TimeSpan"; break;
                case "timestamp": csharpType = "byte[]"; break;
                case "tinyint": csharpType = "byte"; break;
                case "varbinary": csharpType = "byte[]"; break;
                case "varchar": csharpType = "string"; break;
                default: csharpType = "object"; break;
            }
            return csharpType;
        }
    }
}
