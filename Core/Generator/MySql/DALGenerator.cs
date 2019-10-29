using Generator.Common;
using Generator.Core.Config;
using Generator.Template;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generator.Core.MySql
{
    public class DALGenerator : BaseGenerator_DAL
    {
        public override string FileName => throw new NotImplementedException();

        public DALGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string GetPartialViewPath(string method)
        {
            switch (method.ToLower())
            {
                case "insert":
                    return "DAL/Insert/insert_mysql.cshtml";
                case "update":
                    return "DAL/Update/update_mysql.cshtml";
                case "getmodel":
                case "getlist":
                case "getcount":
                    return "DAL/GetModel/get_mysql.cshtml";
                case "getpage":
                    return "DAL/Page/page_mysql.cshtml";
                default:
                    return base.GetPartialViewPath(method);
            }
        }

        public override string MakeTableName(string rawName)
        {
            return $"`{rawName}`";
        }

        public override string MakeParamComment(List<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"/// <param name=\"{item.Name}\">{item.Comment}</param>");
            return sb.ToString();
        }

        public override string MakeMethodParam(List<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"{item.DbType} {item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeParamList(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
            {
                if (!item.IsIdentity)
                    sb.Append($"@{item.Name}, ");
            }
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeParamValueList(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"@{item.Name}={item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        public override string MakeWhere(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"[{item.Name}]=@{item.Name} and ");
            return sb.TrimEnd("and ").ToString();
        }

        public override string MakeConnectionInit()
        {
            var str = @"var builder = new ConfigurationBuilder().SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory).AddJsonFile(""appsettings.json"");
            // 创建配置根对象
            var configurationRoot = builder.Build();
            _connectionstring = configurationRoot.GetSection(""DbConnect"").Value; ";

            return str;
        }

        public override string MakeGetOpenConnection()
        {
            var str = @"var connection = new MySqlConnection(_connectionstring);
            connection.Open();
            return connection;";

            return str;
        }

        public override string MakeBasePaging(TableMetaData tableMetaData)
        {
            return Render("DAL/Base/BasePaging_mysql.cshtml", new ViewInfoWapper(this) { TableInfo = tableMetaData });
        }

        public override string MakeBaseParseExpression()
        {
            return Render("DAL/Base/BaseParseExpression.cshtml", new ViewInfoWapper(this) {KeyWordsEscape= new Template.KeyWordsEscape { Left = "`", Right = "`" } });
        }

        public override string MakeTableColumns()
        {
            throw new NotImplementedException();
        }
    }
}
