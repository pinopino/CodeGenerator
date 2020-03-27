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

        public override string MakeTableName(string rawName)
        {
            return $"[{rawName}]";
        }

        public override string MakeMethodParam(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"{item.DbType} {item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeMethodParamComment(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"/// <param name=\"{item.Name}\">{item.Comment}</param>");
            return sb.ToString();
        }

        public override string MakeSQLParamList(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
            {
                if (!item.IsIdentity)
                    sb.Append($"@{item.Name}, ");
            }
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeSQLParamValueList(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"@{item.Name}={item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeSQLWhere(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"[{item.Name}]=@{item.Name} and ");
            return sb.TrimEnd("and ").ToString();
        }
    }
}
