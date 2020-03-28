using Generator.Common;
using Generator.Core.Config;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Generator.Core.MSSql
{
    public class Parser : BaseParser
    {
        public Parser(GlobalConfiguration config, IProgressBar progress)
            : base(config, progress)
        { }

        public override Dictionary<string, TableMetaData> ParseMetadata()
        {
            using (SQLServerManagement manage = new SQLServerManagement(base.ConnStr))
            {
                var db_key = FindDBName(base.ConnStr);
                var tables = manage.Databases[db_key].Tables;

                var ret = new Dictionary<string, TableMetaData>();
                // 解析表
                for (var i = 0; i < tables.Count;)
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
                    ProgressPrint(++i, tables.Count);
                }

                return ret;
            }
        }

        private TableMetaData ParseTable(Table table)
        {
            var name = table.Name;
            var comment = table.ExtendedProperties["MS_Description"]?.Value.ToString().Trim();

            return new TableMetaData(name, comment);
        }

        private void ParseColumn(TableMetaData metaTable, Column column)
        {
            var meta_col = new ColumnMetaData();
            meta_col.TableName = metaTable.Name;
            meta_col.Name = column.Name;
            meta_col.DbType = OutputHelper.MapCsharpType(column.DataType.ToString());
            meta_col.Comment = column.ExtendedProperties["MS_Description"]?.Value.ToString().Trim();
            meta_col.IsPrimaryKey = column.InPrimaryKey;
            meta_col.IsIdentity = column.Identity;
            meta_col.Nullable = column.Nullable;
            meta_col.HasDefaultValue = column.DefaultConstraint != null && !string.IsNullOrWhiteSpace(column.DefaultConstraint.Text);
            metaTable.Columns.Add(meta_col);

            // 主键
            if (column.InPrimaryKey)
            {
                if (metaTable.PrimaryKeyPair.HasValue)
                    throw new InvalidOperationException("最多支持两个列的复合主键!");

                if (metaTable.PrimaryKey == null)
                {
                    metaTable.PrimaryKey = meta_col;
                }
                else
                {
                    metaTable.PrimaryKeyPair = new PrimaryPair { Item1 = metaTable.PrimaryKey, Item2 = meta_col };
                    metaTable.PrimaryKey = null;
                }
            }

            // 标识
            if (column.Identity)
                metaTable.Identity = meta_col;
        }

        protected override string FindDBName(string connStr)
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
}
