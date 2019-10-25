using Generator.Core.Config;
using Generator.Template;

namespace Generator.Core
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
            _table_name = table.Name;
            var model = new ViewInfoWapper(this);
            model.Config = _config;
            model.TableInfo = table;

            return Render("Model/model.cshtml", model);
        }
    }
}
