using System.Collections.Generic;

namespace Generator.Core.Config
{
    // links：
    // https://andrewlock.net/how-to-use-the-ioptions-pattern-for-configuration-in-asp-net-core-rc2/
    // https://stackoverflow.com/questions/53166201/converting-iconfigurationsection-to-ioptions
    public class GlobalConfig
    {
        /// <summary>
        /// 连接数据库
        /// </summary>
        public string DBConn { set; get; }
        /// <summary>
        /// 输出的文件位置
        /// </summary>
        public string OutputBasePath { set; get; }
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
        public List<DBInfo> DBs { set; get; }
    }

    public class ExceptColumnInfo
    {
        public string ColumnName { set; get; }
    }

    public class ModelConfig
    {
        public string HeaderNote { set; get; }
        public string Using { set; get; }
        public string Namespace { set; get; }
        public string BaseClass { set; get; }
        public string ClassPrefix { set; get; }
        public string ClassSuffix { set; get; }
    }

    public class DALConfig
    {
        public string HeaderNote { set; get; }
        public string Using { set; get; }
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
