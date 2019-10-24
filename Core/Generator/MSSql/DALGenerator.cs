using Generator.Common;
using Generator.Core.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Generator.Core.MSSql
{
    public class DALGenerator : BaseGenerator_DAL
    {
        public override string FileName => throw new NotImplementedException();

        public DALGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string GetPartialViewPath(string method)
        {
            if (method.ToLower() == "insert")
                return "DAL/Insert/insert_mssql.cshtml";
            else
                return base.GetPartialViewPath(method);
        }

        public override string MakeTableName(string rawName)
        {
            return $"[{rawName}]";
        }

        public override string MakeParamComment(List<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"/// <param name=\"{item.Name}\">{item.Comment}</param>");
            return sb.ToString();
        }

        public override string MakeMethodParam(List<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"{item.DbType} {item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeParamList(List<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
            {
                if (!item.IsIdentity)
                    sb.Append($"@{item.Name}, ");
            }
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeParamValList(List<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"@{item.Name}={item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeWhere(List<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"[{item.Name}]=@{item.Name} and ");
            return sb.TrimEnd("and ").ToString();
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
    }
}
