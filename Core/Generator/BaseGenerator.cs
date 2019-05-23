using Generator.Core.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Generator.Core
{
    public class BaseGenerator
    {
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

        #region 元数据
        public abstract string Get_MetaData1(string tableName);
        public abstract string Get_MetaData2(JoinMapping join_info);
        public abstract string Get_MetaData3(string tableName);
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
        public BaseGenerator_Model(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        public abstract string Get_Class(string tableName);

        public abstract string Get_Joined_Class(JoinMapping join_info);

        public abstract string Get_Entity_Class(string tableName);
    }

    public abstract class BaseGenerator_Enum : BaseGenerator
    {
        public BaseGenerator_Enum(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        public abstract string Get_Enum(string enumName, string comment, string[] values, string type);
    }
}
