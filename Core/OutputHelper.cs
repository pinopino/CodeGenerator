using Generator.Common;
using Generator.Core.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Generator.Core
{
    public sealed partial class OutputHelper
    {
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
