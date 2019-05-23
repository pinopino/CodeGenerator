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
        public EnumGenerator(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        public override string Get_Enum(string enumName, string enumStr, ColumnMetaData column)
        {
            var sb = new StringBuilder();
            var arrs = enumStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrs.Length; i += 2)
            {
                var is_number = int.TryParse(arrs[i], out _);
                if (i + 2 == arrs.Length)
                {
                    sb.AppendLine("\t\t/// <summary>");
                    sb.AppendLine($"\t\t/// {arrs[i + 1]} {arrs[i]}");
                    sb.AppendLine("\t\t/// </summary>");
                    sb.Append($"\t\tpublic static readonly {column.DbType} {arrs[i + 1]} = {(is_number ? arrs[i] : "\"" + arrs[i] + "\"")};");
                }
                else
                {
                    if (i == 0)
                    {
                        sb.AppendLine("\t\t/// <summary>");
                        sb.AppendLine($"\t\t/// {arrs[i + 1]} {arrs[i]}");
                        sb.AppendLine("\t\t/// </summary>");
                        sb.AppendLine($"\t\tpublic static readonly {column.DbType} {arrs[i + 1]} = {(is_number ? arrs[i] : "\"" + arrs[i] + "\"")};");
                    }
                    else
                    {
                        sb.AppendLine("\t\t/// <summary>");
                        sb.AppendLine($"\t\t/// {arrs[i + 1]} {arrs[i]}");
                        sb.AppendLine("\t\t/// </summary>");
                        sb.AppendLine($"\t\tpublic static readonly {column.DbType} {arrs[i + 1]} = {(is_number ? arrs[i] : "\"" + arrs[i] + "\"")};");
                    }
                }
            }

            var str = string.Format(EnumTemplate.Enum_TEMPLATE,
                                    enumName,
                                    enumStr,
                                    enumName,
                                    sb.ToString());
            return str;
        }
    }
}
