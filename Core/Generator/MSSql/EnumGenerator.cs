using Generator.Core.Config;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Generator.Core.MSSql
{
    public class EnumGenerator : BaseGenerator_Enum
    {
        private string _enum_name;
        public override string FileName { get { return this._enum_name; } }

        public EnumGenerator(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        public override string Get_Enum(ColumnMetaData column)
        {
            var enum_str = GetStrFromComment(column);
            var tempname = Regex.Replace(column.TableName, @"\d", "").Replace("_", "");
            _enum_name = string.Format("{0}_{1}_{2}", tempname, column.Name, "Enum");

            var sb = new StringBuilder();
            var arrs = enum_str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrs.Length; i += 2)
            {
                var is_number = int.TryParse(arrs[i], out _);
                sb.AppendLine("\t\t/// <summary>");
                sb.AppendLine($"\t\t/// {arrs[i + 1]} {arrs[i]}");
                sb.AppendLine("\t\t/// </summary>");
                sb.AppendLine($"\t\tpublic static readonly {column.DbType} {arrs[i + 1]} = {(is_number ? arrs[i] : "\"" + arrs[i] + "\"")};");
            }

            var str = string.Format(EnumTemplate.Enum_TEMPLATE,
                                    _enum_name,
                                    enum_str,
                                    _enum_name,
                                    sb.ToString());
            return str;
        }

        private string GetStrFromComment(ColumnMetaData column)
        {
            var enumStr = string.Empty;
            var _regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase);
            var match = _regex.Match(column.Comment);
            if (match.Success)
            {
                enumStr = match.Value.Replace("：", " ").Replace("、", " ").Replace("。", " ").Replace("；", " ").Replace(".", " ").Replace(";", " ").Replace(":", " ");
            }

            return enumStr;
        }
    }
}
