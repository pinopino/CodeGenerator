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
        public override string Get_Exists(TableMetaData table)
        {
            var str1 = Get_Exists1(table);
            var str2 = Get_Exists2(table);
            return str1 + Environment.NewLine + str2;
        }

        private string Get_Exists1(TableMetaData table)
        {
            var sb1 = new StringBuilder();
            table.ExistPredicate.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table.ExistPredicate.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table.ExistPredicate.ForEach(p => sb3.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb4 = new StringBuilder();
            table.ExistPredicate.ForEach(p => sb4.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.EXISTS_TEMPLATE1,
                                    table.Name + "实体对象",
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", table.Name),
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));
            return str;
        }

        private string Get_Exists2(TableMetaData table)
        {
            var str = string.Format(DALTemplate.EXISTS_TEMPLATE2,
                                    table.Name + "实体对象",
                                    table.Name,
                                    $"[{table.Name}]");
            return str;
        }
        #endregion

        #region Insert
        public override string Get_Insert(TableMetaData table)
        {
            var primaryKey = table.PrimaryKey[0];
            var sb1 = new StringBuilder();
            table.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    sb1.Append(string.Format("[{0}], ", p.Name));
                }
            });

            var sb2 = new StringBuilder();
            table.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    sb2.Append(string.Format("@{0}, ", p.Name));
                }
            });

            var str = string.Format(DALTemplate.INSERT_TEMPLATE1,
                                    $"新{table.Name}记录",
                                    table.Name + "实体对象",
                                    primaryKey.DbType,
                                    table.Name,
                                    string.Format("[{0}]", table.Name),
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
        public override string Get_Delete(TableMetaData table)
        {
            var str1 = Get_Delete1(table);
            var str2 = Get_Delete2(table);
            return str1 + Environment.NewLine + str2;
        }

        private string Get_Delete1(TableMetaData table)
        {
            var sb1 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb3.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb4 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb4.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.DELETE_TEMPLATE1,
                                    table.Name + "数据记录",
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    $"[{table.Name}]",
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }

        private string Get_Delete2(TableMetaData table)
        {
            var str = string.Format(DALTemplate.DELETE_TEMPLATE2,
                                    table.Name + "数据记录",
                                    table.Name,
                                    $"[{table.Name}]");
            return str;
        }
        #endregion

        #region BatchDelete
        public override string Get_BatchDelete(TableMetaData table)
        {
            var primaryKey = table.PrimaryKey[0];

            var str = string.Format(DALTemplate.BATCHDELETE_TEMPLATE1,
                                    table.Name + "数据记录",
                                    table.Name + "实体对象的",
                                    primaryKey.DbType,
                                    $"[{table.Name}]",
                                    string.Format("[{0}]", primaryKey.Name));

            return str;
        }
        #endregion

        #region Update
        public override string Get_Update(TableMetaData table)
        {
            var str1 = Get_Update1(table);
            var str2 = Get_Update2(table);
            return str2 + Environment.NewLine + str1;
        }

        private string Get_Update1(TableMetaData table)
        {
            var sb1 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb1.Append(string.Format("[{0}] = @{1}, ", p.Name, p.Name)));

            var sb2 = new StringBuilder();
            table.Columns.ForEach(p =>
            {
                if (!p.IsIdentity && !IsUpdateExceptColumn(table.Name, p.Name))
                    sb2.Append(string.Format("[{0}] = @{1}, ", p.Name, p.Name));
            });

            var str = string.Format(DALTemplate.UPDATE_TEMPLATE1,
                                    table.Name + "数据记录",
                                    table.Name + "实体对象",
                                    table.Name,
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()));
            return str;
        }

        private string Get_Update2(TableMetaData table)
        {
            var str = string.Format(DALTemplate.UPDATE_TEMPLATE2,
                                    table.Name + "数据记录",
                                    table.Name + "实体对象",
                                    table.Name);
            return str;
        }
        #endregion

        #region Select
        public override string Get_GetModel(TableMetaData table)
        {
            var str1 = Get_GetModel1(table);
            var str2 = Get_GetModel2(table);
            return str1 + Environment.NewLine + str2;
        }

        private string Get_GetModel1(TableMetaData table)
        {
            var sb1 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table.Columns.ForEach(p => sb3.Append(string.Format("[{0}], ", p.Name)));

            var sb4 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb4.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb5 = new StringBuilder();
            table.PrimaryKey.ForEach(p => sb5.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.GET_MODEL_TEMPLATE1,
                                    table.Name + "实体对象",
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    table.Name,
                                    table.Name,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    sb3.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", table.Name),
                                    sb4.ToString().TrimEnd("and "),
                                    table.Name,
                                    table.Name,
                                    sb5.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }

        private string Get_GetModel2(TableMetaData table)
        {
            var str = string.Format(DALTemplate.GET_MODEL_TEMPLATE2,
                                    table.Name + "实体对象",
                                    table.Name,
                                    $"[{table.Name}]");
            return str;
        }

        public override string Get_GetList(TableMetaData table)
        {
            var str = string.Format(DALTemplate.GET_LIST_TEMPLATE1,
                                    table.Name + "实体对象",
                                    table.Name,
                                    $"[{table.Name}]");
            return str;
        }
        #endregion

        #region Page
        public override string Get_Count(TableMetaData table)
        {
            var str = string.Format(DALTemplate.GET_COUNT_TEMPLATE,
                                    table.Name,
                                    $"[{table.Name}]");
            return str;
        }

        public override string Get_GetListByPage(TableMetaData table)
        {
            var str = string.Format(DALTemplate.GET_LIST_BY_PAGE_TEMPLATE,
                                    table.Name);
            return str;
        }
        #endregion

        #region Joined
        public override string Get_Joined(JoinMapping join_info)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append(Get_JoinedPage(join_info));
            sb.AppendLine();
            sb.AppendLine(Get_Inner_Joined(join_info));
            sb.AppendLine(Get_Left_Joined(join_info));
            sb.Append(Get_Right_Joined(join_info));

            return sb.ToString();
        }

        private string Get_Inner_Joined(JoinMapping join_info)
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

        private string Get_Left_Joined(JoinMapping join_info)
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

        private string Get_Right_Joined(JoinMapping join_info)
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

        private string Get_JoinedPage(JoinMapping join_info)
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
