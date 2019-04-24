﻿using Generator.Template;
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

                if (trace)
                {
                    sb2.AppendLine(string.Format("{0}{0}[Newtonsoft.Json.JsonProperty] private volatile int _ver_{1};", '\t', p.Name.ToLower()));
                    sb3.AppendLine($"\t\t\tif (_ver_{p.Name.ToLower()} != 0)");
                    sb3.AppendLine("\t\t\t{");
                    sb3.AppendLine($"\t\t\t\tupdate_fields.Add({tableName}Helper.Columns.{p.Name});");
                    sb3.AppendLine($"\t\t\t\tif (!preserve) _ver_{ p.Name.ToLower()} = 0;");
                    sb3.AppendLine("\t\t\t}");
                    sb5.AppendLine($"\t\t\t_ver_{p.Name.ToLower()} = 0;");
                }
            }
            sb1.Append("\t\tprivate int ___pagerow;");

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
                    sb4.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; {2}}}", '\t', p.Name.ToLower(), trace ? $"if (_____flag) System.Threading.Interlocked.Increment(ref _ver_{p.Name.ToLower()}); " : string.Empty));
                    sb4.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                    sb4.AppendLine(string.Format("{0}{0}}}", '\t'));
                    sb4.AppendLine();
                }
            }
            sb4.AppendLine();
            sb4.AppendLine("\t\tpublic int __PageRow");
            sb4.AppendLine("\t\t{");
            sb4.AppendLine("\t\t\tset { ___pagerow = value; }");
            sb4.AppendLine("\t\t\tget { return ___pagerow; }");
            sb4.Append("\t\t}");

            if (trace)
            {
                sb4.AppendLine();
                // GetTraceFields
                sb4.AppendLine();
                sb4.AppendLine($"\t\tpublic IList<{tableName}Column> GetTraceFields(bool preserve = false)");
                sb4.AppendLine("\t\t{");
                sb4.AppendLine($"\t\t\tvar update_fields = new List<{tableName}Column>();");
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

        public string Get_Joined_Class(string mainTable, Tuple<string, string> subTable)
        {
            var table_config = _config[mainTable];
            var sb1 = new StringBuilder();
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
            sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', subTable.Item1, subTable.Item1.ToLower()));

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
                    sb2.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', subTable.Item1, subTable.Item2));
                    sb2.AppendLine(string.Format("{0}{0}{{", '\t'));
                    sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; }}", '\t', subTable.Item1.ToLower()));
                    sb2.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', subTable.Item1.ToLower()));
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

            table_config = _config[subTable.Item1];
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
                                    subTable.Item1,
                                    sb3.ToString(),
                                    sb4.ToString(),
                                    Environment.NewLine + sb1.ToString(),
                                    sb2.ToString());
            return str;
        }

        public string Get_Entity_Class(string tableName)
        {
            var table_config = _config[tableName];
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

            var str = string.Format(ModelTemplate.ENTITY_CLASS_TEMPLATE,
                                    tableName,
                                    table_config.Comment,
                                    _config.Model_ClassNamePrefix,
                                    tableName,
                                    _config.Model_ClassNameSuffix,
                                    string.IsNullOrWhiteSpace(_config.Model_BaseClass) ? string.Empty : (" : " + _config.Model_BaseClass),
                                    tableName,
                                    Environment.NewLine + sb1.ToString(),
                                    sb2.ToString(),
                                    sb4.ToString());
            return str;
        }
    }
}
