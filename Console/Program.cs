using Dapper;
using Generator.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn_str = ConfigurationManager.AppSettings["DBConn"];
            if (string.IsNullOrWhiteSpace(conn_str))
            {
                System.Console.WriteLine("未设置数据库连接字符串！");
                System.Console.Read();
                Environment.Exit(0);
            }

            ReCreateDB(conn_str, Encoding.GetEncoding("gb2312"));

            var dbcode = FindDBName(conn_str);
            Print("解析数据库元数据...");
            using (SQLServerManagement manage = new SQLServerManagement(conn_str))
            {
                var config = new SQLMetaData();
                SQLMetaDataHelper.InitConfig(config);
                var data = manage.Databases[dbcode].Tables;
                // 解析数据库元数据
                var parser = new MetaDataParser(config);
                parser.Parse(data);
                Print("解析完毕，生成中间配置文件...");

                // 生成中间配置文件
                var config_json_str = JsonConvert.SerializeObject(config);
                SQLMetaDataHelper.OutputConfig(config_json_str);

                // 生成最终文件
                Print("按 'y/Y' 继续生成最终操作类文件...");
                var key = string.Empty;
                do
                {
                    key = System.Console.ReadLine();
                    if (key == "Y" || key == "y")
                    {
                        // 生成DAL最终文件
                        Print("生成DAL...");
                        SQLMetaDataHelper.OutputDAL(config);

                        // 生成Model最终文件
                        Print("生成Model...");
                        SQLMetaDataHelper.OutputModel(config);

                        // 生成Enum最终文件
                        Print("生成Enum...");
                        SQLMetaDataHelper.OutputEnum(config);

                        // 检测partial字段有效性
                        Print("检测partial字段有效性...");
                        SQLMetaDataHelper.DoPartialCheck(config);

                        Print("生成完毕！");
                        break;
                    }
                    System.Console.WriteLine("按‘quit’退出");
                } while (key != "quit");
            }

            Print("结束！");
            System.Console.Read();
            Environment.Exit(0);
        }

        // link: https://stackoverflow.com/questions/18596876/go-statements-blowing-up-sql-execution-in-net
        static void ReCreateDB(string connStr, Encoding encoding)
        {
            var config_path = ConfigurationManager.AppSettings["ReCreateDB_SQLFile"];
            var db_names = ConfigurationManager.AppSettings["ReCreateDB_Names"];
            if (string.IsNullOrWhiteSpace(config_path) || string.IsNullOrWhiteSpace(db_names))
            {
                return;
            }

            var db = FindDBName(connStr);
            if (string.IsNullOrWhiteSpace(db))
            {
                return;
            }

            var files = Directory.GetFiles(config_path, "*.sql", SearchOption.TopDirectoryOnly);
            var arrs = db_names.Replace('；', ';').Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arrs)
            {
                System.Console.WriteLine("尝试重新生成数据库[" + item + "]...");
                System.Console.WriteLine("检测是否存在该数据库");
                if (IsExist(connStr, item))
                {
                    connStr = connStr.Replace(db, item);
                    using (var conn = new SqlConnection(connStr))
                    {
                        var svr = new Server(new ServerConnection(conn));
                        foreach (var file_path in files)
                        {
                            var script = File.ReadAllText(file_path, Encoding.GetEncoding("gb2312"));
                            svr.ConnectionContext.ExecuteNonQuery(script);
                        }
                    }
                    System.Console.WriteLine("存在，重新生成数据库[" + item + "]成功");
                }
                else
                {
                    System.Console.WriteLine("不存在该数据库，结束");
                }
                System.Console.WriteLine();
            }
        }

        static string FindDBName(string connStr)
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

        static bool IsExist(string connStr, string db)
        {
            using (var conn = GetOpenConnection(connStr))
            {
                return conn.ExecuteScalar<int>("select count(*) From master.dbo.sysdatabases where name='" + db + "'") > 0;
            }
        }

        static SqlConnection GetOpenConnection(string connStr, bool mars = false)
        {
            var cs = connStr;
            if (mars)
            {
                var scsb = new SqlConnectionStringBuilder(cs)
                {
                    MultipleActiveResultSets = true
                };
                cs = scsb.ConnectionString;
            }
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }

        static void Print(string message)
        {
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine(message);
        }
    }
}
