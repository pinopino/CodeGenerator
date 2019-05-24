using Generator.Core.Config;
using System.IO;
using System.Text;

namespace Generator.Core.Inject
{
    public abstract class BaseInjector : IInjector
    {
        protected static readonly string _mark_head = "//@_head__@";
        protected static readonly string _mark_tail = "//@_tail__@";
        protected GlobalConfiguration _config;

        public BaseInjector(GlobalConfiguration config)
        {
            _config = config;
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

        public abstract string Inject(string originContent);

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
    }
}
