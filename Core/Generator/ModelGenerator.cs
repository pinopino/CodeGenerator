using Generator.Template;
using System;
using System.Text;

namespace Generator.Core
{
    public class ModelGenerator
    {
        private SQLMetaData _config;

        public ModelGenerator(SQLMetaData config)
        {
            this._config = config;
        }

        public string Get_Class(string tableName)
        {
            var table_config = _config[tableName];
            var trace = _config.TraceFieldTables.Contains("*") || _config.TraceFieldTables.Contains(tableName);

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var sb3 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                if (i == table_config.Columns.Count - 1)
                {
                    if (p.Nullable && p.DbType != "string")
                    {
                        sb1.Append(string.Format("{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                    else
                    {
                        sb1.Append(string.Format("{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                }
                else
                {
                    if (p.Nullable && p.DbType != "string")
                    {
                        sb1.AppendLine(string.Format("{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                    else
                    {
                        sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                }

                if (trace)
                {
                    sb2.AppendLine(string.Format("{0}{0}[Newtonsoft.Json.JsonProperty] private volatile int _ver_{1};", '\t', p.Name.ToLower()));
                    sb3.AppendLine($"\t\t\tif (_ver_{p.Name.ToLower()} != 0)");
                    sb3.AppendLine("\t\t\t{");
                    sb3.AppendLine("\t\t\t\tupdate_fields.Add(\"" + p.Name + "\");");
                    sb3.AppendLine($"\t\t\t\t_ver_{ p.Name.ToLower()} = 0;");
                    sb3.AppendLine("\t\t\t}");
                }
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
                    sb4.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; {2}}}", '\t', p.Name.ToLower(), trace ? $"if (_____flag) System.Threading.Interlocked.Increment(ref _ver_{p.Name.ToLower()}); " : string.Empty));
                    sb4.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                    sb4.Append(string.Format("{0}{0}}}", '\t'));
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
                    sb4.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; {2}}}", '\t', p.Name.ToLower(), trace ? $"if (_____flag) System.Threading.Interlocked.Increment(ref _ver_{p.Name.ToLower()}); " : string.Empty));
                    sb4.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                    sb4.AppendLine(string.Format("{0}{0}}}", '\t'));
                    sb4.AppendLine();
                }
            }

            if (trace)
            {
                sb4.AppendLine();
                // GetTraceFields
                sb4.AppendLine();
                sb4.AppendLine("\t\tpublic System.Collections.ObjectModel.ReadOnlyCollection<string> GetTraceFields()");
                sb4.AppendLine("\t\t{");
                sb4.AppendLine("\t\t\tvar update_fields = new List<string>();");
                sb4.AppendLine(sb3.ToString());
                sb4.AppendLine("\t\t\treturn update_fields.AsReadOnly();");
                sb4.AppendLine("\t\t}");
                // OpenTrace
                sb4.AppendLine();
                sb4.AppendLine("\t\tinternal void OpenTrace()");
                sb4.AppendLine("\t\t{");
                sb4.AppendLine("\t\t\t_____flag = true;");
                sb4.AppendLine("\t\t}");
                sb4.AppendLine();
                // CloseTrace
                sb4.AppendLine("\t\tinternal void CloseTrace()");
                sb4.AppendLine("\t\t{");
                sb4.AppendLine("\t\t\t_____flag = false;");
                sb4.Append("\t\t}");
            }

            var str = string.Format(ModelTemplate.CLASS_TEMPLATE,
                                    tableName,
                                    table_config.Comment,
                                    _config.Model_ClassNamePrefix,
                                    tableName,
                                    _config.Model_ClassNameSuffix,
                                    string.IsNullOrWhiteSpace(_config.Model_BaseClass) ? string.Empty : (" : " + _config.Model_BaseClass),
                                    tableName,
                                    Environment.NewLine + sb1.ToString(),
                                    trace ? Environment.NewLine + "\t\tprivate bool _____flag;" + Environment.NewLine + sb2.ToString() : string.Empty,
                                    sb4.ToString());
            return str;
        }

        public string Get_Joined_Class(string mainTable, string subTable)
        {
            var table_config = _config[mainTable];
            var sb1 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                if (i == table_config.Columns.Count - 1)
                {
                    if (p.Nullable && p.DbType != "string")
                    {
                        sb1.Append(string.Format("{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                    else
                    {
                        sb1.Append(string.Format("{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                }
                else
                {
                    if (p.Nullable && p.DbType != "string")
                    {
                        sb1.AppendLine(string.Format("{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                    else
                    {
                        sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                    }
                }
            }
            sb1.AppendLine();

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

            table_config = _config[subTable];
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

            var str = string.Format(ModelTemplate.JOINED_CLASS_TEMPLATE,
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
