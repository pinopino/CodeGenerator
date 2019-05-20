using Generator.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Generator.Core
{
    public abstract class BaseParser
    {
        private readonly string _connstr;
        private ConsoleProgressBar _progress;

        public string ConnStr { get { return this._connstr; } }

        public BaseParser(string connStr, bool enableProgress = true)
        {
            _connstr = connStr;
            if (enableProgress)
                _progress = new ConsoleProgressBar(System.Console.CursorLeft, System.Console.CursorTop, 50, ProgressBarType.Character);
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
