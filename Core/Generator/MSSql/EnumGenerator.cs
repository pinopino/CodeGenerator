using Generator.Template;
using System.Text;

namespace Generator.Core.MSSql
{
    public class EnumGenerator
    {
        private SQLMetaData _config;

        public EnumGenerator(SQLMetaData config)
        {
            this._config = config;
        }

        public string Get_Enum(string enumName, string comment, string[] values, string type)
        {
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
