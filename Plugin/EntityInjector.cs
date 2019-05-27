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
    internal class EntityInjector : BaseInjector, IModelInjector
    {
        public EntityInjector(Dictionary<string, TableMetaData> tables, GlobalConfiguration config)
            : base(tables, config)
        { }

        public override string Inject(string originContent, string tableName = "", string columnName = "")
        {
            return string.Empty;
        }

        public void InjectToNewFile(string tableName = "", string columnName = "")
        {
            EnsurePath();

            var table = _tables[tableName];
            var sb = new StringBuilder();
            sb.Append(_config.ModelConfig.HeaderNote);
            sb.AppendLine(string.Join(Environment.NewLine, _config.ModelConfig.Using));
            sb.AppendLine($"using {_config.DALConfig.Namespace};");
            sb.AppendLine($"using {_config.DALConfig.Namespace}.Metadata;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_config.ModelConfig.Namespace}.EntityModel");
            sb.AppendLine("{");            
            foreach (var col in table.Columns)
            {
                sb.AppendLine(string.Format("{0}{0}private volatile int _ver_{1};", '\t', col.Name.ToLower()));
            }
            sb.AppendLine("}");

            //File.AppendAllText(Path.Combine(path, "EntityModel", string.Format("{0}.cs", "Entity" + talbe)), sb2.ToString());
            //sb2.Clear();

        }

        private void EnsurePath()
        {
            var path = Path.Combine(_config.OutputBasePath, "Model");
            var _outputpath = Path.Combine(path, "EntityModel");
            if (Directory.Exists(_outputpath))
            {
                DeleteDirectory(_outputpath);
            }
            else
            {
                Directory.CreateDirectory(_outputpath);
            }
        }

        private void DeleteDirectory(string target_dir, bool del_self = false)
        {
            var files = Directory.GetFiles(target_dir);
            var dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir, true);
            }

            if (del_self)
            {
                Directory.Delete(target_dir, false);
            }
        }
    }
}
