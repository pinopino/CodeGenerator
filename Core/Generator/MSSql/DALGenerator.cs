using Generator.Common;
using Generator.Core.Config;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.Text;

namespace Generator.Core.MSSql
{
    public class DALGenerator : BaseGenerator_DAL
    {
        public override string FileName => throw new NotImplementedException();

        public DALGenerator(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        #region Exists
        protected override string Get_Exists1(string tableName)
        {
            var table_config = _tables[tableName];

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

        protected override string Get_Exists2(string tableName)
        {
            var table_config = _tables[tableName];
            var str = string.Format(DALTemplate.EXISTS_TEMPLATE2,
                                    tableName + "实体对象",
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }
        #endregion

        #region Insert
        public override string Get_Insert(string tableName)
        {
            var table_config = _tables[tableName];
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
        #endregion

        #region Delete
        protected override string Get_Delete1(string tableName)
        {
            var table_config = _tables[tableName];

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

        protected override string Get_Delete2(string tableName)
        {
            var table_config = _tables[tableName];
            var str = string.Format(DALTemplate.DELETE_TEMPLATE2,
                                    tableName + "数据记录",
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }
        #endregion

        #region BatchDelete
        public override string Get_BatchDelete(string tableName)
        {
            var table_config = _tables[tableName];
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
        protected override string Get_Update1(string tableName)
        {
            var table_config = _tables[tableName];

            var sb1 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb1.Append(string.Format("[{0}] = @{1}, ", p.Name, p.Name)));

            var sb2 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity && !IsUpdateExceptColumn(tableName, p.Name))
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

        protected override string Get_Update2(string tableName)
        {
            var table_config = _tables[tableName];
            var str = string.Format(DALTemplate.UPDATE_TEMPLATE2,
                                    tableName + "数据记录",
                                    tableName + "实体对象",
                                    tableName);
            return str;
        }
        #endregion

        #region Select
        protected override string Get_GetModel1(string tableName)
        {
            var table_config = _tables[tableName];

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

        protected override string Get_GetModel2(string tableName)
        {
            var table_config = _tables[tableName];
            var str = string.Format(DALTemplate.GET_MODEL_TEMPLATE2,
                                    tableName + "实体对象",
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }

        public override string Get_GetList(string tableName)
        {
            var table_config = _tables[tableName];
            var str = string.Format(DALTemplate.GET_LIST_TEMPLATE1,
                                    tableName + "实体对象",
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }
        #endregion

        #region Page
        public override string Get_Count(string tableName)
        {
            var table_config = _tables[tableName];
            var str = string.Format(DALTemplate.GET_COUNT_TEMPLATE,
                                    tableName,
                                    $"[{tableName}]");
            return str;
        }

        public override string Get_GetListByPage(string tableName)
        {
            var table_config = _tables[tableName];
            var str = string.Format(DALTemplate.GET_LIST_BY_PAGE_TEMPLATE,
                                    tableName);
            return str;
        }
        #endregion

        #region Joined
        protected override string Get_Joined1(JoinMapping join_info)
        {
            var mainTable = join_info.Table_Main.Name;
            var subTable = join_info.Table_Sub.Name;
            var alias = join_info.Sub_InnerName;
            var str = string.Format(DALTemplate.INNER_JOIN_TEMPLATE,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable,
                                    alias);
            return str;
        }

        protected override string Get_Joined2(JoinMapping join_info)
        {
            var mainTable = join_info.Table_Main.Name;
            var subTable = join_info.Table_Sub.Name;
            var alias = join_info.Sub_InnerName;
            var str = string.Format(DALTemplate.LEFT_JOIN_TEMPLATE,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable,
                                    alias);
            return str;
        }

        protected override string Get_Joined3(JoinMapping join_info)
        {
            var mainTable = join_info.Table_Main.Name;
            var subTable = join_info.Table_Sub.Name;
            var alias = join_info.Sub_InnerName;
            var str = string.Format(DALTemplate.RIGHT_JOIN_TEMPLATE,
                                    $"Joined{mainTable}",
                                    mainTable,
                                    subTable,
                                    alias);
            return str;
        }

        protected override string Get_JoinedPage(JoinMapping join_info)
        {
            var mainTable = join_info.Table_Main.Name;
            var subTable = join_info.Table_Sub.Name;
            var alias = join_info.Sub_InnerName;
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
