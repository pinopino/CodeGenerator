using Generator.Core.Config;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Generator.Core.Inject
{
    public class BaseInjector : IInjector
    {
        protected static readonly string _mark_head = "//@_head__@";
        protected static readonly string _mark_tail = "//@_tail__@";
        protected GlobalConfiguration _config;
        protected Dictionary<string, TableMetaData> _tables;

        public BaseInjector(Dictionary<string, TableMetaData> tables, GlobalConfiguration config)
        {
            _tables = tables;
            _config = config;
        }

        public string InjectHead(string originContent, string injectContent, string tableName = "", string columnName = "")
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

        public string InjectTail(string originContent, string injectContent, string tableName = "", string columnName = "")
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
    }
}
