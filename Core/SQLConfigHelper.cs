using Generator.Common;
using Generator.Core.Config;
using Generator.Core.MSSql;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Generator.Core
{
    public class SQLMetaDataHelper
    {
        private static string _project;
        private static string _outputpath;
        private static readonly string _project_default = "YourProject";
        private static readonly string _outputpath_default = "c:\\output";
        private static readonly string _headerNode_default = "/*{0} *  {1}{0} *  本文件由生成工具自动生成，请勿随意修改内容除非你很清楚自己在做什么！{0} */{0}";
        private static readonly string _using_default = "using System;using System.Collections.Generic;using System.Linq;using System.Text;{0}";
        private static readonly string _baseClass_default = string.Empty;
        private static readonly string _classPrefix_default = string.Empty;
        private static readonly string _classSuffix_default = string.Empty;
        private static readonly List<string> _methods_default = new List<string> { "Exists", "Insert", "Delete", "Update", "GetModel", "GetList", "GetCount", "GetPage" };
        private static readonly string _partial_check_dal_path = string.Empty;
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
        private static GlobalConfiguration _configuration;
        public static GlobalConfiguration Config { get { return _configuration; } }

        public static void InitConfig(SQLMetaData config)
        {
            _project = _configuration.Project ?? _project_default;
            _outputpath = _configuration.OutputBasePath ?? _outputpath_default;

            var model_headerNode = _configuration.ModelConfig.HeaderNote ?? _headerNode_default;
            var model_using = _configuration.ModelConfig.Using ?? string.Format(_using_default, string.Empty);
            var model_namespace = _configuration.ModelConfig.Namespace ?? "Model";
            var model_baseClass = _configuration.ModelConfig.BaseClass ?? _baseClass_default;
            var model_classPrefix = _configuration.ModelConfig.ClassPrefix ?? _classPrefix_default;
            var model_classSuffix = _configuration.ModelConfig.ClassSuffix ?? _classSuffix_default;

            var dal_headerNode = _configuration.DALConfig.HeaderNote ?? _headerNode_default;
            var dal_using = _configuration.DALConfig.Using ?? string.Format(_using_default, "using Dapper;");
            dal_using += string.Format("using {0}.{1};", _project, model_namespace);
            var dal_namespace = _configuration.DALConfig.Namespace ?? "DAL";
            var dal_baseClass = _configuration.DALConfig.BaseClass ?? _baseClass_default;
            var dal_classPrefix = _configuration.DALConfig.ClassPrefix ?? _classPrefix_default;
            var dal_classSuffix = _configuration.DALConfig.ClassSuffix ?? _classSuffix_default;
            var dal_methods = _configuration.DALConfig.Methods.Select(p => p.Name) ?? _methods_default;

            config.Model_HeaderNote = string.Format(model_headerNode, Environment.NewLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            config.Model_Using = model_using.Replace('；', ';').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(p => p + ";").ToList();
            config.Model_Namespace = string.Format("{0}.{1}", _project, model_namespace);
            config.Model_BaseClass = model_baseClass;
            config.Model_ClassNamePrefix = model_classPrefix;
            config.Model_ClassNameSuffix = model_classSuffix;

            config.DAL_HeaderNote = string.Format(dal_headerNode, Environment.NewLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            config.DAL_Using = dal_using.Replace('；', ';').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(p => p + ";").ToList();
            config.DAL_Namespace = string.Format("{0}.{1}", _project, dal_namespace);
            config.DAL_BaseClass = dal_baseClass;
            config.DAL_ClassNamePrefix = dal_classPrefix;
            config.DAL_ClassNameSuffix = dal_classSuffix;
            config.DAL_Methods = dal_methods.ToList();

            config.PartialCheck_DAL_Path = _configuration.PartialCheck_DAL_Path ?? _partial_check_dal_path;
            config.TraceFieldTables = _configuration.TraceFieldTables == null ? Enumerable.Empty<string>().ToList() : _configuration.TraceFieldTables.Select(p => p.Name).ToList();
            config.EntityTables = _configuration.EntityTables == null ? Enumerable.Empty<string>().ToList() : _configuration.EntityTables.Select(p => p.Name).ToList();
            config.ExceptTables = _configuration.ExceptTables == null ? Enumerable.Empty<string>().ToList() : _configuration.ExceptTables.Select(p => p.Name).ToList();
            config.ExceptColumns = _configuration.UpdateExceptColumns == null ?
                new Dictionary<string, List<string>>() :
                _configuration.UpdateExceptColumns.ToDictionary(p => p.Key, p => p.Value.Select(k => k.ColumnName).ToList());
            config.JoinedTables = _configuration.JoinedTables == null ?
                new Dictionary<string, Tuple<string, string>>() :
                _configuration.JoinedTables.ToDictionary(p => p.Table_Main.Name, p => Tuple.Create(p.Table_Sub.Name, p.Sub_InnerName));
        }

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

        public static void OutputConfig(string content, bool enableProgress = true)
        {
            if (Directory.Exists(_outputpath))
            {
                DeleteDirectory(_outputpath);
            }
            else
            {
                Directory.CreateDirectory(_outputpath);
            }

            File.AppendAllText(Path.Combine(_outputpath, "sql_config.config"), FormatJsonStr(content), Encoding.GetEncoding("gb2312"));
            if (enableProgress)
            {
                ConsoleProgressBar progress = GetProgressBar();
                ProgressPrint(progress, 100, 100);
            }
        }

        public static void OutputDAL(GlobalConfiguration config, Dictionary<string, TableMetaData> tables, bool enableProgress = true)
        {
            var path = Path.Combine(_outputpath, "DAL");
            Directory.CreateDirectory(path);

            ConsoleProgressBar progress = null;
            if (enableProgress)
            {
                progress = GetProgressBar();
            }

            var sb = new StringBuilder();
            var g = new DALGenerator(config, tables);
            // 解析
            var i = 0;
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables.Any(p => p.Name == table.Name))
                {
                    continue;
                }
                sb.Append(config.DALConfig.HeaderNote);
                sb.AppendLine(string.Join(Environment.NewLine, config.DALConfig.Using));
                sb.AppendLine($"using {config.DALConfig.Namespace}.Metadata;");
                if (config.JoinedTables.Count > 0)
                {
                    sb.AppendLine($"using {config.ModelConfig.Namespace}.JoinedViewModel;");
                }
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
                var join_info = config.JoinedTables.FirstOrDefault(p => p.Table_Main.Name == table.Name);
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

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "DAL"), path);
        }

        public static void OutputModel(GlobalConfiguration config, Dictionary<string, TableMetaData> tables, bool enableProgress = true)
        {
            var path = Path.Combine(_outputpath, "Model");
            Directory.CreateDirectory(path);

            ConsoleProgressBar progress = null;
            if (enableProgress)
            {
                progress = GetProgressBar();
            }

            var sb = new StringBuilder();
            var g = new ModelGenerator(config, tables);
            // 解析
            var i = 0;
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables.Any(p => p.Name == table.Name))
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
            if (config.JoinedTables.Count > 0)
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
            if (config.EntityTables.Count > 0)
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

        public static void OutputEnum(GlobalConfiguration config, Dictionary<string, TableMetaData> tables, bool enableProgress = true)
        {
            var path = Path.Combine(_outputpath, "Enum");
            Directory.CreateDirectory(path);

            ConsoleProgressBar progress = null;
            if (enableProgress)
            {
                progress = GetProgressBar();
            }

            var sb = new StringBuilder();
            var g = new EnumGenerator(config, tables);
            // 解析
            var regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var i = 0;
            foreach (var key in tables.Keys)
            {
                var table = tables[key];
                if (config.ExceptTables.Any(p => p.Name == table.Name))
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
                            sb.AppendLine(string.Format("namespace {0}.{1}", _project, "GenEnum"));
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

        public static void DoPartialCheck(SQLMetaData config)
        {
            if (string.IsNullOrWhiteSpace(config.PartialCheck_DAL_Path))
            {
                return;
            }
            var partial_path = Path.Combine(config.PartialCheck_DAL_Path, "partial");
            var partial_files = Directory.GetFiles(partial_path);
            var list = InnerCheckPartial(config, partial_files);
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

        public static ConsoleProgressBar GetProgressBar()
        {
            return new ConsoleProgressBar(Console.CursorLeft, Console.CursorTop, 50, ProgressBarType.Character);
        }

        private static void ProgressPrint(ConsoleProgressBar progress, long index, long total)
        {
            progress.Dispaly(Convert.ToInt32((index / (total * 1.0)) * 100));
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

        private static List<string> InnerCheckPartial(SQLMetaData config, string[] partial_file_names)
        {
            var ret = new List<string>();
            var regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            // 先检测一波表是否能对应
            for (int i = 0; i < partial_file_names.Length; i++)
            {
                var file1 = partial_file_names[i];
                var tmp_file_name = file1.Substring(file1.LastIndexOf('\\') + 1).Replace("Helper.cs", "");
                var table = config.Tables.Find(p => p.Name == tmp_file_name);
                if (table == null)
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
                var table = config.Tables.Find(p => p.Name == tmp_file_name);
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

        private static bool IsExceptColumn(SQLMetaData config, string table, string colunm)
        {
            return config.ExceptColumns.ContainsKey("*") && config.ExceptColumns["*"].Contains(colunm) ||
                config.ExceptColumns.ContainsKey(table) && config.ExceptColumns[table].Contains(colunm);
        }
    }
}
