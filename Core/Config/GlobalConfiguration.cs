using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Generator.Core.Config
{
    // links：
    // https://andrewlock.net/how-to-use-the-ioptions-pattern-for-configuration-in-asp-net-core-rc2/
    // https://stackoverflow.com/questions/53166201/converting-iconfigurationsection-to-ioptions
    public sealed class GlobalConfiguration
    {
        private IConfigurationRoot _root;

        public void Init(string path = "")
        {
            if (string.IsNullOrWhiteSpace(path))
                path = AppDomain.CurrentDomain.BaseDirectory;

            var builder = new ConfigurationBuilder()
               .SetBasePath(path)
               .AddJsonFile("appsettings.json");
            _root = builder.Build();
            _root.Bind(this);

            // 修正某些字段保证初始化效果
            if (string.IsNullOrWhiteSpace(this.Project))
                this.Project = "YourProject";
            if (string.IsNullOrWhiteSpace(this.OutputBasePath))
                this.OutputBasePath = "c:\\output";

            // namespace
            if (string.IsNullOrWhiteSpace(this.DALConfig.Namespace))
                this.DALConfig.Namespace = "DAL";
            this.DALConfig.Namespace = $"{this.Project}.{this.DALConfig.Namespace}";
            if (string.IsNullOrWhiteSpace(this.ModelConfig.Namespace))
                this.ModelConfig.Namespace = "Model";
            this.ModelConfig.Namespace = $"{this.Project}.{this.ModelConfig.Namespace}";

            // model
            if (string.IsNullOrWhiteSpace(this.ModelConfig.HeaderNote))
                this.ModelConfig.HeaderNote = "本文件由生成工具自动生成，请勿随意修改内容除非你很清楚自己在做什么！";
            if (this.ModelConfig.Using == null || this.ModelConfig.Using.Count == 0)
                this.ModelConfig.Using = new List<string> {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using System.Text;"
                };
            this.ModelConfig.Using.Add($"using {this.DALConfig.Namespace};");
            this.ModelConfig.Using.Add($"using {this.DALConfig.Namespace}.Metadata;");

            // dal
            if (string.IsNullOrWhiteSpace(this.DALConfig.HeaderNote))
                this.DALConfig.HeaderNote = "本文件由生成工具自动生成，请勿随意修改内容除非你很清楚自己在做什么！";
            if (this.DALConfig.Using == null || this.DALConfig.Using.Count == 0)
                this.DALConfig.Using = new List<string> {
                    "using Dapper;",
                    "using System;",
                    "using System.Collections;",
                    "using System.Collections.Generic;",
                    "using System.Data;",
                    "using System.Linq;",
                    "using System.Linq.Expressions;",
                    "using System.Text;"
                };
            this.DALConfig.Using.Add($"using {this.ModelConfig.Namespace};");
            this.DALConfig.Using.Add($"using {this.DALConfig.Namespace}.Metadata;");
            this.DALConfig.Using.Add($"using {this.DALConfig.Namespace}.Base;");

            // exclude table
            this.ExcludeTables = new List<TableInfo>();
            foreach (var item in _root.GetSection("ExcludeTables").GetChildren())
                this.ExcludeTables.Add(new TableInfo { Name = item.Value });
        }

        /// <summary>
        /// 数据库类型: mssql, mysql
        /// </summary>
        public string DBType { set; get; }
        /// <summary>
        /// 连接数据库
        /// </summary>
        public string DBConn { set; get; }
        /// <summary>
        /// 输出文件路径
        /// </summary>
        public string OutputBasePath { set; get; }
        /// <summary>
        /// 模板文件路径
        /// </summary>
        public string TemplatePath { set; get; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string Project { set; get; }
        /// <summary>
        /// 生成时想要排除的表
        /// </summary>
        public List<TableInfo> ExcludeTables { set; get; }
        /// <summary>
        /// 生成Model对象时指定的配置信息
        /// </summary>
        public ModelConfig ModelConfig { set; get; }
        /// <summary>
        /// 生成DAL交互逻辑时指定的配置信息
        /// </summary>
        public DALConfig DALConfig { set; get; }
    }

    public class DBInfo
    {
        public string Name { set; get; }
    }

    public class TableInfo
    {
        public string Name { set; get; }
    }

    public class ColumnInfo
    {
        public string Name { set; get; }
    }

    public class ModelConfig
    {
        public string HeaderNote { set; get; }
        public List<string> Using { set; get; }
        public string Namespace { set; get; }
        public string BaseClass { set; get; }
        public string ClassPrefix { set; get; }
        public string ClassSuffix { set; get; }
    }

    public class DALConfig
    {
        public string HeaderNote { set; get; }
        public List<string> Using { set; get; }
        public string Namespace { set; get; }
        public string BaseClass { set; get; }
        public string ClassPrefix { set; get; }
        public string ClassSuffix { set; get; }
        public List<string> Methods { set; get; }
    }
}
