using Dapper;
using Generator.Common;
using Generator.Core.Config;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Generator.Core.Oracle
{
    public class Parser : BaseParser
    {
        class FieldViewModel
        {
            public string Field { get; set; }
            public string Type { get; set; }
            public dynamic Collation { get; set; }
            public string Null { get; set; }
            public string Key { get; set; }
            public dynamic Default { get; set; }
            public string Extra { get; set; }
            public string Privileges { get; set; }
            public string Comment { get; set; }
        }

        public Parser(GlobalConfiguration config, IProgressBar progress)
            : base(config, progress)
        { }

        public override Dictionary<string, TableMetaData> ParseMetadata()
        {
            //TODO:模板文件还要写，具体需要新加一个序列值生成的方法，insert的时候需要用到
            var result = new Dictionary<string, TableMetaData>();
            List<TableFileds> TableNames = GetUserAllTables(base.ConnStr);//获取到该用户能获取到的表
            List<TablePKColumn> PKs = GetAllTablePKColumn(base.ConnStr);
            var count = 0;
            foreach (var info in TableNames)
            {///这里会循环去查数据库，生成代码也不用管什么性能了。
                var table = new TableMetaData(info.table_name, info.comments);
                List<TableColumns> columns = GetTableColumns(info.table_name, base.ConnStr);
                var pk = PKs.Where(p => p.table_name == info.table_name).FirstOrDefault();
                if (pk == null)
                {
                    pk = new TablePKColumn();
                    pk.column_name = "0";
                }
                foreach (var item in columns)
                {
                    if (item.data_type == "NUMBER")
                    {
                        if (item.data_length == 1)
                        {
                            item.data_type = "NUMBER(1)";
                        }
                    }
                    var ColumData = new ColumnMetaData
                    {
                        Comment = item.comments,
                        DbType = MapCsharpType(item.data_type),
                        HasDefaultValue = false,//默认值甚至可能是个函数，不读了
                        IsIdentity = false,//oracle 是用sequence完成自增
                        IsPrimaryKey = pk.column_name == item.column_name ? true : false,
                        TableName = table.Name,
                        Name = item.column_name,
                        Nullable = item.nullable == "Y" ? true : false
                    };

                    if (pk.column_name == item.column_name)
                    {
                        if (table.PrimaryKeyPair.HasValue)
                            throw new InvalidOperationException("最多支持两个列的复合主键!");

                        if (table.PrimaryKey == null)
                        {
                            table.PrimaryKey = ColumData;
                        }
                        else
                        {
                            table.PrimaryKeyPair = new PrimaryPair { Item1 = table.PrimaryKey, Item2 = ColumData };
                            table.PrimaryKey = null;
                        }
                    }
                    table.Columns.Add(ColumData);

                }
                result.Add(table.Name, table);

                // 打印进度
                ProgressPrint(++count, TableNames.Count);
            }

            return result;
        }

        protected override string FindDBName(string connStr)
        {
            return "";
        }

        protected override string MapCsharpType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            string csharpType = "object";
            switch (dbtype)
            {
                case "BFILE": csharpType = "byte[]"; break;
                case "BLOB": csharpType = "byte[]"; break;
                case "CHAR": csharpType = "string"; break;
                case "CLOB": csharpType = "string"; break;
                case "DATE": csharpType = "DateTime"; break;
                case "DATETIME": csharpType = "DateTime"; break;
                case "LONG": csharpType = "string"; break;
                case "LONG RAW": csharpType = "byte[]"; break;
                case "INTEGER": csharpType = "decimal"; break;
                case "FLOAT": csharpType = "decimal"; break;
                case "NCHAR": csharpType = "string"; break;
                case "NCLOB": csharpType = "string"; break;
                case "NUMBER": csharpType = "decimal"; break;
                case "NVARCHAR2": csharpType = "string"; break;
                case "RAW": csharpType = "byte[]"; break;
                case "ROWID": csharpType = "string"; break;
                case "TIMESTAMP": csharpType = "DateTime"; break;
                case "VARCHAR2": csharpType = "string"; break;
                case "INTERVAL DAY TO  SECOND": csharpType = "TimeSpan"; break;
                case "INTERVAL YEAR TO  MONTH": csharpType = "int"; break;
                case "NUMBER(1)": csharpType = "bool"; break;
                default: csharpType = "object"; break;
            }
            return csharpType;
        }

        /// <summary>
        /// 获取数据表名和备注
        /// </summary>
        private static List<TableFileds> GetUserAllTables(string conn_str)
        {
            var sql = new StringBuilder();
            sql.Append("select a.TABLE_NAME, b.COMMENTS from user_tables a, user_tab_comments b");
            sql.Append(" where a.TABLE_NAME = b.TABLE_NAME order by TABLE_NAME");
            List<TableFileds> ret;
            using (var conn = GetOpenConnection(conn_str))
            {
                ret = conn.Query<TableFileds>(sql.ToString()).ToList();
            }

            return ret;
        }

        /// <summary>
        /// 获取到表的字段名，类型，长度和介绍
        /// </summary>
        private static List<TableColumns> GetTableColumns(string tableName, string conn_str)
        {
            var sql = new StringBuilder();
            sql.Append("SELECT a.column_name,a.data_type,a.data_length,b.comments,a.nullable");
            sql.Append(" FROM user_tab_columns a, user_col_comments b ");
            sql.Append(" WHERE a.table_name = b.table_name and b.column_name=a.column_name and a.table_name = :tableName");
            List<TableColumns> ret;
            using (var conn = GetOpenConnection(conn_str))
            {
                ret = conn.Query<TableColumns>(sql.ToString(), new { @tableName = tableName }).ToList();
            }

            return ret;
        }

        private static List<TablePKColumn> GetAllTablePKColumn(string conn_str)
        {
            var sql = new StringBuilder();
            sql.Append("select  col.table_name,col.column_name from user_constraints con,user_cons_columns col where con.constraint_name=col.constraint_name and con.constraint_type='P'");
            List<TablePKColumn> ret;
            using (var conn = GetOpenConnection(conn_str))
            {
                ret = conn.Query<TablePKColumn>(sql.ToString()).ToList();
            }

            return ret;
        }

        private static OracleConnection GetOpenConnection(string conn_str)
        {
            var connection = new OracleConnection(conn_str);
            connection.Open();
            return connection;
        }
    }
}
