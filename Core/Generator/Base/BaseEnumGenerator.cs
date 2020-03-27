using Generator.Core.Config;

namespace Generator.Core
{
    public abstract class BaseEnumGenerator : BaseGenerator
    {
        public BaseEnumGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public abstract bool CanGenerateEnum(TableMetaData table, ColumnMetaData column);

        public abstract string RenderEnumFor(TableMetaData table, ColumnMetaData column);
		
		public virtual string AppendEnumUsing()
        {
            return string.Empty;
        }
    }
}
