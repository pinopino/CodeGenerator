using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

        public Parser(string connStr, bool enableProgress = true)
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
                            // Print($"表{name}存在enum和set字段，暂无对应数据结构，跳过此表的生成！");
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
}
