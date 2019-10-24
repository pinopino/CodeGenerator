using Generator.Common;
using Generator.Core.Config;
using Generator.Template;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generator.Core.MySql
{
    public class DALGenerator : BaseGenerator_DAL
    {
        public override string FileName => throw new NotImplementedException();

        public DALGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string GetPartialViewPath(string method)
        {
            throw new NotImplementedException();
        }

        public override string MakeTableName(string rawName)
        {
            throw new NotImplementedException();
        }

        public override string MakeParamComment(List<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }

        public override string MakeMethodParam(List<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }

        public override string MakeParamList(List<ColumnMetaData> columns)
        {
            throw new NotImplementedException();
        }

        public override string MakeParamValList(List<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }

        public override string MakeWhere(List<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
