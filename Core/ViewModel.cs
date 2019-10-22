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

        public string MakeParamComment(List<ColumnMetaData> predicate, int indent = 4)
        {
            return _generator.MakeParamComment(predicate, indent);
        }

        public string MakeParamList(List<ColumnMetaData> predicate)
        {
            return _generator.MakeParamList(predicate);
        }

        public string MakeParamValList(List<ColumnMetaData> predicate)
        {
            return _generator.MakeParamValList(predicate);
        }

        public string MakeWhere(List<ColumnMetaData> predicate)
        {
            return _generator.MakeWhere(predicate);
        }
    }
}
