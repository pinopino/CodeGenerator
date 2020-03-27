using Generator.Core.Config;

namespace Generator.Core
{
    public abstract class BaseModelGenerator : BaseGenerator
    {
        public BaseModelGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public abstract string RenderModelFor(TableMetaData table);
		
		public virtual string AppendModelUsing()
        {
            return string.Empty;
        }
    }
}
