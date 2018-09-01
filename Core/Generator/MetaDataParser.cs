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
        private Dictionary<string, Dictionary<string, string>> _tableRef;
        private List<TableMetaData> _tableNeedCheck;
        private Regex _k_reg = new Regex("FK[:|：].+", RegexOptions.Compiled);
        private Regex _t_reg = new Regex("FT[:|：].+", RegexOptions.Compiled);

        public MetaDataParser(SQLMetaData metaData)
        {
            if (metaData == null)
            {
                throw new ArgumentNullException("metaData");
            }

            _config = metaData;
            _tableRef = new Dictionary<string, Dictionary<string, string>>();
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

            // 单独解析剩余的外键关系
            if (_tableNeedCheck.Count > 0)
            {
                foreach (var item in _tableNeedCheck)
                {
                    foreach (var col in item.Columns)
                    {
                        var foreignTable = string.Empty;
                        if (ParseSpecialComment_Table(col.Comment, out foreignTable))
                        {
                            var source_table = _config.Tables.Find(p => p.Name == foreignTable);
                            item.IsRefed = true;
                            var ref_meta = new RefTableMetaData();
                            ref_meta.Name = item.Name;
                            ref_meta.ForeignKey.Add(new ForeignKeyMetaData { FromName = _tableRef[col.Name][foreignTable], ToName = col.Name, DbType = col.DbType.ToString() });
                            source_table.ReferenceTable.Add(ref_meta);
                        }
                    }
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

            // 外键
            var foreignKey = string.Empty;
            if (column.IsForeignKey)
            {
                // todo: 暂不处理这种情况
            }
            else
            {
                if (_tableRef.ContainsKey(column.Name))
                {
                    // 目前有两种规则，分别是FK和FT
                    // FK，表示当前列在其它表中作为“外键”时的名字
                    // FT，表示通过当前列所关联到的“主表”的名字，又有两种可能：
                    //      FT:table_name1, table_name2, ...，表示所关联到的多张表每张表的名字
                    //      FT:*，表示只要跟当前列名一致，即为相关联的表
                    var foreignTable = string.Empty;
                    if (ParseSpecialComment_Table(meta_col.Comment, out foreignTable))
                    {
                        var source_tables = new List<TableMetaData>();
                        if (foreignTable == "*")
                        {
                            source_tables.AddRange(_config.Tables.Where(p => _tableRef[column.Name].ContainsKey(p.Name)));
                        }
                        else
                        {
                            var data = foreignTable.Replace('，', ',').Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(p => _config.Tables.Find(k => k.Name == p));
                            source_tables.AddRange(data);
                        }
                        foreach(var item in source_tables)
                        {
                            var tmp = item.ReferenceTable.Find(p => p.Name == metaTable.Name);
                            if (tmp != null)
                            {
                                tmp.ForeignKey.Add(new ForeignKeyMetaData { FromName = _tableRef[column.Name][item.Name], ToName = column.Name, DbType = column.DataType.ToString() });
                            }
                            else
                            {
                                metaTable.IsRefed = true;
                                var ref_meta = new RefTableMetaData();
                                ref_meta.Name = metaTable.Name;
                                ref_meta.ForeignKey.Add(new ForeignKeyMetaData { FromName = _tableRef[column.Name][item.Name], ToName = column.Name, DbType = column.DataType.ToString() });
                                item.ReferenceTable.Add(ref_meta);
                                _tableNeedCheck.Remove(metaTable);
                            }
                        }
                    }
                }
                else
                {
                    if (ParseSpecialComment_Key(meta_col.Comment, out foreignKey))
                    {
                        var dict = new Dictionary<string, string>();
                        if (_tableRef.TryGetValue(foreignKey, out dict))
                        {
                            dict.Add(metaTable.Name, column.Name);
                        }
                        else
                        {
                            _tableRef.Add(foreignKey, new Dictionary<string, string> { { metaTable.Name, column.Name } });
                        }
                        _tableNeedCheck.Remove(metaTable);
                    }
                }
            }
        }

        private bool ParseSpecialComment_Key(string comment, out string foreignKey)
        {
            // 目前有两种规则，分别是FK和FT
            // FK，表示当前列在其它表中作为“外键”时的名字
            // FT，表示通过当前列所关联到的“主表”的名字，又有两种可能：
            //      FT:table_name1, table_name2, ...，表示所关联到的多张表每张表的名字
            //      FT:*，表示只要跟当前列名一致，即为相关联的表
            foreignKey = string.Empty;
            if (string.IsNullOrWhiteSpace(comment))
            {
                return false;
            }

            var k_match = _k_reg.Match(comment);
            if (k_match.Success)
            {
                foreignKey = k_match.Value.Replace("FK：", "FK:").Replace("FK:", "");
                return true;
            }

            return false;
        }

        private bool ParseSpecialComment_Table(string comment, out string foreignTable)
        {
            foreignTable = string.Empty;
            if (string.IsNullOrWhiteSpace(comment))
            {
                return false;
            }

            var t_match = _t_reg.Match(comment);
            if (t_match.Success)
            {
                foreignTable = t_match.Value.Replace("FT：", "FT:").Replace("FT:", "");
                return true;
            }

            return false;
        }

        private void ProgressPrint(ConsoleProgressBar progress, long index, long total)
        {
            progress.Dispaly(Convert.ToInt32((index / (total * 1.0)) * 100));
        }
    }
}
