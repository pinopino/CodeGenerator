using Generator.Core;
using Generator.Core.Config;
using System.Collections.Generic;

namespace Generator.Template
{
    public class EnumInfo
    {
        public string EnumName;
        public string Comment;
        public string[] Values;
        public string DbType;
    }

    public class ViewInfoWapper
    {
        private BaseGenerator _generator;
        public GlobalConfiguration Config;
        public TableMetaData TableInfo;
        public EnumInfo EnumInfo;

        public ViewInfoWapper(BaseGenerator generator)
        {
            _generator = generator;
        }

        public string GetPartialViewPath(string method)
        {
            return ((BaseGenerator_DAL)_generator).GetPartialViewPath(method);
        }

        public string MakeTableName(string rawName)
        {
            return ((BaseGenerator_DAL)_generator).MakeTableName(rawName);
        }

        public string MakeParamComment(List<ColumnMetaData> columns)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamComment(columns);
        }

        public string MakeMethodParam(List<ColumnMetaData> columns)
        {
            return ((BaseGenerator_DAL)_generator).MakeMethodParam(columns);
        }

        public string MakeParamList(List<ColumnMetaData> columns)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamList(columns);
        }

        public string MakeParamValList(List<ColumnMetaData> columns)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamValList(columns);
        }

        public string MakeWhere(List<ColumnMetaData> columns)
        {
            return ((BaseGenerator_DAL)_generator).MakeWhere(columns);
        }

        public string MakeConnectionInit()
        {
            return ((BaseGenerator_DAL)_generator).MakeConnectionInit();
        }

        public string MakeGetOpenConnection()
        {
            return ((BaseGenerator_DAL)_generator).MakeGetOpenConnection();
        }

        public string MakeBasePaging()
        {
            return ((BaseGenerator_DAL)_generator).MakeBasePaging();
        }
    }
}
