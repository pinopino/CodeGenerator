using Generator.Common;
using Generator.Core.Config;
using Generator.Core.MSSql;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Generator.Core
{
    public class OutputHelper
    {
        private static readonly List<string> _exist_enum = new List<string>();
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

        public static void OutputConfig(string content, GlobalConfiguration config, IProgressBar progress = null)
        {
            if (Directory.Exists(config.OutputBasePath))
            {
                DeleteDirectory(config.OutputBasePath);
            }
            else
            {
                Directory.CreateDirectory(config.OutputBasePath);
            }

            File.AppendAllText(Path.Combine(config.OutputBasePath, "sql_config.config"), FormatJsonStr(content), Encoding.GetEncoding("gb2312"));
            if (progress != null)
            {
                progress.Reset();
                ProgressPrint(progress, 100, 100);
            }
        }

        public static void OutputDAL(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            if (progress != null)
            {
                progress.Reset();
            }
            var path = Path.Combine(config.OutputBasePath, "DAL");
            Directory.CreateDirectory(path);

            var sb = new StringBuilder();
            BaseGenerator_DAL g = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    g = new Generator.Core.MSSql.DALGenerator(config, tables);
                    break;
                case "mysql":
                    throw new NotImplementedException();
                    break;
            }
            // 解析
            var i = 0;
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables != null && config.ExceptTables.Any(p => p.Name == table.Name))
                {
                    continue;
                }
                sb.Append(config.DALConfig.HeaderNote);
                sb.AppendLine(string.Join(Environment.NewLine, config.DALConfig.Using));
                sb.AppendLine();
                sb.AppendLine($"namespace {config.DALConfig.Namespace}");
                sb.AppendLine("{");
                sb.AppendLine(g.Get_MetaData1(table.Name));
                sb.AppendLine(string.Format("{0}public partial class {1}{2}{3}{4}",
                        '\t',
                        config.DALConfig.ClassPrefix,
                        table.Name,
                        config.DALConfig.ClassSuffix,
                        string.IsNullOrWhiteSpace(config.DALConfig.BaseClass) ? string.Empty : (" : " + config.DALConfig.BaseClass)));
                sb.AppendLine(string.Format("{0}{{", '\t'));
                var join_info = config.JoinedTables == null ? null : config.JoinedTables.FirstOrDefault(p => p.Table_Main.Name == table.Name);
                if (join_info != null)
                {
                    sb.AppendLine(g.Get_MetaData2(join_info));
                }
                sb.AppendLine(g.Get_MetaData3(table.Name));
                // 按方法生成
                foreach (var item in config.DALConfig.Methods)
                {
                    switch (item.Name.ToLower())
                    {
                        case "exists":
                            {
                                sb.AppendLine(g.Get_Exists(table.Name));
                            }
                            break;
                        case "insert":
                            {
                                sb.AppendLine(g.Get_Insert(table.Name));
                            }
                            break;
                        case "delete":
                            {
                                sb.AppendLine(g.Get_Delete(table.Name));
                                sb.AppendLine(g.Get_BatchDelete(table.Name));
                            }
                            break;
                        case "update":
                            {
                                sb.AppendLine(g.Get_Update(table.Name));
                            }
                            break;
                        case "getmodel":
                            {
                                sb.AppendLine(g.Get_GetModel(table.Name));
                            }
                            break;
                        case "getlist":
                            {
                                sb.AppendLine(g.Get_GetList(table.Name));
                            }
                            break;
                        case "getcount":
                            {
                                sb.AppendLine(g.Get_Count(table.Name));
                            }
                            break;
                        case "getpage":
                            {
                                sb.Append(g.Get_GetListByPage(table.Name));
                            }
                            break;
                    }
                }
                // Joined
                if (join_info != null)
                {
                    sb.Append(g.Get_Joined(join_info));
                }
                sb.AppendLine(string.Format("{0}}}", '\t'));
                sb.AppendLine("}");

                File.AppendAllText(Path.Combine(path, string.Format("{0}Helper.cs", table.Name)), sb.ToString());
                sb.Clear();

                if (progress != null)
                {
                    // 打印进度
                    ProgressPrint(progress, (i + 1), tables.Count);
                }
            }

            // 生成BaseTableHelper、PageDataView
            File.AppendAllText(Path.Combine(path, "BaseTableHelper.cs"), g.Get_BaseTableHelper());
            File.AppendAllText(Path.Combine(path, "PageDataView.cs"), g.Get_PageDataView());

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "DAL"), path);
        }

        public static void OutputModel(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            if (progress != null)
            {
                progress.Reset();
            }
            var path = Path.Combine(config.OutputBasePath, "Model");
            Directory.CreateDirectory(path);

            var sb = new StringBuilder();
            BaseGenerator_Model g = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    g = new Generator.Core.MSSql.ModelGenerator(config, tables);
                    break;
                case "mysql":
                    throw new NotImplementedException();
                    break;
            }
            // 解析
            var i = 0;
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables != null && config.ExceptTables.Any(p => p.Name == table.Name))
                {
                    continue;
                }
                sb.Append(config.ModelConfig.HeaderNote);
                sb.AppendLine(string.Join(Environment.NewLine, config.ModelConfig.Using));
                sb.AppendLine($"using {config.DALConfig.Namespace};");
                sb.AppendLine($"using {config.DALConfig.Namespace}.Metadata;");
                sb.AppendLine();
                sb.AppendLine($"namespace {config.ModelConfig.Namespace}");
                sb.AppendLine("{");
                sb.AppendLine(g.Get_Class(table.Name));
                sb.AppendLine("}");

                File.AppendAllText(Path.Combine(path, string.Format("{0}.cs", table.Name)), sb.ToString());
                sb.Clear();

                if (progress != null)
                {
                    // 打印进度
                    ProgressPrint(progress, (i + 1), tables.Count);
                }
            }

            // 如果配置文件指定了JoinedTables，那么这里需要为这些关联表生成额外的包装model，
            // 路径：Model\JoinedViewModel
            if (config.JoinedTables != null && config.JoinedTables.Count > 0)
            {
                Directory.CreateDirectory(Path.Combine(path, "JoinedViewModel"));
                var sb2 = new StringBuilder();
                foreach (var map in config.JoinedTables)
                {
                    sb2.Append(config.ModelConfig.HeaderNote);
                    sb2.AppendLine(string.Join(Environment.NewLine, config.ModelConfig.Using));
                    sb.AppendLine($"using {config.DALConfig.Namespace};");
                    sb2.AppendLine($"using {config.DALConfig.Namespace}.Metadata;");
                    sb2.AppendLine();
                    sb2.AppendLine($"namespace {config.ModelConfig.Namespace}.JoinedViewModel");
                    sb2.AppendLine("{");
                    sb2.AppendLine(g.Get_Joined_Class(map));
                    sb2.AppendLine("}");

                    File.AppendAllText(Path.Combine(path, "JoinedViewModel", string.Format("{0}.cs", "Joined" + map.Table_Main.Name)), sb2.ToString());
                    sb2.Clear();
                }
            }

            // 如果配置文件指定了EntityTables，那么这里需要生成实现接口IEntity接口的model
            // 路径：Model\EntityModel
            if (config.EntityTables != null && config.EntityTables.Count > 0)
            {
                Directory.CreateDirectory(Path.Combine(path, "EntityModel"));
                var sb2 = new StringBuilder();
                foreach (var talbe in config.EntityTables)
                {
                    sb2.Append(config.ModelConfig.HeaderNote);
                    sb2.AppendLine(string.Join(Environment.NewLine, config.ModelConfig.Using));
                    sb.AppendLine($"using {config.DALConfig.Namespace};");
                    sb2.AppendLine($"using {config.DALConfig.Namespace}.Metadata;");
                    sb2.AppendLine();
                    sb2.AppendLine($"namespace {config.ModelConfig.Namespace}.EntityModel");
                    sb2.AppendLine("{");
                    sb2.AppendLine(g.Get_Entity_Class(talbe.Name));
                    sb2.AppendLine("}");

                    File.AppendAllText(Path.Combine(path, "EntityModel", string.Format("{0}.cs", "Entity" + talbe)), sb2.ToString());
                    sb2.Clear();
                }
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "Model"), path);
        }

        public static void OutputEnum(Dictionary<string, TableMetaData> tables, GlobalConfiguration config, IProgressBar progress = null)
        {
            if (progress != null)
            {
                progress.Reset();
            }
            var path = Path.Combine(config.OutputBasePath, "Enum");
            Directory.CreateDirectory(path);

            var sb = new StringBuilder();
            BaseGenerator_Enum g = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    g = new Generator.Core.MSSql.EnumGenerator(config, tables);
                    break;
                case "mysql":
                    throw new NotImplementedException();
                    break;
            }
            // 解析
            var regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var i = 0;
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables != null && config.ExceptTables.Any(p => p.Name == table.Name))
                {
                    continue;
                }

                foreach (var column in table.Columns)
                {
                    if (!string.IsNullOrWhiteSpace(column.Comment))
                    {
                        var match = regex.Match(column.Comment);
                        if (match.Success)
                        {
                            var comment = match.Value.Replace("：", " ").Replace("、", " ").Replace("。", " ").Replace("；", " ").Replace(".", " ").Replace(";", " ").Replace(":", " ");
                            var tempname = Regex.Replace(table.Name, @"\d", "").Replace("_", "");
                            var enum_name = string.Format("{0}_{1}_{2}", tempname, column.Name, "Enum");
                            if (_exist_enum.Contains(enum_name)) continue;
                            var arrs = comment.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            sb.Append(config.ModelConfig.HeaderNote);
                            sb.Append(string.Join(Environment.NewLine, config.ModelConfig.Using));
                            sb.AppendLine();
                            sb.AppendLine();
                            sb.AppendLine(string.Format("namespace {0}.{1}", config.Project, "GenEnum"));
                            sb.AppendLine("{");
                            sb.AppendLine(g.Get_Enum(enum_name, comment, arrs, column.DbType));
                            sb.AppendLine("}");
                            File.AppendAllText(Path.Combine(path, string.Format("{0}.cs", enum_name)), sb.ToString());
                            sb.Clear();
                            _exist_enum.Add(enum_name);
                        }
                    }
                }

                if (progress != null)
                {
                    // 打印进度
                    ProgressPrint(progress, (i + 1), tables.Count);
                }
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "Enum"), path);
        }

        // link: https://stackoverflow.com/questions/18596876/go-statements-blowing-up-sql-execution-in-net
        public static void ReCreateDB(GlobalConfiguration config, IProgressBar progress = null)
        {
            if (string.IsNullOrWhiteSpace(config.ReCreateDB.SQLFilePath) || config.ReCreateDB.DBs == null)
                return;

            IReCreateDB c = null;
            // todo: 有点丑陋，可以考虑走ioc
            switch (config.DBType)
            {
                case "mssql":
                    c = new Generator.Core.MSSql.ReCreator(config);
                    break;
                case "mysql":
                    throw new NotImplementedException();
                    break;
            }
            c.ReCreate();
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

        private static void DeleteDirectory(string target_dir, bool del_self = false)
        {
            var files = Directory.GetFiles(target_dir);
            var dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir, true);
            }

            if (del_self)
            {
                Directory.Delete(target_dir, false);
            }
        }

        private static string FormatJsonStr(string json)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(json);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return json;
            }
        }

        private static void ProgressPrint(IProgressBar progress, long index, long total)
        {
            progress.Dispaly(Convert.ToInt32((index / (total * 1.0)) * 100));
        }
    }
}
