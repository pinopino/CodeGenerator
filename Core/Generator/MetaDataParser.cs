using Generator.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Generator.Core
{
    public class MetaDataParser
    {
        private SQLMetaData _config;
        private List<TableMetaData> _tableNeedCheck;        

        public MetaDataParser(SQLMetaData metaData)
        {
            if (metaData == null)
            {
                throw new ArgumentNullException("metaData");
            }

            _config = metaData;
            _tableNeedCheck = new List<TableMetaData>();
        }

        public void Parse(TableCollection data, bool enableProgress = true)
        {
            ConsoleProgressBar progress = null;
            if (enableProgress)
            {
                progress = new ConsoleProgressBar(Console.CursorLeft, Console.CursorTop, 50, ProgressBarType.Character);
            }

            // 解析表
            var i = 0;
            for (i = 0; i < data.Count;)
            {
                var table = data[i];
                var meta_table = ParseTable(table);
                _tableNeedCheck.Add(meta_table);
                // 解析行
                foreach (Column col in table.Columns)
                {
                    ParseColumn(meta_table, col);
                }
                _config.Tables.Add(meta_table);

                // 打印进度
                if (progress != null)
                {
                    ProgressPrint(progress, ++i, data.Count + 1);
                }
            }

            // 打印进度
            ProgressPrint(progress, ++i, data.Count + 1);
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

        private void ProgressPrint(ConsoleProgressBar progress, long index, long total)
        {
            progress.Dispaly(Convert.ToInt32((index / (total * 1.0)) * 100));
        }
    }
}
