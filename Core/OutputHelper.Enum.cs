using Generator.Common;
using Generator.Core.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Generator.Core
{
    public sealed partial class OutputHelper
    {
        public static void OutputEnum(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            var path = Path.Combine(config.OutputBasePath, "Enum");
            Directory.CreateDirectory(path);

            BaseEnumGenerator g = new EnumGenerator(config);
            // 解析
            var i = 0;
            var sb = new StringBuilder();
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables != null && config.ExceptTables.Any(p => p.Name == table.Name))
                    continue;

                foreach (var column in table.Columns)
                {
                    if (g.CanGenerateEnum(table, column))
                    {
                        sb.AppendLine(g.RenderEnumFor(table, column));
                        File.AppendAllText(Path.Combine(path, string.Format("{0}.cs", g.FileName)), sb.ToString());
                        sb.Clear();
                    }
                }
                PrintProgress(progress, ++i, tables.Count);
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "Enum"), path);
        }
    }
}
