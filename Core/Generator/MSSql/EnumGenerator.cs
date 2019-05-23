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
        private Regex _regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public EnumGenerator(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        public override bool Validate(string str, out string comment)
        {
            comment = string.Empty;
            var match = _regex.Match(str);
            if (match.Success)
            {
                comment = match.Value.Replace("：", " ").Replace("、", " ").Replace("。", " ").Replace("；", " ").Replace(".", " ").Replace(";", " ").Replace(":", " ");
            }
            return match.Success;
        }

        public override string Get_Enum(string enumName, string comment, string type)
        {
            var values = comment.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var sb1 = new StringBuilder();
            for (int i = 0; i < values.Length; i += 2)
            {
                var is_number = int.TryParse(values[i], out _);
                if (i + 2 == values.Length)
                {
                    sb1.AppendLine("\t\t/// <summary>");
                    sb1.AppendLine($"\t\t/// {values[i + 1]} {values[i]}");
                    sb1.AppendLine("\t\t/// </summary>");
                    sb1.Append($"\t\tpublic static readonly {type} {values[i + 1]} = {(is_number ? values[i] : "\"" + values[i] + "\"")};");
                }
                else
                {
                    if (i == 0)
                    {
                        sb1.AppendLine("\t\t/// <summary>");
                        sb1.AppendLine($"\t\t/// {values[i + 1]} {values[i]}");
                        sb1.AppendLine("\t\t/// </summary>");
                        sb1.AppendLine($"\t\tpublic static readonly {type} {values[i + 1]} = {(is_number ? values[i] : "\"" + values[i] + "\"")};");
                    }
                    else
                    {
                        sb1.AppendLine("\t\t/// <summary>");
                        sb1.AppendLine($"\t\t/// {values[i + 1]} {values[i]}");
                        sb1.AppendLine("\t\t/// </summary>");
                        sb1.AppendLine($"\t\tpublic static readonly {type} {values[i + 1]} = {(is_number ? values[i] : "\"" + values[i] + "\"")};");
                    }
                }
            }

            var str = string.Format(EnumTemplate.Enum_TEMPLATE,
                                    enumName,
                                    comment,
                                    enumName,
                                    sb1.ToString());
            return str;
        }
    }
}
