using Generator.Common;
using Generator.Core.Config;
using Generator.Template;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generator.Core
{
    public abstract class BaseDALGenerator : BaseGenerator
    {
        private List<string> _keywords = new List<string> { "Type" };

        public abstract string EscapeLeft { get; }
        public abstract string EscapeRight { get; }

        public BaseDALGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public virtual string RenderDALFor(TableMetaData table)
        {
            var model = new ViewInfoWapper(this);
            model.Config = _config;
            model.TableInfo = table;

            return Render("DAL/dal_master.cshtml", model);
        }

        public string RenderBaseTableHelper()
        {
            var model = new ViewInfoWapper(this);
            model.Config = _config;

            return Render("DAL/BaseTable/basetablehelper.cshtml", model);
        }

        public virtual string GetPartialViewPath(string method)
        {
            switch (method.ToLower())
            {
                case "exists":
                    return "DAL/Exists/exist.cshtml";
                case "insert":
                    return "DAL/Insert/insert.cshtml";
                case "delete":
                    return "DAL/Delete/delete.cshtml";
                case "update":
                    return "DAL/Update/update.cshtml";
                case "get":
                    return "DAL/Get/get.cshtml";
                case "page":
                    return "DAL/Page/page.cshtml";
            }

            throw new System.ArgumentException($"暂不支持生成{method}相关方法");
        }

        /// <summary>
        /// 生成的dapper查询时使用的表名
        /// </summary>
        public abstract string NormalizeTableName(string tableName);

        /// <summary>
        /// 生成的dapper查询时使用的列名
        /// </summary>
        public abstract string NormalizeFieldName(string fieldName);

        public abstract string MakeConnection();

        public abstract string MakeSqlUsing();

        /// <summary>
        /// 生成的方法的参数列表
        /// </summary>
        public virtual string MakeMethodParam(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"{item.DbType} {item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        /// <summary>
        /// 生成的方法注释信息中包含的参数说明文字
        /// </summary>
        public virtual string MakeMethodParamComment(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"/// <param name=\"{item.Name}\">{item.Comment}</param>");
            return sb.ToString();
        }
    }
}
