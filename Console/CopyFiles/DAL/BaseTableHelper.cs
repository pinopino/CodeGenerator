using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataLayer.Base
{
    public interface IColumn
    {
        string Table { get; }
        string Name { get; }
        bool IsAddEqual { get; }
        string Asc { get; }
    }

    internal class PredicateParser : ExpressionVisitor
    {
        private StringBuilder sb = new StringBuilder();
        private bool invert = false;
        private bool quote = true;
        private bool boolean = false; // 布尔表达式的左侧表达式是否为一个bool类型
        private bool has_right = false;
        private bool is_left = false;

        public string Parse(Expression predicate)
        {
            this.Visit(predicate);
            return this.ToString();
        }

        public void Reset()
        {
            sb.Clear();
            invert = false;
            quote = true;
            boolean = false;
            has_right = false;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            has_right = true;
            sb.Append("(");
            is_left = true;
            this.Visit(node.Left);
            is_left = false;

            var tmp = invert;
            if (node.Left.NodeType == ExpressionType.Not)
                invert = invert ^ true;
            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append(invert ? " OR " : " AND ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sb.Append(invert ? " AND " : " OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(node.Right))
                    {
                        sb.Append(invert ? " IS NOT " : " IS ");
                    }
                    else
                    {
                        if (boolean)
                        {
                            sb.Append(" = ");
                        }
                        else
                        {
                            sb.Append(invert ? " <> " : " = ");
                        }
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(node.Right))
                    {
                        sb.Append(invert ? " IS " : " IS NOT ");
                    }
                    else
                    {
                        if (boolean)
                        {
                            sb.Append(" = ");
                            invert = true;
                        }
                        else
                        {
                            sb.Append(invert ? " = " : " != ");
                        }
                    }
                    break;

                case ExpressionType.LessThan:
                    sb.Append(invert ? " >= " : " < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    sb.Append(invert ? " > " : " <= ");
                    break;

                case ExpressionType.GreaterThan:
                    sb.Append(invert ? " <= " : " > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(invert ? " < " : " >= ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", node.NodeType));
            }

            this.Visit(node.Right);
            invert = tmp;
            sb.Append(")");
            has_right = false;
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    var tmp = invert;
                    invert = invert ^ true;
                    this.Visit(node.Operand);
                    invert = tmp;
                    break;
                case ExpressionType.Convert:
                    this.Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", node.NodeType));
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(node.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        if (is_left)
                        {
                            sb.Append(((bool)node.Value) ^ invert ? true : false);
                        }
                        else
                        {
                            sb.Append(((bool)node.Value) ^ invert ? 1 : 0);
                        }
                        break;

                    case TypeCode.String:
                        if (quote) sb.Append("'");
                        sb.Append(node.Value);
                        if (quote) sb.Append("'");
                        break;

                    case TypeCode.DateTime:
                        sb.Append("'");
                        sb.Append(node.Value);
                        sb.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", node.Value));

                    default:
                        sb.Append(node.Value);
                        break;
                }
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == null)
            {
                var value = ((FieldInfo)node.Member).GetValue(null);
                sb.Append(value);
            }
            else
            {
                // 闭包带进来的变量是生成类型的一个Field
                if (node.Expression.NodeType == ExpressionType.Constant)
                {
                    var container = ((ConstantExpression)node.Expression).Value;
                    var value = ((FieldInfo)node.Member).GetValue(container);
                    var coll = value as IList;
                    if (coll != null)
                    {
                        for (int i = 0; i < coll.Count; i++)
                        {
                            var item = coll[i];
                            if (i == coll.Count - 1)
                            {
                                if (item is string)
                                {
                                    sb.Append($"'{item}'");
                                }
                                else
                                {
                                    sb.Append($"{item}");
                                }
                            }
                            else
                            {
                                if (item is string)
                                {
                                    sb.Append($"'{item}', ");
                                }
                                else
                                {
                                    sb.Append($"{item}, ");
                                }
                            }
                        }
                    }
                    else
                    {
                        switch (Type.GetTypeCode(value.GetType()))
                        {
                            case TypeCode.Boolean:
                                if (is_left)
                                {
                                    sb.Append(((bool)value) ^ invert ? true : false);
                                }
                                else
                                {
                                    sb.Append(((bool)value) ^ invert ? 1 : 0);
                                }
                                break;

                            case TypeCode.String:
                                if (quote) sb.Append("'");
                                sb.Append(value);
                                if (quote) sb.Append("'");
                                break;

                            case TypeCode.DateTime:
                                sb.Append("'");
                                sb.Append(value);
                                sb.Append("'");
                                break;

                            case TypeCode.Object:
                                throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", value));

                            default:
                                sb.Append(value);
                                break;
                        }
                    }
                }

                if (node.Expression.NodeType == ExpressionType.Parameter)
                {
                    sb.Append($"[{node.Expression.Type.Name}].{node.Member.Name}");
                    if (node.Type == typeof(bool))
                    {
                        boolean = true;
                        if (!has_right)
                        {
                            sb.Append(invert ? " = 0" : " = 1");
                        }
                    }
                }
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
            {
                sb.Append("(");
                this.Visit(node.Object);
                sb.Append(invert ? "NOT LIKE '%" : " LIKE '%");
                quote = false;
                this.Visit(node.Arguments[0]);
                quote = true;
                sb.Append("%')");
                return node;
            }
            if (node.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
            {
                sb.Append("(");
                this.Visit(node.Object);
                sb.Append(invert ? "NOT LIKE '" : " LIKE '");
                quote = false;
                this.Visit(node.Arguments[0]);
                quote = true;
                sb.Append("%')");
                return node;
            }
            if (node.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
            {
                sb.Append("(");
                this.Visit(node.Object);
                sb.Append(invert ? "NOT LIKE '%" : " LIKE '%");
                quote = false;
                this.Visit(node.Arguments[0]);
                quote = true;
                sb.Append("')");
                return node;
            }

            // 注意区分两种contains方法的方式，一个是在对象上list.contains，一个是在string上string.contains
            if (node.Method.Name == "Contains")
            {
                Expression collection;
                Expression property;
                if (node.Method.IsDefined(typeof(ExtensionAttribute)) && node.Arguments.Count == 2) // 支持直接调用扩展方法的形式
                {
                    collection = node.Arguments[0];
                    property = node.Arguments[1];
                }
                else if (!node.Method.IsDefined(typeof(ExtensionAttribute)) && node.Arguments.Count == 1)
                {
                    collection = node.Object;
                    property = node.Arguments[0];
                }
                else
                {
                    throw new Exception("Unsupported method call: " + node.Method.Name);
                }
                sb.Append("(");
                this.Visit(property);
                sb.Append(invert ? " NOT IN (" : " IN (");
                this.Visit(collection);
                sb.Append("))");
            }
            else
            {
                throw new Exception("Unsupported method call: " + node.Method.Name);
            }

            return node;
        }

        private bool IsNullConstant(Expression expr)
        {
            return (expr.NodeType == ExpressionType.Constant && ((ConstantExpression)expr).Value == null);
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }

    public abstract class BaseTableHelper
    {
        public static string ConnectionString { get; }

        private static SqlConnection _connection;

        protected static SqlConnection connection => _connection ?? (_connection = GetOpenConnection());

        static BaseTableHelper()
        {
            // 添加json配置文件路径
#if LOCAL
            var builder = new ConfigurationBuilder().SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.Local.json");
#elif DEBUG
            var builder = new ConfigurationBuilder().SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.Development.json");
#else
            var builder = new ConfigurationBuilder().SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json");
#endif
            // 创建配置根对象
            var configurationRoot = builder.Build();
            ConnectionString = configurationRoot.GetSection("DbConnect:ConnectString_SilverPay").Value;
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

        protected static PageDataView<T> Paged<T>(
            string tableName,
            string where,
            string orderBy,
            string columns,
            int pageSize,
            int currentPage)
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

        protected static PageDataView<T> JoinPaged<T>(
            int type, /* 1 left 2 inner 3 right */
            string tableName1,
            string tableName2,
            string on,
            string where,
            string orderBy,
            string columns,
            int pageSize,
            int currentPage)
        {
            var result = new PageDataView<T>();
            var join = type == 1 ? "LEFT JOIN" : (type == 2 ? " INNER JOIN" : "RIGHT JOIN");
            var count_sql = string.Format("SELECT COUNT(1) FROM {0} {1} {2} ON {3} {4}",
                tableName1,
                join,
                tableName2,
                on,
                string.IsNullOrEmpty(where) ? string.Empty : "WHERE " + where);

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "id desc";
            }

            var sql = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS Row, {0} FROM {2} {3} {4} ON {5} {6}) AS Paged ",
                columns,
                orderBy,
                tableName1,
                join,
                tableName2,
                on,
                string.IsNullOrEmpty(where) ? string.Empty : "WHERE " + where);
            var pageStart = (currentPage - 1) * pageSize;
            sql += string.Format(" WHERE Row >{0} AND Row <={1}", pageStart, pageStart + pageSize);
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
