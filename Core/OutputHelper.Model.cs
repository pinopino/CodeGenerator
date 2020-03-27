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
                if (config.ExceptTables != null && config.ExceptTables.Any(p => p.Name == table.Name))
                    continue;

                sb.AppendLine(g.RenderModelFor(table));

                var new_str = sb.ToString();
                // 插件的执行顺序
                foreach (var plug in _plugins)
                {
                    //if (!typeof(IModelInjector).IsAssignableFrom(plug.GetType()))
                    //    continue;
                    //if (!plug.Check(table.Name))
                    //    continue;
                    //new_str = plug.Inject(sb.ToString(), table.Name);
                    //File.AppendAllText(Path.Combine(config.OutputBasePath, plug.Name, string.Format("{0}.cs", g.FileName)), new_str);
                    //new_str = string.Empty;
                }

                File.AppendAllText(Path.Combine(path, string.Format("{0}.cs", g.FileName)), new_str);
                sb.Clear();
                PrintProgress(progress, ++i, tables.Count);
            }

            // 如果配置文件指定了JoinedTables，那么这里需要为这些关联表生成额外的包装model，
            // 路径：Model\JoinedViewModel
            if (config.JoinedTables != null && config.JoinedTables.Count > 0)
            {
                //Directory.CreateDirectory(Path.Combine(path, "JoinedViewModel"));
                //var sb2 = new StringBuilder();
                //foreach (var map in config.JoinedTables)
                //{
                //    sb2.AppendLine(g.Get_Join_Head(map));
                //    sb2.AppendLine(g.Get_Joined_Class(map));
                //    sb2.AppendLine(g.Get_Join_Tail(map));

                //    File.AppendAllText(Path.Combine(path, "JoinedViewModel", string.Format("{0}.cs", "Joined" + g.FileName)), sb2.ToString());
                //    sb2.Clear();
                //}
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "Model"), path);
        }
    }
}
