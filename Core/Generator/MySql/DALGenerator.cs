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

        public override string MakeParamList(IEnumerable<ColumnMetaData> columns)
        {
            throw new NotImplementedException();
        }

        public override string MakeParamValueList(IEnumerable<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }

        public override string MakeWhere(IEnumerable<ColumnMetaData> predicate)
        {
            throw new NotImplementedException();
        }

        public override string MakeConnectionInit()
        {
            throw new NotImplementedException();
        }

        public override string MakeGetOpenConnection()
        {
            throw new NotImplementedException();
        }

        public override string MakeBasePaging()
        {
            throw new NotImplementedException();
        }
        public override string MakeBaseParseExpression()
        {
            return Render("DAL/Base/BaseParseExpression.cshtml", new ViewInfoWapper(this) {EnumInfo= new Template.EnumInfo { EnumName = "`", Comment = "`" } });
        }
    }
}
