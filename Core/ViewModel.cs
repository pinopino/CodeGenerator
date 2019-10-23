using Generator.Core;
using Generator.Core.Config;
using System.Collections.Generic;

namespace Generator.Template
{
    public class ViewInfoWapper
    {
        private BaseGenerator _generator;
        public GlobalConfiguration Config;
        public TableMetaData TableInfo;

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

        public string MakeParamComment(List<ColumnMetaData> predicate, int indent = 4)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamComment(predicate, indent);
        }

        public string MakeParamList(List<ColumnMetaData> predicate)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamList(predicate);
        }

        public string MakeParamValList(List<ColumnMetaData> predicate)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamValList(predicate);
        }

        public string MakeWhere(List<ColumnMetaData> predicate)
        {
            return ((BaseGenerator_DAL)_generator).MakeWhere(predicate);
        }
    }
}
