using Generator.Core;
using Newtonsoft.Json;
using System;
using System.Configuration;

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
            System.Data.Common.DbConnectionStringBuilder s = new System.Data.Common.DbConnectionStringBuilder(false);
            s.ConnectionString = conn_str;
            string dbcode="cap_db";
            object database = "";
            if (s.TryGetValue("Initial Catalog", out database))
            {
                dbcode = database.ToString();
            }
            Print("解析数据库元数据...");
            using (SQLServerManagement manage = new SQLServerManagement(conn_str))
            {
                var config = new SQLMetaData();
                SQLMetaDataHelper.InitConfig(config);
                //var database = ConfigurationManager.AppSettings["GetDatabases"];
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

        static void Print(string message)
        {
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine(message);
        }
    }
}
