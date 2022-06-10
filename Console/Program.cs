using Generator.Common;
using Generator.Core;
using Generator.Core.Config;
using Newtonsoft.Json;
using System;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new GlobalConfiguration();
            config.Init();

            if (string.IsNullOrWhiteSpace(config.DBType))
                Exit("未设置要连接的数据库类型（目前支持mssql，mysql，oracle）！");

            if (string.IsNullOrWhiteSpace(config.DBConn))
                Exit("未设置数据库连接字符串！");

            Print("解析数据库元数据...");
            IProgressBar progress = GetProgressBar();
            BaseParser parser = null;
            // TODO：有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    parser = new Generator.Core.MSSql.Parser(config, progress);
                    break;
                case "mysql":
                    parser = new Generator.Core.MySql.Parser(config, progress);
                    break;
                case "oracle":
                    parser = new Generator.Core.Oracle.Parser(config, progress);
                    break;
                default:
                    throw new NotSupportedException("不支持的数据库类型");
            }

            // 生成中间配置文件
            var meta_data = parser.ParseMetadata();
            Print("解析完毕，生成中间配置文件...");
            var meta_json = JsonConvert.SerializeObject(meta_data, Formatting.Indented);
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

                    Print("生成完毕！");
                    break;
                }
                System.Console.WriteLine("输入‘quit’退出");
            } while (key != "quit");

            Print("程序结束！");
            Exit();
        }

        static void Print(string message)
        {
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine(message);
        }

        static void Exit(string message = "")
        {
            if (!string.IsNullOrEmpty(message))
                System.Console.WriteLine(message);
            System.Console.Read();
            Environment.Exit(0);
        }

        static ConsoleProgressBar GetProgressBar()
        {
            return new ConsoleProgressBar(System.Console.CursorLeft, System.Console.CursorTop, 50, ProgressBarType.Character);
        }
    }
}
