using Generator.Core.Config;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public override string Get_Joined_Class(JoinMapping join_info)
        {
            var mainTable = join_info.Table_Main.Name;
            var subTable = join_info.Table_Sub.Name;
            var alias = join_info.Sub_InnerName;

            var sb1 = new StringBuilder();
            var table_config = _tables[mainTable];
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                if (p.Nullable && p.DbType != "string")
                {
                    sb1.AppendLine(string.Format("{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
                else
                {
                    sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
            }
            sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', subTable, subTable.ToLower()));

            var sb2 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
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
            }

            table_config = _tables[subTable];
            var sb3 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                if (p.Nullable && p.DbType != "string")
                {
                    sb3.AppendLine(string.Format("{0}{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
                else
                {
                    sb3.AppendLine(string.Format("{0}{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
            }

            var sb4 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                sb4.AppendLine(string.Format("{0}{0}{0}/// <summary>", '\t'));
                sb4.AppendLine(string.Format("{0}{0}{0}/// {1}", '\t', p.Comment));
                sb4.AppendLine(string.Format("{0}{0}{0}/// </summary>", '\t'));
                if (p.Nullable && p.DbType != "string")
                {
                    sb4.AppendLine(string.Format("{0}{0}{0}public {1}? {2}", '\t', p.DbType, p.Name));
                }
                else
                {
                    sb4.AppendLine(string.Format("{0}{0}{0}public {1} {2}", '\t', p.DbType, p.Name));
                }
                sb4.AppendLine(string.Format("{0}{0}{0}{{", '\t'));
                sb4.AppendLine(string.Format("{0}{0}{0}{0}set {{ _{1} = value; }}", '\t', p.Name.ToLower()));
                sb4.AppendLine(string.Format("{0}{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                sb4.AppendLine(string.Format("{0}{0}{0}}}", '\t'));
            }

            var str = string.Format(ModelTemplate.JOINED_CLASS,
                                    "Joined" + mainTable,
                                    "Joined" + mainTable,
                                    subTable,
                                    sb3.ToString(),
                                    sb4.ToString(),
                                    Environment.NewLine + sb1.ToString(),
                                    sb2.ToString());
            return str;
        }
    }
}
