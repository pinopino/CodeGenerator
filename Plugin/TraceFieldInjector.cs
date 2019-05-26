using Generator.Core;
using Generator.Core.Config;
using Generator.Core.Inject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugin
{
    public class TraceFieldInjector : BaseInjector, IModelInjector
    {
        public TraceFieldInjector(Dictionary<string, TableMetaData> tables, GlobalConfiguration config)
            : base(tables, config)
        { }

        public override string Inject(string originContent, string tableName = "", string columnName = "")
        {
            var ret = new StringBuilder();

            // fields
            var sb1 = new StringBuilder();
            sb1.AppendLine("\t\tprivate bool _____flag;");
            var table = _tables[tableName];
            foreach (var col in table.Columns)
            {
                sb1.AppendLine(string.Format("{0}{0}private volatile int _ver_{1};", '\t', col.Name.ToLower()));
            }

            // GetTraceFields
            sb1.AppendLine();
            sb1.AppendLine($"\t\tpublic IList<{tableName}Column> GetTraceFields(bool preserve = false)");
            sb1.AppendLine("\t\t{");
            sb1.AppendLine($"\t\t\tvar update_fields = new List<{tableName}Column>();");
            foreach (var col in table.Columns)
            {
                sb1.AppendLine($"\t\t\tif (_ver_{col.Name.ToLower()} != 0)");
                sb1.AppendLine("\t\t\t{");
                sb1.AppendLine($"\t\t\t\tupdate_fields.Add({tableName}Helper.Columns.{col.Name});");
                sb1.AppendLine($"\t\t\t\tif (!preserve) _ver_{ col.Name.ToLower()} = 0;");
                sb1.AppendLine("\t\t\t}");
            }
            sb1.AppendLine("\t\t\treturn update_fields;");
            sb1.AppendLine("\t\t}");

            // OpenTrace
            sb1.AppendLine();
            sb1.AppendLine("\t\tpublic void BeginTrace()");
            sb1.AppendLine("\t\t{");
            sb1.AppendLine("\t\t\t_____flag = true;");
            sb1.AppendLine("\t\t}");
            sb1.AppendLine();
            // CloseTrace
            sb1.AppendLine("\t\tpublic void EndTrace()");
            sb1.AppendLine("\t\t{");
            sb1.AppendLine("\t\t\t_____flag = false;");
            sb1.AppendLine("\t\t}");

            // ResetTrace
            sb1.AppendLine();
            sb1.AppendLine("\t\tpublic void ResetTrace()");
            sb1.AppendLine("\t\t{");
            foreach (var col in table.Columns)
            {
                sb1.AppendLine($"\t\t\t_ver_{col.Name.ToLower()} = 0;");
            }
            sb1.AppendLine("\t\t\t_____flag = false;");
            sb1.Append("\t\t}");

            // 替换
            var reg = new Regex("_.+ =");
            using (StringReader sr = new StringReader(originContent))
            {
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (line.Contains(_mark_tail))
                    {
                        line = sb1.ToString();
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
