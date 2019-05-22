using Generator.Common;
using Generator.Core.Config;
using System;
using System.Collections.Generic;

namespace Generator.Core
{
    public abstract class BaseParser
    {
        private IProgressBar _progress;
        private GlobalConfiguration _config;

        public string ConnStr { get { return this._config.DBConn; } }

        public BaseParser(GlobalConfiguration config, IProgressBar progress)
        {
            _config = config;
            _progress = progress;
        }

        public abstract Dictionary<string, TableMetaData> ParseMetadata();

        protected abstract string FindDBName(string connStr);

        protected void ProgressPrint(long index, long total)
        {
            if (_progress != null)
                _progress.Dispaly(Convert.ToInt32((index / (total * 1.0)) * 100));
        }
    }
}
