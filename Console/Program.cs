using Generator.Common;
using Generator.Core;
using Generator.Core.Config;
using Generator.Template;
using Newtonsoft.Json;
using RazorLight;
using System;
using System.IO;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new GlobalConfiguration();
            config.Init();

            if (string.IsNullOrWhiteSpace(config.DBType))
                Exit("未设置要连接的数据库类型（目前支持mssql，mysql）！");

            if (string.IsNullOrWhiteSpace(config.DBConn))
                Exit("未设置数据库连接字符串！");

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
                    // 手动加载插件
                    OutputHelper.LoadPlugin(meta_data, config);

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
                    //OutputHelper.DoPartialCheck(meta_data, config, progress);

                    Print("生成完毕！");
                    break;
                }
                System.Console.WriteLine("输入‘quit’退出");
            } while (key != "quit");

            Print("结束！");
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
            if(!string.IsNullOrEmpty(message))
                System.Console.WriteLine(message);
            System.Console.Read();
            Environment.Exit(0);
        }

        static ConsoleProgressBar GetProgressBar()
        {
            return new ConsoleProgressBar(System.Console.CursorLeft, System.Console.CursorTop, 50, ProgressBarType.Character);
        }

        static void TestRazor()
        {
            var _engine = new RazorLightEngineBuilder()
             .UseFilesystemProject(@"D:\test")
             .UseMemoryCachingProvider()
             .Build();

            var model = new ViewInfoWapper(null);
            model.Config = new Generator.Core.Config.GlobalConfiguration();
            model.Config.Project = "my_test_proj";
            model.Config.DALConfig = new Generator.Core.Config.DALConfig();
            model.Config.DALConfig.Namespace = "abc";
            model.Config.DALConfig.HeaderNote = "head-note: hello world";
            model.Config.DALConfig.Using = new System.Collections.Generic.List<string> { "using a;", "using b;" };
            model.Config.DALConfig.ClassPrefix = string.Empty;
            model.Config.DALConfig.ClassSuffix = string.Empty;
            model.Config.DALConfig.BaseClass = "mybaseclass";
            model.Config.DALConfig.Methods = new System.Collections.Generic.List<Generator.Core.Config.MethodInfo>
            {
                new Generator.Core.Config.MethodInfo { Name = "test" },
                new Generator.Core.Config.MethodInfo { Name = "exists" },
            };
            model.TableInfo = new Generator.Core.TableMetaData();
            model.TableInfo.Name = "sql_table";
            model.TableInfo.PrimaryKey = new System.Collections.Generic.List<Generator.Core.ColumnMetaData>
            {
                new Generator.Core.ColumnMetaData { Name ="p1", Comment ="p1的注释信息" },
                new Generator.Core.ColumnMetaData { Name ="p2", Comment ="p2的注释信息" },
                new Generator.Core.ColumnMetaData { Name ="p3", Comment ="p3的注释信息" },
            };

            var result = string.Empty;
            var cacheResult = _engine.TemplateCache.RetrieveTemplate("dal_test.cshtml");
            if (cacheResult.Success)
                result = _engine.RenderTemplateAsync(cacheResult.Template.TemplatePageFactory(), model).Result;
            else
                result = _engine.CompileRenderAsync("dal_test.cshtml", model).Result;

            File.WriteAllText("hhhhh.txt", result);
            System.Console.WriteLine("ok");
            System.Console.Read();
        }
    }
}
