using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;

namespace Generator.Core.MSSql
{
    public class Parser : BaseParser
    {
        public Parser(string connStr, bool enableProgress = true)
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
}
