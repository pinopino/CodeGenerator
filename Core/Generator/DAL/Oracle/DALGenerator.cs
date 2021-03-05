﻿using Generator.Common;
using Generator.Core.Config;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.Text;

namespace Generator.Core.Oracle
{
    public class DALGenerator : BaseDALGenerator
    {
        public override string FileName => throw new NotImplementedException();

        public override string EscapeLeft => "\"";

        public override string EscapeRight => "\"";

        public DALGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string GetPartialViewPath(string method)
        {
            switch (method.ToLower())
            {
                case "insert":
                    return "DAL/Insert/insert_oracle.cshtml";
                case "page":
                    return "DAL/Page/page_oracle.cshtml";
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
            return "new OracleConnection()";
        }
    }
}
