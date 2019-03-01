using Generator.Common;
using Generator.Template;
using System;
using System.Text;

namespace Generator.Core
{
    public class DALGenerator
    {
        private SQLMetaData _config;

        public DALGenerator(SQLMetaData config)
        {
            this._config = config;
        }

        #region Exists
        public string Get_Exists(string tableName)
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

            var str = string.Format(DALTemplate.EXISTS_TEMPLATE,
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));
            return str;
        }
        #endregion

        #region Insert
        public string Get_Insert(string tableName)
        {
            var table_config = _config[tableName];
            if (table_config.PrimaryKey.Count == 1)
            {
                return Get_Insert_SingleId(tableName);
            }
            else
            {
                return Get_Insert_MultipleId(tableName);
            }
        }

        private string Get_Insert_SingleId(string tableName)
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
                                    table_config.Comment,
                                    table_config.Comment,
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

        private string Get_Insert_MultipleId(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            sb1.Append("Tuple<");
            table_config.PrimaryKey.ForEach(p => sb1.Append(string.Format("{0}, ", p.DbType)));
            sb1.Append(">");

            var sb2 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    sb2.Append(string.Format("[{0}], ", p.Name));
                }
            });

            var sb3 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb3.Append(string.Format("INSERTED.[{0}], ", p.Name)));

            var sb4 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    sb4.Append(string.Format("@{0}, ", p.Name));
                }
            });

            var sb5 = sb1.ToString().Replace(", >", "") + ">";

            var sb6 = sb1.ToString().Replace(", >", "") + ">";

            var sb7 = sb1.ToString().Replace(", >", "") + ">";

            var str = string.Format(DALTemplate.INSERT_TEMPLATE2,
                                    table_config.Comment,
                                    table_config.Comment,
                                    sb1.ToString().Replace(", >", "") + ">",
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    sb3.ToString().TrimEnd(", ".ToCharArray()),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()),
                                    sb5,
                                    sb6,
                                    sb7);

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
            var table_config = _config[tableName];
            return Get_Delete1(tableName);
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
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }
        #endregion

        #region BatchDelete
        public string Get_BatchDelete(string tableName)
        {
            var table_config = _config[tableName];
            if (table_config.PrimaryKey.Count == 1)
            {
                return Get_BatchDelete_SingleId(tableName);
            }
            else
            {
                return Get_BatchDelete_MultipleId(tableName);
            }
        }

        private string Get_BatchDelete_SingleId(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey[0];

            var str = string.Format(DALTemplate.BATCHDELETE_TEMPLATE1,
                                    table_config.Comment,
                                    table_config.Comment,
                                    primaryKey.DbType,
                                    string.Format("[{0}]", tableName),
                                    string.Format("[{0}]", primaryKey.Name));

            return str;
        }

        private string Get_BatchDelete_MultipleId(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            sb1.Append("Tuple<");
            table_config.PrimaryKey.ForEach(p => sb1.Append(string.Format("{0}, ", p.DbType)));
            sb1.Append(">");

            var sb2 = new StringBuilder();
            sb2.Append("(");
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("[{0}], ", p.Name)));
            sb2.Append(")");

            var str = string.Format(DALTemplate.BATCHDELETE_TEMPLATE2,
                                    table_config.Comment,
                                    table_config.Comment,
                                    sb1.ToString().Replace(", >", "") + ">",
                                    string.Format("[{0}]", tableName),
                                    sb2.ToString().Replace(", )", "") + ")");

            return str;
        }
        #endregion

        #region Update
        public string Get_Update(string tableName)
        {
            var table_config = _config[tableName];
            return Get_Update1(tableName);
        }

        private string Get_Update1(string tableName)
        {
            var table_config = _config[tableName];
            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity && !IsExceptColumn(tableName, p.Name))
                    sb1.Append(string.Format("[{0}]=@{1}, ", p.Name, p.Name));
            });

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("[{0}]=@{1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.UPDATE_TEMPLATE1,
                                    table_config.Comment,
                                    table_config.Comment,
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()));

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
            var table_config = _config[tableName];
            return Get_GetModel1(tableName);
        }

        private string Get_GetModel1(string tableName)
        {
            var table_config = _config[tableName];
            var trace = _config.TraceFieldTables.Contains("*") || _config.TraceFieldTables.Contains(tableName);

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
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    tableName,
                                    tableName,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    sb3.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb4.ToString().TrimEnd("and "),
                                    tableName,
                                    tableName,
                                    sb5.ToString().TrimEnd(", ".ToCharArray()),
                                    trace ? "ret.OpenTrace();" + Environment.NewLine : string.Empty);

            return str;
        }

        public string Get_GetList(string tableName)
        {
            var table_config = _config[tableName];
            return Get_GetList1(tableName);
        }

        private string Get_GetList1(string tableName)
        {
            var table_config = _config[tableName];
            var trace = _config.TraceFieldTables.Contains("*") || _config.TraceFieldTables.Contains(tableName);

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p => sb1.Append(string.Format("[{0}], ", p.Name)));

            var sb2 = new StringBuilder();
            if (trace)
            {
                sb2.AppendLine("foreach (var item in ret)");
                sb2.AppendLine("\t\t\t{");
                sb2.AppendLine("\t\t\t\titem.OpenTrace();");
                sb2.AppendLine("\t\t\t}");
            }

            var str = string.Format(DALTemplate.GET_LIST_TEMPLATE1,
                                    table_config.Comment,
                                    tableName,
                                    tableName,
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    tableName,
                                    tableName,
                                    sb2.ToString());

            return str;
        }
        #endregion

        #region Page
        public string Get_Count(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.GET_COUNT_TEMPLATE,
                                    string.Format("[{0}]", tableName));

            return str;
        }

        public string Get_GetListByPage(string tableName)
        {
            var table_config = _config[tableName];
            var str = string.Format(DALTemplate.GET_LIST_BY_PAGE_TEMPLATE,
                                    tableName,
                                    tableName,
                                    tableName);

            return str;
        }
        #endregion
    }
}
