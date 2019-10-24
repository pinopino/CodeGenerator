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
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using System.Text;",
                    "using Dapper;",
                };
            this.DALConfig.Using.Add($"using {this.ModelConfig.Namespace};");
            this.DALConfig.Using.Add($"using {this.DALConfig.Namespace}.Metadata;");
            this.DALConfig.Using.Add($"using {this.DALConfig.Namespace}.Base;");
            if (this.JoinedTables != null && this.JoinedTables.Count > 0)
                this.DALConfig.Using.Add($"using {this.ModelConfig.Namespace}.JoinedViewModel;");
        }

        public T GetValue<T>(string key)
        {
            return _root.GetValue<T>(key);
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
        /// 需要检查的DAL Partial文件的路径
        /// </summary>
        public string PartialCheck_DAL_Path { set; get; }
        /// <summary>
        /// 生成时想要排除的表
        /// </summary>
        public List<TableInfo> ExceptTables { set; get; }
        /// <summary>
        /// 生成更新代码时想要排除掉的字段。例如：Table1:Name,Age;*:CreatedTime
        /// </summary>
        public Dictionary<string, List<ExceptColumnInfo>> UpdateExceptColumns { set; get; }
        /// <summary>
        /// 需要生成join方法的表
        /// </summary>
        public List<JoinMapping> JoinedTables { set; get; }
        /// <summary>
        /// 如果需要在数据库定义文档更新时重建sql server中的数据库，请指定该字段值
        /// 包含：
        ///     1. 需要重建的数据库名称
        ///     2. 用于重建的数据库定义sql文件的路径
        /// </summary>
        public ReCreateDBInfo ReCreateDB { set; get; }
        /// <summary>
        /// 需要追踪字段修改的表
        /// </summary>
        public List<TableInfo> TraceFieldTables { set; get; }
        /// <summary>
        /// 需要实现接口IEntity接口的表
        /// </summary>
        public List<TableInfo> EntityTables { set; get; }
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

    public class JoinMapping
    {
        public TableInfo Table_Main { set; get; }
        public TableInfo Table_Sub { set; get; }
        public string Sub_InnerName { set; get; }
    }

    public class ReCreateDBInfo
    {
        public string SQLFilePath { set; get; }
        public string Encoding { set; get; }
        public List<DBInfo> DBs { set; get; }
    }

    public class ExceptColumnInfo
    {
        public string ColumnName { set; get; }
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
        public List<MethodInfo> Methods { set; get; }
    }

    public class MethodInfo
    {
        public string Name { set; get; }
    }
}
