using Generator.Core.Config;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.Text;

namespace Generator.Core.MSSql
{
    public class ModelGenerator : BaseGenerator_Model
    {
        public ModelGenerator(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
            : base(config, tables)
        { }

        public override string Get_Class(TableMetaData table)
        {
            var sb1 = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var p = table.Columns[i];
                if (p.Nullable && p.DbType != "string")
                {
                    sb1.AppendLine(string.Format("{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
                else
                {
                    sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
            }

            var sb2 = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var p = table.Columns[i];
                sb2.AppendLine(string.Format("{0}{0}/// <summary>", '\t'));
                sb2.AppendLine(string.Format("{0}{0}/// {1}", '\t', p.Comment));
                sb2.AppendLine(string.Format("{0}{0}/// </summary>", '\t'));
                if (p.Nullable && p.DbType != "string")
                {
                    sb2.AppendLine(string.Format("{0}{0}public {1}? {2}", '\t', p.DbType, p.Name));
                }
                else
                {
                    sb2.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', p.DbType, p.Name));
                }
                sb2.AppendLine(string.Format("{0}{0}{{", '\t'));
                sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; }}", '\t', p.Name.ToLower()));
                sb2.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                sb2.AppendLine(string.Format("{0}{0}}}", '\t'));
                sb2.AppendLine();
            }

            var str = string.Format(ModelTemplate.CLASS,
                                    table.Name,
                                    table.Comment,
                                    _config.ModelConfig.ClassPrefix,
                                    table.Name,
                                    _config.ModelConfig.ClassSuffix,
                                    string.IsNullOrWhiteSpace(_config.ModelConfig.BaseClass) ? string.Empty : (" : " + _config.ModelConfig.BaseClass),
                                    table.Name,
                                    sb1.ToString(),
                                    sb2.ToString());
            return str;
        }
    }
}
