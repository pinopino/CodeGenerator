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
        public static void OutputModel(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            var path = Path.Combine(config.OutputBasePath, "Model");
            Directory.CreateDirectory(path);

            BaseModelGenerator g = new ModelGenerator(config);
            // 解析
            var i = 0;
            var sb = new StringBuilder();
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExcludeTables != null && config.ExcludeTables.Any(p => p.Name == table.Name))
                    continue;

                sb.AppendLine(g.RenderModelFor(table));
                File.AppendAllText(Path.Combine(path, string.Format("{0}.cs", g.FileName)), sb.ToString());
                sb.Clear();
                PrintProgress(progress, ++i, tables.Count);
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "Model"), path);
        }
    }
}
