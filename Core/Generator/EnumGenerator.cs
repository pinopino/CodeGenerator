using Generator.Template;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Generator.Core
{
    public class EnumGenerator
    {
        private SQLMetaData _config;

        public EnumGenerator(SQLMetaData config)
        {
            this._config = config;
        }

        public string Get_Enum(string enumName, string[] values)
        {
            var sb1 = new StringBuilder();
            for (int i = 0; i < values.Length; i += 2)
            {
                if (i + 2 == values.Length)
                {
                    sb1.Append("\t\t" + values[i + 1] + " = " + values[i]);
                }
                else
                {
                    if (i == 0)
                    {
                        sb1.AppendLine(values[i + 1] + " = " + values[i] + ",");
                    }
                    else
                    {
                        sb1.AppendLine("\t\t" + values[i + 1] + " = " + values[i] + ",");
                    }
                }
            }

            var str = string.Format(EnumTemplate.Enum_TEMPLATE,
                                    enumName,
                                    enumName,
                                    sb1.ToString());
            return str;
        }
    }
}
