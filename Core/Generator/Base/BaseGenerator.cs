using Generator.Core.Config;
using Generator.Template;
using RazorLight;

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
}
