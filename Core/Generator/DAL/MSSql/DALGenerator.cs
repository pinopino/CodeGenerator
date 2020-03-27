using Generator.Common;
using Generator.Core.Config;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.Text;

namespace Generator.Core.MSSql
{
    public class DALGenerator : BaseDALGenerator
    {
        public override string FileName => throw new NotImplementedException();

        public DALGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string GetPartialViewPath(string method)
        {
            if (method.ToLower() == "insert")
                return "DAL/Insert/insert_mssql.cshtml";
            else if (method.ToLower() == "update")
                return "DAL/Update/update_mssql.cshtml";
            else
                return base.GetPartialViewPath(method);
        }

        public override string RenderBaseTableHelper()
        {
            var model = new ViewInfoWapper(this);
            model.Config = _config;

            return Render("DAL/BaseTable/basetablehelper_mssql.cshtml", model);
        }

        public override string NormalizeTableName(string tableName)
        {
            return $"[{tableName}]";
        }

        public override string NormalizeFieldName(string fieldName)
        {
            return $"[{fieldName}]";
        }
    }
}
