using Generator.Core.Config;
using Generator.Template;
using System;
using System.Collections.Generic;

namespace Generator.Core.MySql
{
    public class DALGenerator : BaseDALGenerator
    {
        public override string FileName => throw new NotImplementedException();

        public DALGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string GetPartialViewPath(string method)
        {
            switch (method.ToLower())
            {
                case "insert":
                    return "DAL/Insert/inser_mysql.cshtml";
                case "getmodel":
                case "getlist":
                case "getcount":
                    return "DAL/GetModel/get_mysql.cshtml";
                case "getpage":
                    return "DAL/Page/page.cshtml";
                default:
                    return base.GetPartialViewPath(method);
            }
        }

        public override string RenderBaseTableHelper()
        {
            throw new NotImplementedException();
        }

        public override string MakeTableName(string rawName)
        {
            throw new NotImplementedException();
        }

        public override string MakeMethodParam(IEnumerable<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }

        public override string MakeMethodParamComment(IEnumerable<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }

        public override string MakeSQLParamList(IEnumerable<ColumnMetaData> columns)
        {
            throw new NotImplementedException();
        }

        public override string MakeSQLParamValueList(IEnumerable<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }

        public override string MakeSQLWhere(IEnumerable<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
