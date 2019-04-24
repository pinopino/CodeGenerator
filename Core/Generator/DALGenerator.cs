﻿using Generator.Common;
using Generator.Template;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Core
{
    public class DALGenerator
    {
        private SQLMetaData _config;
        private List<string> _keywords = new List<string> { "Type" };

        public DALGenerator(SQLMetaData config)
        {
            this._config = config;
        }

        #region 元数据
        public string Get_MetaData1(string tableName)
        {
            var sb1 = new StringBuilder();
            sb1.AppendLine($"\tnamespace Metadata");
            sb1.AppendLine("\t{");
            sb1.AppendLine($"\t\tpublic sealed class {tableName}Column : IColumn");
            sb1.AppendLine("\t\t{");
            sb1.AppendLine($"\t\t\tinternal {tableName}Column(string table, string name)");
            sb1.AppendLine("\t\t\t{");
            sb1.AppendLine("\t\t\t\tTable = table;");
            sb1.AppendLine("\t\t\t\tName = name;");
            sb1.AppendLine("\t\t\t}");
            sb1.AppendLine();
            sb1.AppendLine("\t\t\tpublic string Name { private set; get; }");
            sb1.AppendLine();
            sb1.AppendLine("\t\t\tpublic string Table { private set; get; }");
            sb1.AppendLine();
            sb1.AppendLine("\t\t\tpublic bool IsAddEqual { private set; get; }");
            sb1.AppendLine();
            sb1.AppendLine("\t\t\tprivate bool _asc;");
            sb1.AppendLine("\t\t\tpublic string Asc { get { return this._asc ? \"ASC\" : \"DESC\"; } }");
            sb1.AppendLine();
            sb1.AppendLine($"\t\t\tpublic {tableName}Column SetAddEqual() {{ IsAddEqual ^= true; return this; }}");
            sb1.AppendLine();
            sb1.AppendLine($"\t\t\tpublic {tableName}Column SetOrderByAsc() {{ this._asc = true; return this; }}");
            sb1.AppendLine();
            sb1.AppendLine($"\t\t\tpublic {tableName}Column SetOrderByDesc() {{ this._asc = false; return this; }}");
            sb1.AppendLine("\t\t}");
            sb1.AppendLine();

            sb1.AppendLine($"\t\tpublic sealed class {tableName}Table");
            sb1.AppendLine("\t\t{");
            sb1.AppendLine($"\t\t\tinternal {tableName}Table(string name)");
            sb1.AppendLine("\t\t\t{");
            sb1.AppendLine("\t\t\t\tName = name;");
            sb1.AppendLine("\t\t\t}");
            sb1.AppendLine();
            sb1.AppendLine("\t\t\tpublic string Name { private set; get; }");
            sb1.AppendLine("\t\t}");
            sb1.AppendLine("\t}");

            return sb1.ToString();
        }

        public string Get_MetaData2(string mainTable, Tuple<string, string> subInfo)
        {
            var subTable = subInfo.Item1;
            var columns = _config[subTable].Columns;
            var sb = new StringBuilder();
            sb.AppendLine("\t\tstatic Dictionary<string, string> col_map = new Dictionary<string, string>");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t// column ==> property");
            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                if (i == columns.Count - 1)
                {
                    sb.AppendLine($"\t\t\t{{\"{col.Name}2\", \"{col.Name}\"}}");
                }
                else
                {
                    sb.AppendLine($"\t\t\t{{\"{col.Name}2\", \"{col.Name}\"}}, ");
                }
            }
            sb.AppendLine("\t\t};");
            sb.AppendLine();
            sb.AppendLine("\t\tstatic Func<Type, string, System.Reflection.PropertyInfo> mapper = (t, col) =>");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif (col_map.ContainsKey(col))");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn t.GetProperty(col_map[col]);");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\telse");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn t.GetProperty(col);");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t};");
            sb.AppendLine();
            sb.AppendLine($"\t\tstatic CustomPropertyTypeMap type_map = new CustomPropertyTypeMap(typeof(Joined{mainTable}.{subTable}), (t, col) => mapper(t, col));");

            return sb.ToString();
        }

        public string Get_MetaData3(string tableName)
        {
            var table_config = _config[tableName];
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            sb1.AppendLine($"\t\tpublic static readonly {tableName}Table Table = new {tableName}Table(\"{tableName}\");");
            sb1.AppendLine();
            sb1.AppendLine("\t\tpublic sealed class Columns");
            sb1.AppendLine("\t\t{");
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var column = table_config.Columns[i];
                sb1.AppendLine($"\t\t\tpublic static readonly {tableName}Column {column.Name} = new {tableName}Column(\"{tableName}\", \"{column.Name}\");");
                if (i == table_config.Columns.Count - 1)
                {
                    if (IsKeyword(column.Name))
                    {
                        sb2.Append($"@{column.Name} ");
                    }
                    else
                    {
                        sb2.Append($"{column.Name} ");
                    }
                }
                else
                {
                    if (IsKeyword(column.Name))
                    {
                        sb2.Append($"@{column.Name}, ");
                    }
                    else
                    {
                        sb2.Append($"{column.Name}, ");
                    }
                }
            }
            sb1.Append($"\t\t\tpublic static readonly List<{tableName}Column> All = new List<{tableName}Column> {{ ");
            sb1.Append(sb2);
            sb1.AppendLine("};");
            sb1.AppendLine("\t\t}");

            return sb1.ToString();
        }

        private bool IsKeyword(string colunm)
        {
            return _keywords.Contains(colunm);
        }
        #endregion

        #region Exists
        public string Get_Exists(string tableName)
        {
            var str1 = Get_Exists1(tableName);
            var str2 = Get_Exists2(tableName);
            return str1 + Environment.NewLine + str2;
        }

        private string Get_Exists1(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.ExistPredicate.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table_config.ExistPredicate.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table_config.ExistPredicate.ForEach(p => sb3.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb4 = new StringBuilder();
            table_config.ExistPredicate.ForEach(p => sb4.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.EXISTS_TEMPLATE1,
                                    tableName + "实体对象",
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));
            return str;
        }

        private string Get_Exists2(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.EXISTS_TEMPLATE2,
                                    tableName + "实体对象",
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }
        #endregion

        #region Insert
        public string Get_Insert(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey[0];

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    sb1.Append(string.Format("[{0}], ", p.Name));
                }
            });

            var sb2 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    sb2.Append(string.Format("@{0}, ", p.Name));
                }
            });

            var str = string.Format(DALTemplate.INSERT_TEMPLATE1,
                                    $"新{tableName}记录",
                                    tableName + "实体对象",
                                    primaryKey.DbType,
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    primaryKey.Name,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    GetReturnStr(primaryKey.DbType),
                                    primaryKey.DbType,
                                    primaryKey.DbType);

            return str;
        }

        private string GetReturnStr(string keyType)
        {
            switch (keyType.ToLower())
            {
                case "int":
                    {
                        return "0";
                    }
                case "long":
                    {
                        return "0L";
                    }
                case "string":
                    {
                        return "string.Empty";
                    }
            }

            return string.Empty;
        }
        #endregion

        #region Delete
        public string Get_Delete(string tableName)
        {
            var str1 = Get_Delete1(tableName);
            var str2 = Get_Delete2(tableName);
            return str1 + Environment.NewLine + str2;
        }

        private string Get_Delete1(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb3.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb4 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb4.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.DELETE_TEMPLATE1,
                                    tableName + "数据记录",
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    $"[{tableName}]",
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }

        private string Get_Delete2(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.DELETE_TEMPLATE2,
                                    tableName + "数据记录",
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }
        #endregion

        #region BatchDelete
        public string Get_BatchDelete(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey[0];

            var str = string.Format(DALTemplate.BATCHDELETE_TEMPLATE1,
                                    tableName + "数据记录",
                                    tableName + "实体对象的",
                                    primaryKey.DbType,
                                    $"[{tableName}]",
                                    string.Format("[{0}]", primaryKey.Name));

            return str;
        }
        #endregion

        #region Update
        public string Get_Update(string tableName)
        {
            var str1 = Get_Update1(tableName);
            var str2 = Get_Update2(tableName);
            return str2 + Environment.NewLine + str1;
        }

        private string Get_Update1(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb1.Append(string.Format("[{0}] = @{1}, ", p.Name, p.Name)));

            var sb2 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity && !IsExceptColumn(tableName, p.Name))
                    sb2.Append(string.Format("[{0}] = @{1}, ", p.Name, p.Name));
            });

            var str = string.Format(DALTemplate.UPDATE_TEMPLATE1,
                                    tableName + "数据记录",
                                    tableName + "实体对象",
                                    tableName,
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()));
            return str;
        }

        private string Get_Update2(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.UPDATE_TEMPLATE2,
                                    tableName + "数据记录",
                                    tableName + "实体对象",
                                    tableName);
            return str;
        }

        private bool IsExceptColumn(string table, string colunm)
        {
            return _config.ExceptColumns.ContainsKey("*") && _config.ExceptColumns["*"].Contains(colunm) ||
                _config.ExceptColumns.ContainsKey(table) && _config.ExceptColumns[table].Contains(colunm);
        }
        #endregion

        #region Select
        public string Get_GetModel(string tableName)
        {
            var str1 = Get_GetModel1(tableName);
            var str2 = Get_GetModel2(tableName);
            return str1 + Environment.NewLine + str2;
        }

        private string Get_GetModel1(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table_config.Columns.ForEach(p => sb3.Append(string.Format("[{0}], ", p.Name)));

            var sb4 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb4.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb5 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb5.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.GET_MODEL_TEMPLATE1,
                                    tableName + "实体对象",
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    tableName,
                                    tableName,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    sb3.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb4.ToString().TrimEnd("and "),
                                    tableName,
                                    tableName,
                                    sb5.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }

        private string Get_GetModel2(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.GET_MODEL_TEMPLATE2,
                                    tableName + "实体对象",
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }

        public string Get_GetList(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.GET_LIST_TEMPLATE1,
                                    tableName + "实体对象",
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }
        #endregion

        #region Page
        public string Get_Count(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.GET_COUNT_TEMPLATE,
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }

        public string Get_GetListByPage(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.GET_LIST_BY_PAGE_TEMPLATE,
                                    tableName);
            return str;
        }
        #endregion

        #region Joined
        public string Get_Joined(string mainTable, Tuple<string, string> subInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append(Get_JoinedPage(mainTable, subInfo));
            sb.AppendLine();
            sb.AppendLine(Get_Joined1(mainTable, subInfo));
            sb.AppendLine(Get_Joined2(mainTable, subInfo));
            sb.Append(Get_Joined3(mainTable, subInfo));

            return sb.ToString();
        }

        private string Get_Joined1(string mainTable, Tuple<string, string> subInfo)
        {
            var subTable = subInfo.Item1;
            var alias = subInfo.Item2;
            var str = string.Format(DALTemplate.INNER_JOIN_TEMPLATE,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable,
                                    alias);
            return str;
        }

        private string Get_Joined2(string mainTable, Tuple<string, string> subInfo)
        {
            var subTable = subInfo.Item1;
            var alias = subInfo.Item2;
            var str = string.Format(DALTemplate.LEFT_JOIN_TEMPLATE,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable,
                                    alias);
            return str;
        }

        private string Get_Joined3(string mainTable, Tuple<string, string> subInfo)
        {
            var subTable = subInfo.Item1;
            var alias = subInfo.Item2;
            var str = string.Format(DALTemplate.RIGHT_JOIN_TEMPLATE,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable,
                                    alias);
            return str;
        }

        private string Get_JoinedPage(string mainTable, Tuple<string, string> subInfo)
        {
            var subTable = subInfo.Item1;
            var alias = subInfo.Item2;
            var str1 = string.Format(DALTemplate.JOINED_PAGE_TEMPLATE1,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable);

            var str2 = string.Format(DALTemplate.JOINED_PAGE_TEMPLATE2,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable);

            var str3 = string.Format(DALTemplate.JOINED_PAGE_TEMPLATE3,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable);

            var str4 = string.Format(DALTemplate.JOINED_PAGE_TEMPLATE4,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable,
                                    alias);

            return str1 + Environment.NewLine + str2 + Environment.NewLine + str3 + Environment.NewLine + str4;
        }
        #endregion
    }
}
