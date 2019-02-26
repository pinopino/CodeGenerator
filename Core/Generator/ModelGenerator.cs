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
            var trace = _config.TraceFieldTables.Contains(tableName);

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (p.Nullable && p.DbType != "string")
                {
                    sb1.AppendLine(string.Format("{0}{0}private {1}? _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
                else
                {
                    sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
            });

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
                    sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; {2}}}", '\t', p.Name.ToLower(), trace ? "if (_fields != null) _fields.Add(\"" + p.Name + "\");" : string.Empty));
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
                    sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; {2}}}", '\t', p.Name.ToLower(), trace ? "if (_fields != null) _fields.Add(\"" + p.Name + "\");" : string.Empty));
                    sb2.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                    sb2.AppendLine(string.Format("{0}{0}}}", '\t'));
                    sb2.AppendLine();
                }
            }

            if (trace)
            {
                sb2.AppendLine();
                sb2.AppendLine();
                sb2.AppendLine("\t\tpublic void BeginTrace() { _fields = new List<string>(); }");
                sb2.AppendLine();
                sb2.Append("\t\tpublic void EndTrace() { _fields.Clear(); _fields = null; }");
            }

            var str = string.Format(ModelTemplate.CLASS_TEMPLATE,
                                    table_config.Comment,
                                    _config.Model_ClassNamePrefix,
                                    tableName,
                                    _config.Model_ClassNameSuffix,
                                    string.IsNullOrWhiteSpace(_config.Model_BaseClass) ? string.Empty : (" : " + _config.Model_BaseClass),
                                    tableName,
                                    trace ? Environment.NewLine + "\t\tprivate List<string> _fields;" : string.Empty,
                                    sb1.ToString(),
                                    sb2.ToString());
            return str;
        }
    }
}
