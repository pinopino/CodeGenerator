using Dapper;
using Generator.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Console
{
    class ExUser
    {
        public int Id;
        public string Name;
    }

    class PredicateVisitor : ExpressionVisitor
    {
        private StringBuilder sb = new StringBuilder();
        private bool invert = false;
        private bool quote = true;

        public void Test()
        {
            var list = new List<string> { "1", "2", "3" };
            Expression<Func<ExUser, object>> exp1 = p => !(!(p.Id > 1 && p.Id < 20) || p.Name == "abc");
            Visit(exp1);
            System.Console.WriteLine(this);
            sb.Clear();
            Expression<Func<ExUser, object>> exp2 = p => !(p.Id == 1 || p.Id == 2 || p.Id == 3);
            Visit(exp2);
            System.Console.WriteLine(this);
            sb.Clear();
            Expression<Func<ExUser, object>> exp3 = p => p.Name.Contains("a");
            Visit(exp3);
            System.Console.WriteLine(this);
            sb.Clear();
            Expression<Func<ExUser, object>> exp4 = p => list.Contains(p.Name);
            Visit(exp4);
            System.Console.WriteLine(this);
            sb.Clear();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            sb.Append("(");
            this.Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(node.Right))
                    {
                        sb.Append(" IS ");
                    }
                    else
                    {
                        sb.Append(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(node.Right))
                    {
                        sb.Append(" IS NOT ");
                    }
                    else
                    {
                        sb.Append(" <> ");
                    }
                    break;

                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", node.NodeType));
            }

            this.Visit(node.Right);
            sb.Append(")");
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append("NOT");
                    this.Visit(node.Operand);
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
                        sb.Append(((bool)node.Value) ? 1 : 0);
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
            // 闭包带进来的变量是生成类型的一个Field
            if (node.Expression.NodeType == ExpressionType.Constant)
            {
                var container = ((ConstantExpression)node.Expression).Value;
                var value = ((FieldInfo)node.Member).GetValue(container);
                var coll = value as IList;
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

            if (node.Expression.NodeType == ExpressionType.Parameter)
                sb.Append(node.Member.Name);

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
            {
                sb.Append("(");
                this.Visit(node.Object);
                sb.Append($" LIKE '%");
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
                sb.Append($" LIKE '");
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
                sb.Append($" LIKE '%");
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
                sb.Append(" IN (");
                this.Visit(collection);
                sb.Append("))");
            }
            else
            {
                throw new Exception("Unsupported method call: " + node.Method.Name);
            }

            return node;
        }

        private bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var s = new PredicateVisitor();
            s.Test();
            System.Console.Read();

            return;
            var conn_str = ConfigurationManager.AppSettings["DBConn"];
            if (string.IsNullOrWhiteSpace(conn_str))
            {
                System.Console.WriteLine("未设置数据库连接字符串！");
                System.Console.Read();
                Environment.Exit(0);
            }

            ReCreateDB(conn_str, Encoding.GetEncoding("gb2312"));

            var dbcode = FindDBName(conn_str);
            Print("解析数据库元数据...");
            using (SQLServerManagement manage = new SQLServerManagement(conn_str))
            {
                var config = new SQLMetaData();
                SQLMetaDataHelper.InitConfig(config);
                var data = manage.Databases[dbcode].Tables;
                // 解析数据库元数据
                var parser = new MetaDataParser(config);
                parser.Parse(data);
                Print("解析完毕，生成中间配置文件...");

                // 生成中间配置文件
                var config_json_str = JsonConvert.SerializeObject(config);
                SQLMetaDataHelper.OutputConfig(config_json_str);

                // 生成最终文件
                Print("按 'y/Y' 继续生成最终操作类文件...");
                var key = string.Empty;
                do
                {
                    key = System.Console.ReadLine();
                    if (key == "Y" || key == "y")
                    {
                        // 生成DAL最终文件
                        Print("生成DAL...");
                        SQLMetaDataHelper.OutputDAL(config);

                        // 生成Model最终文件
                        Print("生成Model...");
                        SQLMetaDataHelper.OutputModel(config);

                        // 生成Enum最终文件
                        Print("生成Enum...");
                        SQLMetaDataHelper.OutputEnum(config);

                        // 检测partial字段有效性
                        Print("检测partial字段有效性...");
                        SQLMetaDataHelper.DoPartialCheck(config);

                        Print("生成完毕！");
                        break;
                    }
                    System.Console.WriteLine("按‘quit’退出");
                } while (key != "quit");
            }

            Print("结束！");
            System.Console.Read();
            Environment.Exit(0);
        }

        // link: https://stackoverflow.com/questions/18596876/go-statements-blowing-up-sql-execution-in-net
        static void ReCreateDB(string connStr, Encoding encoding)
        {
            var config_path = ConfigurationManager.AppSettings["ReCreateDB_SQLFile"];
            var db_names = ConfigurationManager.AppSettings["ReCreateDB_Names"];
            if (string.IsNullOrWhiteSpace(config_path) || string.IsNullOrWhiteSpace(db_names))
            {
                return;
            }

            var db = FindDBName(connStr);
            if (string.IsNullOrWhiteSpace(db))
            {
                return;
            }

            var files = Directory.GetFiles(config_path, "*.sql", SearchOption.TopDirectoryOnly);
            var arrs = db_names.Replace('；', ';').Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arrs)
            {
                System.Console.WriteLine("尝试重新生成数据库[" + item + "]...");
                System.Console.WriteLine("检测是否存在该数据库");
                if (IsExist(connStr, item))
                {
                    connStr = connStr.Replace(db, item);
                    using (var conn = new SqlConnection(connStr))
                    {
                        var svr = new Server(new ServerConnection(conn));
                        foreach (var file_path in files)
                        {
                            var script = File.ReadAllText(file_path, Encoding.GetEncoding("gb2312"));
                            svr.ConnectionContext.ExecuteNonQuery(script);
                        }
                    }
                    System.Console.WriteLine("存在，重新生成数据库[" + item + "]成功");
                }
                else
                {
                    System.Console.WriteLine("不存在该数据库，结束");
                }
                System.Console.WriteLine();
            }
        }

        static string FindDBName(string connStr)
        {
            var db_name = string.Empty;
            var cb = new DbConnectionStringBuilder(false);
            cb.ConnectionString = connStr;
            object database;
            if (cb.TryGetValue("Initial Catalog", out database))
            {
                db_name = database.ToString();
            }
            else
            {
                if (cb.TryGetValue("Database", out database))
                {
                    db_name = database.ToString();
                }
            }

            return db_name;
        }

        static bool IsExist(string connStr, string db)
        {
            using (var conn = GetOpenConnection(connStr))
            {
                return conn.ExecuteScalar<int>("select count(*) From master.dbo.sysdatabases where name='" + db + "'") > 0;
            }
        }

        static SqlConnection GetOpenConnection(string connStr, bool mars = false)
        {
            var cs = connStr;
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

        static void Print(string message)
        {
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine(message);
        }
    }
}
