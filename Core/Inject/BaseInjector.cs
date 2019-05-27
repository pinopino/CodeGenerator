using Generator.Core.Config;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Generator.Core.Inject
{
    public abstract class BaseInjector : IInjector
    {
        protected static readonly string _mark_head = "//@_head__@";
        protected static readonly string _mark_tail = "//@_tail__@";
        protected GlobalConfiguration _config;
        protected Dictionary<string, TableMetaData> _tables;

        public abstract string Name { get; }

        public BaseInjector(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, bool checkPath = true)
        {
            _tables = tables;
            _config = config;
            if (checkPath)
                EnsurePath();
        }

        public string InjectHead(string originContent, string injectContent)
        {
            var ret = new StringBuilder();
            using (StringReader sr = new StringReader(originContent))
            {
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (line.Contains(_mark_head))
                    {
                        line = injectContent;
                    }
                    ret.AppendLine(line);
                }
            }

            return ret.ToString();
        }

        public virtual string Inject(string originContent, string tableName = "", string columnName = "")
        {
            return originContent;
        }

        public string InjectTail(string originContent, string injectContent)
        {
            var ret = new StringBuilder();
            using (StringReader sr = new StringReader(originContent))
            {
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (line.Contains(_mark_tail))
                    {
                        line = injectContent;
                    }
                    ret.AppendLine(line);
                }
            }

            return ret.ToString();
        }

        protected void EnsurePath()
        {
            var path = Path.Combine(_config.OutputBasePath, "Model");
            var _outputpath = Path.Combine(path, this.Name);
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
