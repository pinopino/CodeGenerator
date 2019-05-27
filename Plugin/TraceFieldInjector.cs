using Generator.Core;
using Generator.Core.Config;
using Generator.Core.Inject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugin
{
    public class TraceFieldInjector : BaseInjector, IModelInjector
    {
        public override string Name => "TraceFieldSupport";

        public TraceFieldInjector(Dictionary<string, TableMetaData> tables, GlobalConfiguration config)
            : base(tables, config)
        { }

        public override string Inject(string originContent, string tableName = "", string columnName = "")
        {
            var table = _tables[tableName];
            if (_config.TraceFieldTables == null || _config.TraceFieldTables.Count == 0)
                return originContent;
            if (!_config.TraceFieldTables.Any(p => p.Name == tableName))
                return originContent;

            var ret = new StringBuilder();
            // fields
            var sb = new StringBuilder();
            sb.AppendLine("\t\tprivate bool _____flag;");
            foreach (var col in table.Columns)
            {
                sb.AppendLine(string.Format("{0}{0}private volatile int _ver_{1};", '\t', col.Name.ToLower()));
            }

            // GetTraceFields
            sb.AppendLine();
            sb.AppendLine($"\t\tpublic IList<{tableName}Column> GetTraceFields(bool preserve = false)");
            sb.AppendLine("\t\t{");
            sb.AppendLine($"\t\t\tvar update_fields = new List<{tableName}Column>();");
            foreach (var col in table.Columns)
            {
                sb.AppendLine($"\t\t\tif (_ver_{col.Name.ToLower()} != 0)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine($"\t\t\t\tupdate_fields.Add({tableName}Helper.Columns.{col.Name});");
                sb.AppendLine($"\t\t\t\tif (!preserve) _ver_{ col.Name.ToLower()} = 0;");
                sb.AppendLine("\t\t\t}");
            }
            sb.AppendLine("\t\t\treturn update_fields;");
            sb.AppendLine("\t\t}");

            // OpenTrace
            sb.AppendLine();
            sb.AppendLine("\t\tpublic void BeginTrace()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t_____flag = true;");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            // CloseTrace
            sb.AppendLine("\t\tpublic void EndTrace()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t_____flag = false;");
            sb.AppendLine("\t\t}");

            // ResetTrace
            sb.AppendLine();
            sb.AppendLine("\t\tpublic void ResetTrace()");
            sb.AppendLine("\t\t{");
            foreach (var col in table.Columns)
            {
                sb.AppendLine($"\t\t\t_ver_{col.Name.ToLower()} = 0;");
            }
            sb.AppendLine("\t\t\t_____flag = false;");
            sb.Append("\t\t}");

            // 替换
            var reg = new Regex("_.+ =");
            using (StringReader sr = new StringReader(originContent))
            {
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (line.Contains(_mark_tail))
                    {
                        line = sb.ToString();
                    }
                    else
                    {
                        if (line.Contains("set {"))
                        {
                            var m = reg.Match(line);
                            var arr = line.Split('}');
                            var tmp = "_ver" + m.Value.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[0];
                            var newline = arr[0] + "if (_____flag) System.Threading.Interlocked.Increment(ref " + tmp + "); }";
                            line = newline;
                        }
                    }
                    ret.AppendLine(line);
                }
            }

            return ret.ToString();
        }
    }
}
