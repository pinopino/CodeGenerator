using Generator.Common;
using Generator.Core.Config;
using System;
using System.Collections.Generic;
using System.Data.Common;

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

        protected string FindDBName(string connStr)
        {
            var db_name = string.Empty;
            var cb = new DbConnectionStringBuilder(false);
            cb.ConnectionString = connStr;
            object database;
            if (cb.TryGetValue("Initial Catalog", out database))
            {
                db_name = database.ToString();
            }
            else
            {
                if (cb.TryGetValue("Database", out database))
                {
                    db_name = database.ToString();
                }
            }

            return db_name;
        }

        protected void ProgressPrint(long index, long total)
        {
            if (_progress != null)
                _progress.Dispaly(Convert.ToInt32((index / (total * 1.0)) * 100));
        }
    }
}
