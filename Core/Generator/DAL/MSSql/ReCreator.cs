using Dapper;
using Generator.Core.Config;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Generator.Core.MSSql
{
    public class ReCreator : IReCreateDB
    {
        private GlobalConfiguration _config;

        public ReCreator(GlobalConfiguration config)
        {
            _config = config;
        }

        // link: https://stackoverflow.com/questions/18596876/go-statements-blowing-up-sql-execution-in-net
        public void ReCreate()
        {
            var conn_str = _config.DBConn;
            var db_list = _config.ReCreateDB.DBs;
            var config_path = _config.ReCreateDB.SQLFilePath;
            if (string.IsNullOrWhiteSpace(config_path) || db_list == null || db_list.Count == 0)
            {
                return;
            }

            var db_current = FindDBName(_config.DBConn);
            if (string.IsNullOrWhiteSpace(db_current))
            {
                return;
            }

            var files = Directory.GetFiles(config_path, "*.sql", SearchOption.TopDirectoryOnly);
            foreach (var item in db_list)
            {
                System.Console.WriteLine("尝试重新生成数据库[" + item.Name + "]...");
                System.Console.WriteLine("检测是否存在该数据库");
                if (IsExist(item.Name))
                {
                    conn_str = conn_str.Replace(db_current, item.Name);
                    using (var conn = new SqlConnection(conn_str))
                    {
                        var svr = new Server(new ServerConnection(conn));
                        foreach (var file_path in files)
                        {
                            var script = File.ReadAllText(file_path, Encoding.GetEncoding(_config.ReCreateDB.Encoding));
                            svr.ConnectionContext.ExecuteNonQuery(script);
                        }
                    }
                    System.Console.WriteLine("存在，重新生成数据库[" + item.Name + "]成功");
                }
                else
                {
                    System.Console.WriteLine("不存在该数据库，结束");
                }
                System.Console.WriteLine();
            }
        }

        private string FindDBName(string connStr)
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

        private bool IsExist(string db)
        {
            using (SqlConnection connection = new SqlConnection(_config.DBConn))
            {
                return connection.ExecuteScalar<int>("select count(*) From master.dbo.sysdatabases where name='" + db + "'") > 0;
            }
        }
    }
}
