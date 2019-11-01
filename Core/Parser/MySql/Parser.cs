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
                    var table = new TableMetaData
                    {
                        Name = name,
                        Columns = new List<ColumnMetaData>(),
                        Comment = "",
                        Identity = new ColumnMetaData(),
                        PrimaryKey = new List<ColumnMetaData>()
                    };

                    CommandText = $"SHOW FULL FIELDS FROM {name};"; // 得到表结构
                    var fields = connection.Query<FieldViewModel>(CommandText).ToList();
                    bool _fail = false;
                    foreach (var item in fields)
                    {
                        item.Type = item.Type.Substring(0, item.Type.IndexOf("("));
                        if (item.Type.Any(p => p.Equals("enum") || p.Equals("set")))
                        {
                            _fail = true;
                            Console.WriteLine($"表{name}存在enum和set字段，暂无对应数据结构，跳过此表的生成！");
                            break;
                        }
                        var ColumData = new ColumnMetaData
                        {
                            Comment = item.Comment,
                            DbType = OutputHelper.MapCsharpType(item.Type),
                            HasDefaultValue = item.Default != null ? true : false,
                            IsIdentity = item.Key == "PRI" ? true : false,
                            IsPrimaryKey = item.Key == "PRI" ? true : false,
                            TableName = table.Name,
                            Name = item.Field,
                            Nullable = item.Null == "YES" ? true : false
                        };

                        if (item.Key == "PRI")
                        {
                            table.PrimaryKey.Add(ColumData);
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
    }
}
