using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DataLayer.Base
{
    public abstract class BaseTableHelper
    {
        public static string ConnectionString { get; }

        private static SqlConnection _connection;

        protected static SqlConnection connection => _connection ?? (_connection = GetOpenConnection());

        static BaseTableHelper()
        {
            // 添加json配置文件路径
#if LOCAL
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.Local.json");
#elif DEBUG
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.Development.json");
#else
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
#endif
            // 创建配置根对象
            var configurationRoot = builder.Build();
            ConnectionString = configurationRoot.GetSection("DBConn").Value;
        }

        protected static SqlConnection GetOpenConnection(bool mars = false)
        {
            var cs = ConnectionString;
            if (mars)
            {
                var scsb = new SqlConnectionStringBuilder(cs)
                {
                    MultipleActiveResultSets = true
                };
                cs = scsb.ConnectionString;
            }
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }

        protected static SqlConnection GetClosedConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            if (conn.State != ConnectionState.Closed) throw new InvalidOperationException("should be closed!");
            return conn;
        }

        protected static PageDataView<T> Paged<T>(string tableName, string where, string orderBy, string columns, int pageSize, int currentPage)
        {
            var result = new PageDataView<T>();
            var count_sql = string.Format("SELECT COUNT(1) FROM {0}", tableName);
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "id desc";
            }
            if (!string.IsNullOrWhiteSpace(where))
            {
                if (where.ToLower().Contains("where"))
                {
                    throw new ArgumentException("where子句不需要带where关键字");
                }
                where = " WHERE " + where;
            }

            var sql = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS Row, {0} FROM {2} {3}) AS Paged ", columns, orderBy, tableName, where);
            var pageStart = (currentPage - 1) * pageSize;
            sql += string.Format(" WHERE Row >{0} AND Row <={1}", pageStart, pageStart + pageSize);
            count_sql += where;
            using (var conn = GetOpenConnection())
            {
                result.TotalRecords = connection.ExecuteScalar<int>(count_sql);
                result.TotalPages = result.TotalRecords / pageSize;
                if (result.TotalRecords % pageSize > 0)
                    result.TotalPages += 1;
                var list = connection.Query<T>(sql);
                result.Items = list.Count() == 0 ? (new List<T>()) : list.ToList();
            }

            return result;
        }

        protected static PageDataView<T> JoinPaged<T>(string tableName, string where, string orderBy, string columns, int pageSize, int currentPage, string tableAlias, string joinStr)
        {
            var result = new PageDataView<T>();
            var count_sql = string.Format("SELECT COUNT(1) FROM {0}", tableName);
            if (!string.IsNullOrWhiteSpace(joinStr))
            {
                count_sql = string.Format("SELECT COUNT(1) FROM {0} {1}", tableName, joinStr);
            }
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "id desc";
                if (!string.IsNullOrWhiteSpace(tableAlias))
                {
                    orderBy = $"{tableAlias}.id desc";
                }
            }
            if (!string.IsNullOrWhiteSpace(where))
            {
                if (where.ToLower().Contains("where"))
                {
                    throw new ArgumentException("where子句不需要带where关键字");
                }
                where = " WHERE " + where;
            }

            var sql = string.Format("SELECT Paged.* FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS Row, {0} FROM {2} {4} {3}) AS Paged ", columns, orderBy, tableName, where, joinStr);
            var pageStart = (currentPage - 1) * pageSize;
            sql += string.Format(" WHERE Row >{0} AND Row <={1}", pageStart, pageStart + pageSize);
            count_sql += where;
            using (var conn = GetOpenConnection())
            {
                result.TotalRecords = connection.ExecuteScalar<int>(count_sql);
                result.TotalPages = result.TotalRecords / pageSize;
                if (result.TotalRecords % pageSize > 0)
                    result.TotalPages += 1;
                var list = connection.Query<T>(sql);
                result.Items = list.Count() == 0 ? (new List<T>()) : list.ToList();
            }

            return result;
        }
    }
}
