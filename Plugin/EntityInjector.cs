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
    public class EntityInjector : BaseInjector, IModelInjector
    {
        public override string Name => "EntitySupport";

        public EntityInjector(Dictionary<string, TableMetaData> tables, GlobalConfiguration config)
            : base(tables, config)
        { }

        public override string Inject(string originContent, string tableName = "", string columnName = "")
        {
            var table = _tables[tableName];
            if (_config.EntityTables == null || _config.EntityTables.Count == 0)
                return originContent;
            if (!_config.EntityTables.Any(p => p.Name == tableName))
                return originContent;

            var ret = new StringBuilder();
            // fields
            var sb = new StringBuilder();
            foreach (var col in table.Columns)
            {
                sb.AppendLine(string.Format("{0}{0}private int _ver_{1};", '\t', col.Name.ToLower()));
            }

            // GetTraceFields
            sb.AppendLine();
            sb.AppendLine($"\t\tpublic Dictionary<string, object> GetTracedInfo()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tvar info = new Dictionary<string, object>();");
            foreach (var col in table.Columns)
            {
                sb.AppendLine($"\t\t\tif (_ver_{col.Name.ToLower()} != 0)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine($"\t\t\t\tinfo.Add(\"{col.Name}\", _{col.Name.ToLower()});");
                sb.AppendLine($"\t\t\t_ver_{col.Name.ToLower()} = 0;");
                sb.AppendLine("\t\t\t}");
            }
            sb.AppendLine("\t\t\treturn info;");
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
                            var newline = arr[0] + tmp + "++; }";
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
