using Generator.Core.Config;
using Generator.Core.Inject;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Generator.Core
{
    public abstract class BaseGenerator
    {
        public abstract string FileName { get; }
        protected readonly GlobalConfiguration _config;
        protected readonly Dictionary<string, TableMetaData> _tables;

        public BaseGenerator(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
        {
            _config = config;
            _tables = tables;
        }
    }

    public abstract class BaseGenerator_DAL : BaseGenerator
    {
        private List<string> _keywords = new List<string> { "Type" };

        public BaseGenerator_DAL(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        #region head生成逻辑
        public string Get_Head(TableMetaData table)
        {
            var sb = new StringBuilder();
            sb.Append(_config.DALConfig.HeaderNote);
            sb.AppendLine(string.Join(Environment.NewLine, _config.DALConfig.Using));
            sb.AppendLine();
            sb.AppendLine($"namespace {_config.DALConfig.Namespace}");
            sb.AppendLine("{");
            sb.AppendLine(Get_MetaData1(table.Name));
            sb.AppendLine(string.Format("{0}public partial class {1}{2}{3}{4}",
                    '\t',
                    _config.DALConfig.ClassPrefix,
                    table.Name,
                    _config.DALConfig.ClassSuffix,
                    string.IsNullOrWhiteSpace(_config.DALConfig.BaseClass) ? string.Empty : (" : " + _config.DALConfig.BaseClass)));
            sb.AppendLine(string.Format("{0}{{", '\t'));
            var join_info = _config.JoinedTables == null ? null : _config.JoinedTables.FirstOrDefault(p => p.Table_Main.Name == table.Name);
            if (join_info != null)
            {
                sb.AppendLine(Get_MetaData2(join_info));
            }
            sb.AppendLine(Get_MetaData3(table.Name));

            return sb.ToString();
        }

        // 元数据
        private string Get_MetaData1(string tableName)
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

        private string Get_MetaData2(JoinMapping join_info)
        {
            var subTable = join_info.Table_Sub.Name;
            var columns = _tables[subTable].Columns;
            var sb = new StringBuilder();
            sb.AppendLine("\t\tstatic Dictionary<string, string> col_map = new Dictionary<string, string>");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t// column ==> property");
            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                sb.AppendLine($"\t\t\t{{\"{col.Name}2\", \"{col.Name}\"}}, ");
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
            sb.AppendLine($"\t\tstatic CustomPropertyTypeMap type_map = new CustomPropertyTypeMap(typeof(Joined{join_info.Table_Main.Name}.{subTable}), (t, col) => mapper(t, col));");

            return sb.ToString();
        }

        private string Get_MetaData3(string tableName)
        {
            var table_config = _tables[tableName];
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
                if (IsKeyword(column.Name))
                {
                    sb2.Append($"@{column.Name}, ");
                }
                else
                {
                    sb2.Append($"{column.Name}, ");
                }
            }
            sb1.Append($"\t\t\tpublic static readonly List<{tableName}Column> All = new List<{tableName}Column> {{ ");
            sb1.Append(sb2);
            sb1.AppendLine("};");
            sb1.AppendLine("\t\t}");

            return sb1.ToString();
        }
        #endregion

        #region BaseHelper
        public string Get_BaseTableHelper()
        {
            var str1 = "IDbConnection connection = new SqlConnection(ConnectionString);";

            var sb2 = new StringBuilder();
            sb2.AppendLine("var count_sql = string.Format(\"SELECT COUNT(1) FROM {0}\", tableName);");
            sb2.AppendLine("\t\t\tif (string.IsNullOrWhiteSpace(orderBy))");
            sb2.AppendLine("\t\t\t{");
            sb2.AppendLine("\t\t\t\torderBy = \"id desc\";");
            sb2.AppendLine("\t\t\t}");
            sb2.AppendLine("\t\t\tif (!string.IsNullOrWhiteSpace(where))");
            sb2.AppendLine("\t\t\t{");
            sb2.AppendLine("\t\t\t\tif (where.ToLower().Contains(\"where\"))");
            sb2.AppendLine("\t\t\t\t{");
            sb2.AppendLine("\t\t\t\tthrow new ArgumentException(\"where子句不需要带where关键字\");");
            sb2.AppendLine("\t\t\t\t}");
            sb2.AppendLine("\t\t\t\twhere = \" WHERE \" + where;");
            sb2.AppendLine("\t\t\t}");
            sb2.AppendLine();
            sb2.AppendLine("\t\t\tvar sql = string.Format(\"SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS Row, {0} FROM {2} {3}) AS Paged \", columns, orderBy, tableName, where);");
            sb2.AppendLine("\t\t\tvar pageStart = (currentPage - 1) * pageSize;");
            sb2.AppendLine("\t\t\tsql += string.Format(\" WHERE Row >{0} AND Row <={1}\", pageStart, pageStart + pageSize);");
            sb2.AppendLine("\t\t\tcount_sql += where;");
            sb2.AppendLine("\t\t\tusing (var conn = GetOpenConnection())");
            sb2.AppendLine("\t\t\t{");
            sb2.AppendLine("\t\t\t\tresult.TotalRecords = connection.ExecuteScalar<int>(count_sql);");
            sb2.AppendLine("\t\t\t\tresult.TotalPages = result.TotalRecords / pageSize;");
            sb2.AppendLine("\t\t\t\tif (result.TotalRecords % pageSize > 0)");
            sb2.AppendLine("\t\t\t\t\tresult.TotalPages += 1;");
            sb2.AppendLine("\t\t\t\tvar list = connection.Query<T>(sql);");
            sb2.AppendLine("\t\t\t\tresult.Items = list.Count() == 0 ? (new List<T>()) : list.ToList();");
            sb2.Append("\t\t\t}");

            var str = string.Format(DALTemplate.BASE_TABLE_HELPER_TEMPLATE,
                                    _config.DALConfig.Namespace,
                                    _config.DBConn,
                                    str1,
                                    sb2.ToString());
            return str;
        }

        public string Get_PageDataView()
        {
            var str = string.Format(DALTemplate.PAGE_VIEW_DATA_TEMPLATE,
                                    _config.DALConfig.Namespace);
            return str;
        }
        #endregion

        #region Exists
        public string Get_Exists(string tableName)
        {
            var str1 = Get_Exists1(tableName);
            var str2 = Get_Exists2(tableName);
            return str1 + Environment.NewLine + str2;
        }
        protected abstract string Get_Exists1(string tableName);
        protected abstract string Get_Exists2(string tableName);
        #endregion

        #region Insert
        public abstract string Get_Insert(string tableName);
        #endregion

        #region Delete
        public string Get_Delete(string tableName)
        {
            var str1 = Get_Delete1(tableName);
            var str2 = Get_Delete2(tableName);
            return str1 + Environment.NewLine + str2;
        }
        protected abstract string Get_Delete1(string tableName);
        protected abstract string Get_Delete2(string tableName);
        #endregion

        #region BatchDelete
        public abstract string Get_BatchDelete(string tableName);
        #endregion

        #region Update
        public string Get_Update(string tableName)
        {
            var str1 = Get_Update1(tableName);
            var str2 = Get_Update2(tableName);
            return str2 + Environment.NewLine + str1;
        }
        protected abstract string Get_Update1(string tableName);
        protected abstract string Get_Update2(string tableName);
        #endregion

        #region Select
        public string Get_GetModel(string tableName)
        {
            var str1 = Get_GetModel1(tableName);
            var str2 = Get_GetModel2(tableName);
            return str1 + Environment.NewLine + str2;
        }
        protected abstract string Get_GetModel1(string tableName);
        protected abstract string Get_GetModel2(string tableName);
        public abstract string Get_GetList(string tableName);
        #endregion

        #region Page
        public abstract string Get_Count(string tableName);
        public abstract string Get_GetListByPage(string tableName);
        #endregion

        #region Joined
        public string Get_Joined(JoinMapping join_info)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append(Get_JoinedPage(join_info));
            sb.AppendLine();
            sb.AppendLine(Get_Joined1(join_info));
            sb.AppendLine(Get_Joined2(join_info));
            sb.Append(Get_Joined3(join_info));

            return sb.ToString();
        }

        protected abstract string Get_Joined1(JoinMapping join_info);
        protected abstract string Get_Joined2(JoinMapping join_info);
        protected abstract string Get_Joined3(JoinMapping join_info);
        protected abstract string Get_JoinedPage(JoinMapping join_info);
        #endregion

        protected bool IsKeyword(string colunm)
        {
            return _keywords.Contains(colunm);
        }

        protected string GetReturnStr(string keyType)
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

        protected bool IsUpdateExceptColumn(string table, string colunm)
        {
            if (_config.UpdateExceptColumns == null)
                return false;

            return _config.UpdateExceptColumns.ContainsKey("*") && _config.UpdateExceptColumns["*"].Any(p => p.ColumnName == colunm) ||
                _config.UpdateExceptColumns.ContainsKey(table) && _config.UpdateExceptColumns[table].Any(p => p.ColumnName == colunm);
        }
    }

    public abstract class BaseGenerator_Model : BaseGenerator
    {
        class join_inject : BaseInjector, IModelInjector
        {
            public join_inject(BaseGenerator_Model _g)
                : base(_g._tables, _g._config, false)
            { }

            public override string Name => throw new NotImplementedException();

            public override string Inject(string originContent, string tableName = "", string columnName = "")
            {
                var ret = new StringBuilder();
                using (StringReader sr = new StringReader(originContent))
                {
                    while (sr.Peek() > 0)
                    {
                        var line = sr.ReadLine();
                        line = "\t" + line;
                        ret.AppendLine(line);
                    }
                }

                return ret.ToString();
            }
        }

        public BaseGenerator_Model(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        public virtual string Get_Head(TableMetaData table)
        {
            var sb = new StringBuilder();
            sb.Append(_config.ModelConfig.HeaderNote);
            sb.AppendLine(string.Join(Environment.NewLine, _config.ModelConfig.Using));
            sb.AppendLine();
            sb.AppendLine($"namespace {_config.ModelConfig.Namespace}");
            sb.Append("{");

            return sb.ToString();
        }

        public abstract string Get_Class(TableMetaData table);

        public virtual string Get_Tail(TableMetaData table)
        {
            return "}";
        }

        public virtual string Get_Join_Head(JoinMapping join_info)
        {
            var sb = new StringBuilder();
            sb.Append(_config.ModelConfig.HeaderNote);
            sb.AppendLine(string.Join(Environment.NewLine, _config.ModelConfig.Using));
            sb.AppendLine();
            sb.AppendLine($"namespace {_config.ModelConfig.Namespace}.JoinedViewModel");
            sb.AppendLine("{");

            return sb.ToString();
        }

        // 此虚方法中对于join的实现是用嵌套类来表达的，你如果觉得不满足也可以重载掉
        // 填入自己的逻辑
        public virtual string Get_Joined_Class(JoinMapping join_info)
        {
            var table1 = _tables[join_info.Table_Main.Name];
            var table2 = _tables[join_info.Table_Sub.Name];
            // 生成外层
            var out_class = Get_Class(table1);
            // 生成内层
            //  保证表名正常
            if (join_info.Sub_InnerName != join_info.Table_Sub.Name)
                table2.Name = join_info.Sub_InnerName;
            var inner_class = Get_Class(table2);
            table2.Name = join_info.Table_Sub.Name;
            // 通过逐行更改的方式混合在一起
            var inject = new join_inject(this);
            var ret = inject.InjectHead(out_class, inject.Inject(inner_class));

            return ret;
        }

        public virtual string Get_Join_Tail(JoinMapping join_info)
        {
            return "}";
        }
    }

    public abstract class BaseGenerator_Enum : BaseGenerator
    {
        public BaseGenerator_Enum(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        public virtual string Get_Head(ColumnMetaData column)
        {
            var sb = new StringBuilder();
            sb.Append(_config.ModelConfig.HeaderNote);
            sb.Append(string.Join(Environment.NewLine, _config.ModelConfig.Using));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(string.Format("namespace {0}.{1}", _config.Project, "GenEnum"));
            sb.Append("{");

            return sb.ToString();
        }

        public abstract string Get_Enum(ColumnMetaData column);

        public virtual string Get_Tail(ColumnMetaData column)
        {
            return "}";
        }
    }
}
