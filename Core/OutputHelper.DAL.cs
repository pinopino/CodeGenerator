﻿using Generator.Common;
using Generator.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Generator.Core
{
    public sealed partial class OutputHelper
    {
        public static void OutputDAL(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            var path = Path.Combine(config.OutputBasePath, "DAL");
            Directory.CreateDirectory(path);

            var sb = new StringBuilder();
            BaseDALGenerator g = null;
            // TODO：有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    g = new Generator.Core.MSSql.DALGenerator(config);
                    break;
                case "mysql":
                    g = new Generator.Core.MySql.DALGenerator(config);
                    break;
                case "oracle":
                    g = new Generator.Core.Oracle.DALGenerator(config);
                    break;
                default:
                    throw new NotSupportedException("不支持的数据库类型");
            }

            // 解析
            var i = 0;
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExcludeTables != null && config.ExcludeTables.Any(p => p.Name == table.Name))
                    continue;

                sb.Append(g.RenderDALFor(table));

                File.AppendAllText(Path.Combine(path, string.Format("{0}Helper.cs", table.Name)), sb.ToString());
                sb.Clear();
                PrintProgress(progress, ++i, tables.Count);
            }

            // 生成BaseTableHelper、PageDataView
            File.AppendAllText(Path.Combine(path, "BaseTableHelper.cs"), g.RenderBaseTableHelper());

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "DAL"), path);
        }
    }
}
