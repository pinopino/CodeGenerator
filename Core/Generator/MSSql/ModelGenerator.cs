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

        public override string Get_Class_With_Trace(TableMetaData table)
        {
            var trace = _config.TraceFieldTables == null ? false :
                _config.TraceFieldTables.Any(p => p.Name == "*") || _config.TraceFieldTables.Any(p => p.Name == table.Name);
            if (trace == false)
                throw new InvalidOperationException("要生成的表并没有开启trace属性");

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var sb3 = new StringBuilder();
            var sb4 = new StringBuilder();
            var sb5 = new StringBuilder();
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

                
                sb2.AppendLine(string.Format("{0}{0}[Newtonsoft.Json.JsonProperty] private volatile int _ver_{1};", '\t', p.Name.ToLower()));

                sb3.AppendLine(string.Format("{0}{0}/// <summary>", '\t'));
                sb3.AppendLine(string.Format("{0}{0}/// {1}", '\t', p.Comment));
                sb3.AppendLine(string.Format("{0}{0}/// </summary>", '\t'));
                if (p.Nullable && p.DbType != "string")
                {
                    sb3.AppendLine(string.Format("{0}{0}public {1}? {2}", '\t', p.DbType, p.Name));
                }
                else
                {
                    sb3.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', p.DbType, p.Name));
                }
                sb3.AppendLine(string.Format("{0}{0}{{", '\t'));
                sb3.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; {2}}}", '\t', p.Name.ToLower(), $"if (_____flag) System.Threading.Interlocked.Increment(ref _ver_{p.Name.ToLower()}); "));
                sb3.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                sb3.AppendLine(string.Format("{0}{0}}}", '\t'));
                sb3.AppendLine();

                sb4.AppendLine($"\t\t\tif (_ver_{p.Name.ToLower()} != 0)");
                sb4.AppendLine("\t\t\t{");
                sb4.AppendLine($"\t\t\t\tupdate_fields.Add({table.Name}Helper.Columns.{p.Name});");
                sb4.AppendLine($"\t\t\t\tif (!preserve) _ver_{ p.Name.ToLower()} = 0;");
                sb4.AppendLine("\t\t\t}");
                
                sb5.AppendLine($"\t\t\t_ver_{p.Name.ToLower()} = 0;");
            }

            sb4.AppendLine();
            // GetTraceFields
            sb4.AppendLine();
            sb4.AppendLine($"\t\tpublic IList<{table.Name}Column> GetTraceFields(bool preserve = false)");
            sb4.AppendLine("\t\t{");
            sb4.AppendLine($"\t\t\tvar update_fields = new List<{table.Name}Column>();");
            sb4.AppendLine(sb3.ToString());
            sb4.AppendLine("\t\t\treturn update_fields;");
            sb4.AppendLine("\t\t}");
            // OpenTrace
            sb4.AppendLine();
            sb4.AppendLine("\t\tpublic void BeginTrace()");
            sb4.AppendLine("\t\t{");
            sb4.AppendLine("\t\t\t_____flag = true;");
            sb4.AppendLine("\t\t}");
            sb4.AppendLine();
            // CloseTrace
            sb4.AppendLine("\t\tpublic void EndTrace()");
            sb4.AppendLine("\t\t{");
            sb4.AppendLine("\t\t\t_____flag = false;");
            sb4.AppendLine("\t\t}");
            // ResetTrace
            sb4.AppendLine();
            sb4.AppendLine("\t\tpublic void ResetTrace()");
            sb4.AppendLine("\t\t{");
            sb4.Append(sb5.ToString());
            sb4.AppendLine("\t\t\t_____flag = false;");
            sb4.Append("\t\t}");

            var str = string.Format(ModelTemplate.CLASS_WITH_TRACE,
                                    table.Name,
                                    table.Comment,
                                    _config.ModelConfig.ClassPrefix,
                                    table.Name,
                                    _config.ModelConfig.ClassSuffix,
                                    string.IsNullOrWhiteSpace(_config.ModelConfig.BaseClass) ? string.Empty : (" : " + _config.ModelConfig.BaseClass),
                                    table.Name,
                                    sb1.ToString(),
                                    sb2.ToString(),
                                    sb3.ToString());
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
                if (i == table_config.Columns.Count - 1)
                {
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
                    sb2.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', subTable, alias));
                    sb2.AppendLine(string.Format("{0}{0}{{", '\t'));
                    sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; }}", '\t', subTable.ToLower()));
                    sb2.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', subTable.ToLower()));
                    sb2.Append(string.Format("{0}{0}}}", '\t'));
                }
                else
                {
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
            }

            table_config = _tables[subTable];
            var sb3 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                if (i == table_config.Columns.Count - 1)
                {
                    if (p.Nullable && p.DbType != "string")
                    {
                        sb3.Append(string.Format("{0}{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                    else
                    {
                        sb3.Append(string.Format("{0}{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                }
                else
                {
                    if (p.Nullable && p.DbType != "string")
                    {
                        sb3.AppendLine(string.Format("{0}{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                    else
                    {
                        sb3.AppendLine(string.Format("{0}{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                }
            }
            sb3.AppendLine();

            var sb4 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                if (i == table_config.Columns.Count - 1)
                {
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
                    sb4.Append(string.Format("{0}{0}{0}}}", '\t'));
                }
                else
                {
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
                    sb4.AppendLine();
                }
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

        public override string Get_Entity_Class(TableMetaData table)
        {
            var table_config = _tables[table.Name];
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var sb3 = new StringBuilder();
            var sb5 = new StringBuilder();
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

                sb2.AppendLine(string.Format("{0}{0}private int _ver_{1};", '\t', p.Name.ToLower()));
                sb3.AppendLine($"\t\t\tif (_ver_{p.Name.ToLower()} != 0)");
                sb3.AppendLine("\t\t\t{");
                sb3.AppendLine($"\t\t\t\tinfo.Add(\"{p.Name}\", _{p.Name.ToLower()});");
                sb5.AppendLine($"\t\t\t_ver_{p.Name.ToLower()} = 0;");
                sb3.AppendLine("\t\t\t}");
            }

            var sb4 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                if (i == table_config.Columns.Count - 1)
                {
                    sb4.AppendLine(string.Format("{0}{0}/// <summary>", '\t'));
                    sb4.AppendLine(string.Format("{0}{0}/// {1}", '\t', p.Comment));
                    sb4.AppendLine(string.Format("{0}{0}/// </summary>", '\t'));
                    if (p.Nullable && p.DbType != "string")
                    {
                        sb4.AppendLine(string.Format("{0}{0}public {1}? {2}", '\t', p.DbType, p.Name));
                    }
                    else
                    {
                        sb4.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', p.DbType, p.Name));
                    }
                    sb4.AppendLine(string.Format("{0}{0}{{", '\t'));
                    sb4.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; {2}}}", '\t', p.Name.ToLower(), $"_ver_{p.Name.ToLower()}++; "));
                    sb4.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                    sb4.AppendLine(string.Format("{0}{0}}}", '\t'));
                }
                else
                {
                    sb4.AppendLine(string.Format("{0}{0}/// <summary>", '\t'));
                    sb4.AppendLine(string.Format("{0}{0}/// {1}", '\t', p.Comment));
                    sb4.AppendLine(string.Format("{0}{0}/// </summary>", '\t'));
                    if (p.Nullable && p.DbType != "string")
                    {
                        sb4.AppendLine(string.Format("{0}{0}public {1}? {2}", '\t', p.DbType, p.Name));
                    }
                    else
                    {
                        sb4.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', p.DbType, p.Name));
                    }
                    sb4.AppendLine(string.Format("{0}{0}{{", '\t'));
                    sb4.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; {2}}}", '\t', p.Name.ToLower(), $"_ver_{p.Name.ToLower()}++; "));
                    sb4.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                    sb4.AppendLine(string.Format("{0}{0}}}", '\t'));
                    sb4.AppendLine();
                }
            }
            // GetTraceFields
            sb4.AppendLine();
            sb4.AppendLine($"\t\tpublic Dictionary<string, object> GetTracedInfo()");
            sb4.AppendLine("\t\t{");
            sb4.AppendLine("\t\t\tvar info = new Dictionary<string, object>();");
            sb4.AppendLine(sb3.ToString());
            sb4.AppendLine("\t\t\treturn info;");
            sb4.Append("\t\t}");

            var str = string.Format(ModelTemplate.ENTITY_CLASS,
                                    table.Name,
                                    table_config.Comment,
                                    _config.ModelConfig.ClassPrefix,
                                    table.Name,
                                    _config.ModelConfig.ClassSuffix,
                                    string.IsNullOrWhiteSpace(_config.ModelConfig.BaseClass) ? string.Empty : (" : " + _config.ModelConfig.BaseClass),
                                    table.Name,
                                    Environment.NewLine + sb1.ToString(),
                                    sb2.ToString(),
                                    sb4.ToString());
            return str;
        }
    }
}
