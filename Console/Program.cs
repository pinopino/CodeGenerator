using Dapper;
using Generator.Common;
using Generator.Core;
using Generator.Core.Config;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using System;
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
            var config = new GlobalConfiguration();
            config.Init();

            if (string.IsNullOrWhiteSpace(config.DBType))
            {
                System.Console.WriteLine("未设置要连接的数据库类型！");
                System.Console.Read();
                Environment.Exit(0);
            }

            if (string.IsNullOrWhiteSpace(config.DBConn))
            {
                System.Console.WriteLine("未设置数据库连接字符串！");
                System.Console.Read();
                Environment.Exit(0);
            }

            Print("解析数据库元数据...");
            IProgressBar progress = GetProgressBar();
            BaseParser parser = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    parser = new Generator.Core.MSSql.Parser(config, progress);
                    break;
                case "mysql":
                    parser = new Generator.Core.MySql.Parser(config, progress);
                    break;
            }
            var meta_data = parser.ParseMetadata();
            Print("解析完毕，生成中间配置文件...");
            // 生成中间配置文件
            var meta_json = JsonConvert.SerializeObject(meta_data);
            OutputHelper.OutputConfig(meta_json, config, progress);

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
                    OutputHelper.OutputDAL(meta_data, config, progress);

                    // 生成Model最终文件
                    Print("生成Model...");
                    OutputHelper.OutputModel(meta_data, config, progress);

                    // 生成Enum最终文件
                    Print("生成Enum...");
                    OutputHelper.OutputEnum(meta_data, config, progress);

                    // 检测partial字段有效性
                    Print("检测partial字段有效性...");
                    OutputHelper.DoPartialCheck(meta_data, config, progress);

                    Print("生成完毕！");
                    break;
                }
                System.Console.WriteLine("按‘quit’退出");
            } while (key != "quit");

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

        static ConsoleProgressBar GetProgressBar()
        {
            return new ConsoleProgressBar(System.Console.CursorLeft, System.Console.CursorTop, 50, ProgressBarType.Character);
        }
    }
}
