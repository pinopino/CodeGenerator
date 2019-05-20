using Generator.Core.Config;
using System.Collections.Generic;

namespace Generator.Core
{
    public class BaseGenerator
    {
        protected readonly GlobalConfiguration _config;
        protected readonly Dictionary<string, TableMetaData> _tables;

        public BaseGenerator(GlobalConfiguration config, Dictionary<string, TableMetaData> tables)
        {
            _config = config;
            _tables = tables;
        }
    }
}
