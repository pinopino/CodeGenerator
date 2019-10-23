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

        public override string MakeTableName(string rawName)
        {
            return $"[{rawName}]";
        }

        public override string MakeParamComment(List<ColumnMetaData> predicate, int indent = 4)
        {
            var sb = new StringBuilder();
            foreach (var item in predicate)
                sb.AppendLine($"/// <param name=\"{item.Name}\">{item.Comment}</param>".Indent(indent));
            return sb.ToString();
        }

        public override string MakeParamList(List<ColumnMetaData> predicate)
        {
            var sb = new StringBuilder();
            foreach (var item in predicate)
                sb.AppendLine($"{item.DbType} {item.Name}, ");
            return sb.ToString();
        }

        public override string MakeParamValList(List<ColumnMetaData> predicate)
        {
            var sb = new StringBuilder();
            foreach (var item in predicate)
                sb.AppendLine($"@{item.Name}={item.Name}, ");
            return sb.ToString();
        }

        public override string MakeWhere(List<ColumnMetaData> predicate)
        {
            var sb = new StringBuilder();
            foreach (var item in predicate)
                sb.AppendLine($"[{item.Name}]=@{item.Name} and ");
            return sb.ToString();
        }
    }
}
