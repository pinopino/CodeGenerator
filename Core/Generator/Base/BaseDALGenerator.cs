using Generator.Core.Config;
using Generator.Template;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Core
{
    public abstract class BaseDALGenerator : BaseGenerator
    {
        private List<string> _keywords = new List<string> { "Type" };

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

        public virtual string GetPartialViewPath(string method)
        {
            switch (method.ToLower())
            {
                case "exists":
                    return "DAL/Exists/exist.cshtml";
                case "insert":
                    return "DAL/Insert/inser.cshtml";
                case "delete":
                    return "DAL/Delete/delete.cshtml";
                case "update":
                    return "DAL/Update/update.cshtml";
                case "getmodel":
                case "getlist":
                case "getcount":
                    return "DAL/GetModel/get.cshtml";
                case "getpage":
                    return "DAL/Page/page.cshtml";
            }

            throw new System.ArgumentException($"暂不支持生成{method}相关方法");
        }
		
		public virtual string AppendDALUsing()
        {
            return string.Empty;
        }

        public abstract string RenderBaseTableHelper();

        /// <summary>
        /// 生成的dapper查询时使用的表名
        /// </summary>
        public abstract string MakeTableName(string rawName);

        /// <summary>
        /// 生成的方法的参数列表
        /// </summary>
        public abstract string MakeMethodParam(IEnumerable<ColumnMetaData> columns);

        /// <summary>
        /// 生成的方法注释信息中包含的参数说明文字
        /// </summary>
        public abstract string MakeMethodParamComment(IEnumerable<ColumnMetaData> columns);

        /// <summary>
        /// 生成的dapper查询时使用的参数列表
        /// </summary>
        public abstract string MakeSQLParamList(IEnumerable<ColumnMetaData> columns);

        /// <summary>
        /// 生成的dapper查询时使用的参数值列表
        /// </summary>
        public abstract string MakeSQLParamValueList(IEnumerable<ColumnMetaData> columns);

        /// <summary>
        /// 生成的dapper查询时where语句
        /// </summary>
        public abstract string MakeSQLWhere(IEnumerable<ColumnMetaData> columns);

        public bool IsUpdateExcludeColumn(string table, string colunm)
        {
            if (_config.UpdateExceptColumns == null)
                return false;

            return _config.UpdateExceptColumns.ContainsKey("*") && _config.UpdateExceptColumns["*"].Any(p => p.ColumnName == colunm) ||
                _config.UpdateExceptColumns.ContainsKey(table) && _config.UpdateExceptColumns[table].Any(p => p.ColumnName == colunm);
        }
    }
}
