using Generator.Common;
using Generator.Core.Config;
using Generator.Core.Inject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Generator.Core
{
    public class OutputHelper
    {
        private static List<IInjector> _plugins = new List<IInjector>();
        private static Dictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>
        {
            [typeof(byte)] = DbType.Byte,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(short)] = DbType.Int16,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(int)] = DbType.Int32,
            [typeof(uint)] = DbType.UInt32,
            [typeof(long)] = DbType.Int64,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(float)] = DbType.Single,
            [typeof(double)] = DbType.Double,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(bool)] = DbType.Boolean,
            [typeof(string)] = DbType.String,
            [typeof(char)] = DbType.StringFixedLength,
            [typeof(Guid)] = DbType.Guid,
            [typeof(DateTime)] = DbType.DateTime,
            [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
            [typeof(TimeSpan)] = DbType.Time,
            [typeof(byte[])] = DbType.Binary,
            [typeof(byte?)] = DbType.Byte,
            [typeof(sbyte?)] = DbType.SByte,
            [typeof(short?)] = DbType.Int16,
            [typeof(ushort?)] = DbType.UInt16,
            [typeof(int?)] = DbType.Int32,
            [typeof(uint?)] = DbType.UInt32,
            [typeof(long?)] = DbType.Int64,
            [typeof(ulong?)] = DbType.UInt64,
            [typeof(float?)] = DbType.Single,
            [typeof(double?)] = DbType.Double,
            [typeof(decimal?)] = DbType.Decimal,
            [typeof(bool?)] = DbType.Boolean,
            [typeof(char?)] = DbType.StringFixedLength,
            [typeof(Guid?)] = DbType.Guid,
            [typeof(DateTime?)] = DbType.DateTime,
            [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
            [typeof(TimeSpan?)] = DbType.Time,
            [typeof(object)] = DbType.Object
        };

        public static Type MapCommonType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return Type.Missing.GetType();
            dbtype = dbtype.ToLower();
            Type commonType = typeof(object);
            switch (dbtype)
            {
                case "bigint": commonType = typeof(long); break;
                case "binary": commonType = typeof(byte[]); break;
                case "bit": commonType = typeof(bool); break;
                case "char": commonType = typeof(string); break;
                case "date": commonType = typeof(DateTime); break;
                case "datetime": commonType = typeof(DateTime); break;
                case "datetime2": commonType = typeof(DateTime); break;
                case "datetimeoffset": commonType = typeof(DateTimeOffset); break;
                case "decimal": commonType = typeof(decimal); break;
                case "float": commonType = typeof(float); break;
                case "double": commonType = typeof(double); break;
                case "image": commonType = typeof(byte[]); break;
                case "int": commonType = typeof(int); break;
                case "money": commonType = typeof(decimal); break;
                case "nchar": commonType = typeof(string); break;
                case "ntext": commonType = typeof(string); break;
                case "numeric": commonType = typeof(decimal); break;
                case "nvarchar": commonType = typeof(string); break;
                case "real": commonType = typeof(Single); break;
                case "smalldatetime": commonType = typeof(DateTime); break;
                case "smallint": commonType = typeof(short); break;
                case "smallmoney": commonType = typeof(decimal); break;
                case "sql_variant": commonType = typeof(object); break;
                case "sysname": commonType = typeof(object); break;
                case "text": commonType = typeof(string); break;
                case "time": commonType = typeof(TimeSpan); break;
                case "timestamp": commonType = typeof(byte[]); break;
                case "tinyint": commonType = typeof(byte); break;
                case "uniqueidentifier": commonType = typeof(Guid); break;
                case "varbinary": commonType = typeof(byte[]); break;
                case "varchar": commonType = typeof(string); break;
                case "xml": commonType = typeof(string); break;
                default: commonType = typeof(object); break;
            }
            return commonType;
        }

        public static string MapCsharpType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            string csharpType = "object";
            switch (dbtype)
            {
                case "bigint": csharpType = "long"; break;
                case "binary": csharpType = "byte[]"; break;
                case "bit": csharpType = "bool"; break;
                case "char": csharpType = "string"; break;
                case "date": csharpType = "DateTime"; break;
                case "datetime": csharpType = "DateTime"; break;
                case "datetime2": csharpType = "DateTime"; break;
                case "datetimeoffset": csharpType = "DateTimeOffset"; break;
                case "decimal": csharpType = "decimal"; break;
                case "float": csharpType = "float"; break;
                case "double": csharpType = "double"; break;
                case "image": csharpType = "byte[]"; break;
                case "int": csharpType = "int"; break;
                case "money": csharpType = "decimal"; break;
                case "nchar": csharpType = "string"; break;
                case "ntext": csharpType = "string"; break;
                case "numeric": csharpType = "decimal"; break;
                case "nvarchar": csharpType = "string"; break;
                case "real": csharpType = "Single"; break;
                case "smalldatetime": csharpType = "DateTime"; break;
                case "smallint": csharpType = "short"; break;
                case "smallmoney": csharpType = "decimal"; break;
                case "sql_variant": csharpType = "object"; break;
                case "sysname": csharpType = "object"; break;
                case "text": csharpType = "string"; break;
                case "time": csharpType = "TimeSpan"; break;
                case "timestamp": csharpType = "byte[]"; break;
                case "tinyint": csharpType = "byte"; break;
                case "uniqueidentifier": csharpType = "Guid"; break;
                case "varbinary": csharpType = "byte[]"; break;
                case "varchar": csharpType = "string"; break;
                case "xml": csharpType = "string"; break;
                default: csharpType = "object"; break;
            }
            return csharpType;
        }

        public static void LoadPlugin(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            // links: https://stackoverflow.com/questions/10732933/can-i-use-activator-createinstance-with-an-interface
            var plugin_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CopyFiles");
            foreach (var dll in Directory.GetFiles(plugin_path, "*.dll"))
            {
                Type[] load_types = (from t in Assembly.LoadFile(dll).GetExportedTypes()
                                     where !t.IsInterface && !t.IsAbstract
                                     where typeof(IInjector).IsAssignableFrom(t)
                                     select t).ToArray();
                IInjector[] objs = load_types.Select(t => (IInjector)Activator.CreateInstance(t, tables, config)).ToArray();
                _plugins.AddRange(objs);
            }
            PrintProgress(progress, 100, 100);
        }

        public static void OutputConfig(string content, GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            if (Directory.Exists(config.OutputBasePath))
            {
                Directory.Delete(config.OutputBasePath, true);
            }
            else
            {
                Directory.CreateDirectory(config.OutputBasePath);
            }

            File.AppendAllText(Path.Combine(config.OutputBasePath, "sql_config.config"), content, Encoding.UTF8);
            PrintProgress(progress, 100, 100);
        }

        public static void OutputDAL(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            var path = Path.Combine(config.OutputBasePath, "DAL");
            Directory.CreateDirectory(path);

            var sb = new StringBuilder();
            BaseGenerator_DAL g = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    g = new Generator.Core.MSSql.DALGenerator(config);
                    break;
                case "mysql":
                    g = new Generator.Core.MySql.DALGenerator(config);
                    break;
            }

            // 解析
            var i = 0;
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables != null && config.ExceptTables.Any(p => p.Name == table.Name))
                    continue;

                sb.Append(g.RenderDALFor(table));
                // Joined
                var join_info = config.JoinedTables == null ? null : config.JoinedTables.FirstOrDefault(p => p.Table_Main.Name == table.Name);
                if (join_info != null)
                { }

                File.AppendAllText(Path.Combine(path, string.Format("{0}Helper.cs", table.Name)), sb.ToString());
                sb.Clear();
                PrintProgress(progress, ++i, tables.Count);
            }

            // 生成BaseTableHelper、PageDataView
            File.AppendAllText(Path.Combine(path, "BaseTableHelper.cs"), g.RenderBaseTableHelper());
            File.AppendAllText(Path.Combine(path, "PageDataView.cs"), g.RenderPageDataView());

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "DAL"), path);
        }

        public static void OutputModel(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            var path = Path.Combine(config.OutputBasePath, "Model");
            Directory.CreateDirectory(path);

            BaseGenerator_Model g = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    g = new Generator.Core.MSSql.ModelGenerator(config);
                    break;
                case "mysql":
                    g = new Generator.Core.MySql.ModelGenerator(config);
                    break;
            }

            // 解析
            var i = 0;
            var sb = new StringBuilder();
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables != null && config.ExceptTables.Any(p => p.Name == table.Name))
                    continue;

                sb.AppendLine(g.RenderModelFor(table));

                var new_str = sb.ToString();
                // 插件的执行顺序
                foreach (var plug in _plugins)
                {
                    if (!typeof(IModelInjector).IsAssignableFrom(plug.GetType()))
                        continue;
                    if (!plug.Check(table.Name))
                        continue;
                    new_str = plug.Inject(sb.ToString(), table.Name);
                    File.AppendAllText(Path.Combine(config.OutputBasePath, plug.Name, string.Format("{0}.cs", g.FileName)), new_str);
                    new_str = string.Empty;
                }

                File.AppendAllText(Path.Combine(path, string.Format("{0}.cs", g.FileName)), new_str);
                sb.Clear();
                PrintProgress(progress, ++i, tables.Count);
            }

            // 如果配置文件指定了JoinedTables，那么这里需要为这些关联表生成额外的包装model，
            // 路径：Model\JoinedViewModel
            if (config.JoinedTables != null && config.JoinedTables.Count > 0)
            {
                //Directory.CreateDirectory(Path.Combine(path, "JoinedViewModel"));
                //var sb2 = new StringBuilder();
                //foreach (var map in config.JoinedTables)
                //{
                //    sb2.AppendLine(g.Get_Join_Head(map));
                //    sb2.AppendLine(g.Get_Joined_Class(map));
                //    sb2.AppendLine(g.Get_Join_Tail(map));

                //    File.AppendAllText(Path.Combine(path, "JoinedViewModel", string.Format("{0}.cs", "Joined" + g.FileName)), sb2.ToString());
                //    sb2.Clear();
                //}
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "Model"), path);
        }

        public static void OutputEnum(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            var path = Path.Combine(config.OutputBasePath, "Enum");
            Directory.CreateDirectory(path);

            BaseGenerator_Enum g = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    g = new Generator.Core.MSSql.EnumGenerator(config);
                    break;
                case "mysql":
                    g = new Generator.Core.MySql.EnumGenerator(config);
                    break;
            }

            // 解析
            var i = 0;
            var sb = new StringBuilder();
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables != null && config.ExceptTables.Any(p => p.Name == table.Name))
                    continue;

                foreach (var column in table.Columns)
                {
                    if (!string.IsNullOrWhiteSpace(column.Comment))
                    {
                        sb.AppendLine(g.RenderEnumFor(table, column));
                        File.AppendAllText(Path.Combine(path, string.Format("{0}.cs", g.FileName)), sb.ToString());
                        sb.Clear();
                    }
                }
                PrintProgress(progress, ++i, tables.Count);
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "Enum"), path);
        }

        // link: https://stackoverflow.com/questions/18596876/go-statements-blowing-up-sql-execution-in-net
        public static void ReCreateDB(GlobalConfiguration config, IProgressBar progress = null)
        {
            ResetProgress(progress);
            if (string.IsNullOrWhiteSpace(config.ReCreateDB.SQLFilePath) || config.ReCreateDB.DBs == null)
            {
                PrintProgress(progress, 100, 100);
                return;
            }

            IReCreateDB c = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    c = new Generator.Core.MSSql.ReCreator(config);
                    break;
                case "mysql":
                    c = new Generator.Core.MySql.ReCreator(config);
                    break;
            }
            c.ReCreate();
            PrintProgress(progress, 100, 100);
        }

        public static void DoPartialCheck(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            if (string.IsNullOrWhiteSpace(config.PartialCheck_DAL_Path))
            {
                return;
            }
            var partial_path = Path.Combine(config.PartialCheck_DAL_Path, "partial");
            var partial_files = Directory.GetFiles(partial_path);
            var list = InnerCheckPartial(tables, partial_files, config);
            if (list.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("以下字段可能有问题：");
                list.ForEach(p => Console.WriteLine(p));
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("检测完毕");
            }
        }

        private static List<string> InnerCheckPartial(Dictionary<string, TableMetaData> tables, string[] partial_file_names, GlobalConfiguration config)
        {
            var ret = new List<string>();
            var regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            // 先检测一波表是否能对应
            for (int i = 0; i < partial_file_names.Length; i++)
            {
                var file1 = partial_file_names[i];
                var tmp_file_name = file1.Substring(file1.LastIndexOf('\\') + 1).Replace("Helper.cs", "");
                var exist = tables.ContainsKey(tmp_file_name);
                if (!exist)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("partial文件对应的表 [" + tmp_file_name + "] 不存在于当前数据库中，这极有可能是因为连接到错误的数据库引起的!");
                    Console.ResetColor();
                    return ret;
                }
            }

            for (int i = 0; i < partial_file_names.Length; i++)
            {
                var file1 = partial_file_names[i];
                var content1 = File.ReadAllText(file1);
                var matches1 = regex.Matches(content1);
                var key_words1 = new List<string>();
                foreach (Match item in matches1)
                {
                    if (item.Success)
                    {
                        if (key_words1.Find(p => p.ToLower() == item.Value.ToLower()) == null)
                        {
                            key_words1.Add(item.Value);
                        }
                    }
                }

                var tmp_file_name = file1.Substring(file1.LastIndexOf('\\') + 1).Replace("Helper.cs", "");
                var table = tables[tmp_file_name];
                var key_words2 = table.Columns.Select(p => p.Name.ToLower()).ToList();
                key_words2.Add(table.Name.ToLower());

                var sb = new StringBuilder();
                foreach (var item in key_words1)
                {
                    if (key_words2.Find(p => p == item.ToLower()) == null)
                    {
                        sb.Append(string.Format("{0}, ", item));
                    }
                }

                var str = sb.ToString().TrimEnd(", ");
                if (!string.IsNullOrWhiteSpace(str))
                {
                    ret.Add("[" + table.Name + "]: \r\n" + str);
                }
            }

            return ret;
        }

        private static void PrintProgress(IProgressBar progress, long index, long total)
        {
            if (progress != null)
            {
                progress.Dispaly(Convert.ToInt32((index / (total * 1.0)) * 100));
            }
        }

        private static void ResetProgress(IProgressBar progress)
        {
            if (progress != null)
                progress.Reset();
        }
    }
}
