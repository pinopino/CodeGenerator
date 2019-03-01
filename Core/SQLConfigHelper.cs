using Generator.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Generator.Core
{
    public class SQLMetaDataHelper
    {
        private static readonly string _project = ConfigurationManager.AppSettings["Project"] ?? "LuckyStar";
        private static readonly string _basePath = ConfigurationManager.AppSettings["OutputBasePath"] ?? "d:\\output";
        private static readonly string _headerNode_default = "/*{0} *  {1}{0} *  本文件由生成工具自动生成，请勿随意修改内容除非你很清楚自己在做什么！{0} */{0}";
        private static readonly string _using_default = "using System;using System.Collections.Generic;using System.Linq;using System.Text;{0}";
        private static readonly string _baseClass_default = string.Empty;
        private static readonly string _classPrefix_default = string.Empty;
        private static readonly string _classSuffix_default = string.Empty;
        private static readonly string _methods_default = "Exists,Insert,Delete,Update,GetModel,GetList,GetRecordCount,GetListByPage";
        private static readonly string _exceptTables_default = ConfigurationManager.AppSettings["ExceptTables"] ?? string.Empty;
        private static readonly string _traceFieldTables_default = ConfigurationManager.AppSettings["TraceFieldTabls"] ?? string.Empty;
        private static readonly string _exceptColumns_default = ConfigurationManager.AppSettings["UpdateExceptColumns"] ?? string.Empty;
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

        public static void InitConfig(SQLMetaData config)
        {
            var model_headerNode = ConfigurationManager.AppSettings["Model_HeaderNote"] ?? _headerNode_default;
            var model_using = ConfigurationManager.AppSettings["Model_Using"] ?? string.Format(_using_default, string.Empty);
            var model_namespace = ConfigurationManager.AppSettings["Model_Namespace"] ?? "Model";
            var model_baseClass = ConfigurationManager.AppSettings["Model_BaseClass"] ?? _baseClass_default;
            var model_classPrefix = ConfigurationManager.AppSettings["Model_ClassPrefix"] ?? _classPrefix_default;
            var model_classSuffix = ConfigurationManager.AppSettings["Model_ClassSuffix"] ?? _classSuffix_default;

            var dal_headerNode = ConfigurationManager.AppSettings["DAL_HeaderNote"] ?? _headerNode_default;
            var dal_using = ConfigurationManager.AppSettings["DAL_Using"] ?? string.Format(_using_default, "using Dapper;");
            dal_using += string.Format("using {0}.{1};", _project, model_namespace);
            var dal_namespace = ConfigurationManager.AppSettings["DAL_Namespace"] ?? "DAL";
            var dal_baseClass = ConfigurationManager.AppSettings["DAL_BaseClass"] ?? _baseClass_default;
            var dal_classPrefix = ConfigurationManager.AppSettings["DAL_ClassPrefix"] ?? _classPrefix_default;
            var dal_classSuffix = ConfigurationManager.AppSettings["DAL_ClassSuffix"] ?? _classSuffix_default;
            var dal_methods = ConfigurationManager.AppSettings["DAL_Methods"] ?? _methods_default;

            config.PartialCheck_DAL_Path = ConfigurationManager.AppSettings["PartialCheck_DAL_Path"] ?? _partial_check_dal_path;
            config.ExceptTables = _exceptTables_default.Replace('；', ';').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            config.TraceFieldTables = _traceFieldTables_default.Replace('；', ';').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            config.ExceptColumns = _exceptColumns_default
                .Replace('；', ';')
                .Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .ToDictionary(p => p.Split(':')[0], p => p.Split(':')[1].Split(',').ToList());
            config.Model_HeaderNote = string.Format(model_headerNode, Environment.NewLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            config.Model_Using = model_using.Replace('；', ';').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(p => p + ";").ToList();
            config.Model_Namespace = string.Format("namespace {0}.{1}", _project, model_namespace);
            config.Model_BaseClass = model_baseClass;
            config.Model_ClassNamePrefix = model_classPrefix;
            config.Model_ClassNameSuffix = model_classSuffix;

            config.DAL_HeaderNote = string.Format(dal_headerNode, Environment.NewLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            config.DAL_Using = dal_using.Replace('；', ';').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(p => p + ";").ToList();
            config.DAL_Namespace = string.Format("namespace {0}.{1}", _project, dal_namespace);
            config.DAL_BaseClass = dal_baseClass;
            config.DAL_ClassNamePrefix = dal_classPrefix;
            config.DAL_ClassNameSuffix = dal_classSuffix;
            config.DAL_Methods = dal_methods.Replace('，', ',').Split(',').ToList();
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
            if (Directory.Exists(_basePath))
            {
                DeleteDirectory(_basePath);
            }
            else
            {
                Directory.CreateDirectory(_basePath);
            }

            File.AppendAllText(Path.Combine(_basePath, "sql_config.config"), FormatJsonStr(content), Encoding.GetEncoding("gb2312"));
            if (enableProgress)
            {
                ConsoleProgressBar progress = GetProgressBar();
                ProgressPrint(progress, 100, 100);
            }
        }

        public static void OutputDAL(SQLMetaData config, bool enableProgress = true)
        {
            var path = Path.Combine(_basePath, "DAL");
            Directory.CreateDirectory(path);

            ConsoleProgressBar progress = null;
            if (enableProgress)
            {
                progress = GetProgressBar();
            }

            var sb = new StringBuilder();
            var g = new DALGenerator(config);
            // 解析
            for (int i = 0; i < config.Tables.Count; i++)
            {
                var table = config.Tables[i];
                if (config.ExceptTables.Contains(table.Name))
                {
                    continue;
                }
                sb.Append(config.DAL_HeaderNote);
                sb.Append(string.Join(Environment.NewLine, config.DAL_Using));
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine(config.DAL_Namespace);
                sb.AppendLine("{");
                sb.AppendLine(string.Format("{0}public partial class {1}{2}{3}{4}",
                        '\t',
                        config.DAL_ClassNamePrefix,
                        table.Name,
                        config.DAL_ClassNameSuffix,
                        string.IsNullOrWhiteSpace(config.DAL_BaseClass) ? string.Empty : (" : " + config.DAL_BaseClass)));
                sb.AppendLine(string.Format("{0}{{", '\t'));
                sb.Append(string.Format("{0}{0}private static List<string> _all_fields = new List<string> {{ ", '\t'));
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    if (!table.Columns[j].IsIdentity)
                    {
                        if (!IsExceptColumn(config, table.Name, table.Columns[j].Name))
                        {
                            if (j != table.Columns.Count - 1)
                            {
                                sb.Append(string.Format("\"{0}\", ", table.Columns[j].Name));
                            }
                            else
                            {
                                sb.Append(string.Format("\"{0}\" ", table.Columns[j].Name));
                            }
                        }
                    }
                }
                sb.Append("};");
                sb.AppendLine();
                sb.AppendLine();
                // 按方法生成
                foreach (var item in config.DAL_Methods)
                {
                    switch (item.ToLower())
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
                        case "getlistbypage":
                            {
                                sb.Append(g.Get_GetListByPage(table.Name));
                            }
                            break;
                    }
                }
                sb.AppendLine(string.Format("{0}}}", '\t'));
                sb.AppendLine("}");

                File.AppendAllText(Path.Combine(path, string.Format("{0}Helper.cs", table.Name)), sb.ToString());
                sb.Clear();

                if (progress != null)
                {
                    // 打印进度
                    ProgressPrint(progress, (i + 1), config.Tables.Count);
                }
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "DAL"), path);
        }

        public static void OutputModel(SQLMetaData config, bool enableProgress = true)
        {
            var path = Path.Combine(_basePath, "Model");
            Directory.CreateDirectory(path);

            ConsoleProgressBar progress = null;
            if (enableProgress)
            {
                progress = GetProgressBar();
            }

            var sb = new StringBuilder();
            var g = new ModelGenerator(config);
            // 解析
            for (int i = 0; i < config.Tables.Count; i++)
            {
                var table = config.Tables[i];
                if (config.ExceptTables.Contains(table.Name))
                {
                    continue;
                }
                sb.Append(config.Model_HeaderNote);
                sb.Append(string.Join(Environment.NewLine, config.Model_Using));
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine(config.Model_Namespace);
                sb.AppendLine("{");
                sb.AppendLine(g.Get_Class(table.Name));
                sb.AppendLine("}");

                File.AppendAllText(Path.Combine(path, string.Format("{0}.cs", table.Name)), sb.ToString());
                sb.Clear();

                if (progress != null)
                {
                    // 打印进度
                    ProgressPrint(progress, (i + 1), config.Tables.Count);
                }
            }

            // 拷贝公用文件到指定目录
            DirHelper.CopyDirectory(Path.Combine("CopyFiles", "Model"), path);
        }

        public static void OutputEnum(SQLMetaData config, bool enableProgress = true)
        {
            var path = Path.Combine(_basePath, "Enum");
            Directory.CreateDirectory(path);

            ConsoleProgressBar progress = null;
            if (enableProgress)
            {
                progress = GetProgressBar();
            }

            var sb = new StringBuilder();
            var g = new EnumGenerator(config);
            // 解析
            var regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            for (int i = 0; i < config.Tables.Count; i++)
            {
                var table = config.Tables[i];
                if (config.ExceptTables.Contains(table.Name))
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
                            sb.Append(config.Model_HeaderNote);
                            sb.Append(string.Join(Environment.NewLine, config.Model_Using));
                            sb.AppendLine();
                            sb.AppendLine();
                            sb.AppendLine(string.Format("namespace {0}.{1}", _project, "GenEnum"));
                            sb.AppendLine("{");
                            sb.AppendLine(g.Get_Enum(enum_name, arrs, column.DbType));
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
                    ProgressPrint(progress, (i + 1), config.Tables.Count);
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
