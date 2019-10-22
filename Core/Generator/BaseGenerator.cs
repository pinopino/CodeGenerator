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

        protected string Render(string template, ViewInfoWapper model)
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

        public abstract string RenderDALFor(TableMetaData table);

        public string RenderBaseTableHelper()
        {
            return string.Empty;
        }

        public string RenderPageDataView()
        {
            return string.Empty;
        }

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

        protected bool IsUpdateExceptColumn(string table, string colunm)
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

        public abstract string RenderEnumFor(TableMetaData table, ColumnMetaData column);
    }
}
