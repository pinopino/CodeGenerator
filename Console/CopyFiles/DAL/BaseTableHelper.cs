using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Poker.DataLayer
{
    public abstract class BaseTableHelper
    {
        public static string ConnectionString;

        static BaseTableHelper()
        {
            ConnectionString = UnitWork.Configure<DbConnect>("DbConnect").ConnectString;
        }

        private static SqlConnection _connection;

        private static SqlConnection connection => _connection ?? (_connection = GetOpenConnection());


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
            if (!string.IsNullOrEmpty(where))
            {
                if (!where.Trim().StartsWith("where", StringComparison.CurrentCultureIgnoreCase))
                {
                    where = " WHERE " + where;
                }
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
                result.Items = connection.Query<T>(sql).ToList();
            }

            return result;
        }
    }

    public class DbConnect
    {
        public string ConnectString { get; set; }
    }

    public static class UnitWork
    {
        private static IServiceProvider serviceProvider { get; set; }

        public static void SetServiceProvider(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
        }

        public static TEntity Configure<TEntity>(string section) where TEntity : class, new()
        {
            var option = serviceProvider.GetService<IOptions<TEntity>>();
            return option.Value;
        }
    }
}
