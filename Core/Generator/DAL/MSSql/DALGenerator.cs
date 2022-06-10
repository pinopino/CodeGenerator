using Generator.Core.Config;
using System;

namespace Generator.Core.MSSql
{
    public class DALGenerator : BaseDALGenerator
    {
        public override string FileName => throw new NotImplementedException();

        public override string EscapeLeft => "[";

        public override string EscapeRight => "]";

        public DALGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string GetPartialViewPath(string method)
        {
            switch (method.ToLower())
            {
                case "insert":
                    return "DAL/Insert/insert_mssql.cshtml";
                case "page":
                    return "DAL/Page/page_mssql.cshtml";
                default:
                    return base.GetPartialViewPath(method);
            }
        }

        public override string NormalizeTableName(string tableName)
        {
            return $"{EscapeLeft}{tableName}{EscapeRight}";
        }

        public override string NormalizeFieldName(string fieldName)
        {
            return $"{EscapeLeft}{fieldName}{EscapeRight}";
        }

        public override string MakeConnection()
        {
            return "new SqlConnection()";
        }

        public override string MakeSqlUsing()
        {
            return string.Empty;
        }
    }
}
