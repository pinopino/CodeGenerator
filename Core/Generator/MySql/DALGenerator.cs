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

        public override string RenderDALFor(TableMetaData table)
        {
            return Render("dal_master.cshtml", new ViewInfoWapper { Config = _config, TableInfo = table });
        }
    }
}
