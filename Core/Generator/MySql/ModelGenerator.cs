using Generator.Core.Config;
using Generator.Template;

namespace Generator.Core.MySql
{
    public class ModelGenerator : BaseGenerator_Model
    {
        private string _table_name;
        public override string FileName { get { return this._table_name; } }

        public ModelGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string RenderModelFor(TableMetaData table)
        {
            return Render("model.cshtml", new ViewInfoWapper { Config = _config, TableInfo = table });
        }
    }
}
