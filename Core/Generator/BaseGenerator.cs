using Generator.Core.Config;
using Generator.Template;
using RazorLight;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Core
{
    public abstract class BaseGenerator
    {
        public abstract string FileName { get; }
        protected readonly GlobalConfiguration _config;
        protected readonly RazorLightEngine _engine;

        public BaseGenerator(GlobalConfiguration config)
        {
            _config = config;
            _engine = new RazorLightEngineBuilder()
                 .UseFilesystemProject(_config.TemplatePath)
                 .UseMemoryCachingProvider()
                 .Build();
        }

        public string Render(string template, ViewInfoWapper model)
        {
            var result = string.Empty;
            var cacheResult = _engine.TemplateCache.RetrieveTemplate(template);
            if (cacheResult.Success)
                result = _engine.RenderTemplateAsync(cacheResult.Template.TemplatePageFactory(), model).Result;
            else
                result = _engine.CompileRenderAsync(template, model).Result;

            return result;
        }
    }

    public abstract class BaseGenerator_DAL : BaseGenerator
    {
        private List<string> _keywords = new List<string> { "Type" };

        public BaseGenerator_DAL(GlobalConfiguration config)
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

        /// <summary>
        /// 生成的dapper查询时使用的表名
        /// </summary>
        public abstract string MakeTableName(string rawName);

        /// <summary>
        /// 生成的方法注释信息中包含的参数说明文字
        /// </summary>
        public abstract string MakeParamComment(List<ColumnMetaData> columns);

        /// <summary>
        /// 生成的方法的参数列表
        /// </summary>
        public abstract string MakeMethodParam(List<ColumnMetaData> columns);

        /// <summary>
        /// 生成的dapper查询时使用的参数列表
        /// </summary>
        public abstract string MakeParamList(IEnumerable<ColumnMetaData> columns);

        /// <summary>
        /// 生成的dapper查询时使用的参数值列表
        /// </summary>
        public abstract string MakeParamValueList(IEnumerable<ColumnMetaData> columns);

        /// <summary>
        /// 生成的dapper查询时where语句
        /// </summary>
        public abstract string MakeWhere(IEnumerable<ColumnMetaData> columns);

        public string RenderBaseTableHelper()
        {
            var model = new ViewInfoWapper(this);
            model.Config = _config;

            return Render("DAL/Base/BaseTableHelper.cshtml", model);
        }

        public abstract string MakeConnectionInit();

        public abstract string MakeGetOpenConnection();

        public abstract string MakeBasePaging(TableMetaData TableInfo);

        public abstract string MakeBaseParseExpression();

        public abstract string MakeTableColumns();

        protected bool IsKeyword(string colunm)
        {
            return _keywords.Contains(colunm);
        }

        protected string GetReturnStr(string keyType)
        {
            switch (keyType.ToLower())
            {
                case "int":
                    {
                        return "0";
                    }
                case "long":
                    {
                        return "0L";
                    }
                case "string":
                    {
                        return "string.Empty";
                    }
            }

            return string.Empty;
        }

        public bool IsUpdateExceptColumn(string table, string colunm)
        {
            if (_config.UpdateExceptColumns == null)
                return false;

            return _config.UpdateExceptColumns.ContainsKey("*") && _config.UpdateExceptColumns["*"].Any(p => p.ColumnName == colunm) ||
                _config.UpdateExceptColumns.ContainsKey(table) && _config.UpdateExceptColumns[table].Any(p => p.ColumnName == colunm);
        }
    }

    public abstract class BaseGenerator_Model : BaseGenerator
    {
        public BaseGenerator_Model(GlobalConfiguration config)
            : base(config)
        { }

        public abstract string RenderModelFor(TableMetaData table);
    }

    public abstract class BaseGenerator_Enum : BaseGenerator
    {
        public BaseGenerator_Enum(GlobalConfiguration config)
            : base(config)
        { }

        public abstract bool CanGenerateEnum(TableMetaData table, ColumnMetaData column);

        public abstract string RenderEnumFor(TableMetaData table, ColumnMetaData column);
    }
}
