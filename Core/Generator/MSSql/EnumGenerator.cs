using Generator.Core.Config;
using Generator.Template;
using System.Text.RegularExpressions;

namespace Generator.Core.MSSql
{
    public class EnumGenerator : BaseGenerator_Enum
    {
        private string _enum_name;
        public override string FileName { get { return this._enum_name; } }

        public EnumGenerator(GlobalConfiguration config)
            : base(config)
        { }

        public override string RenderEnumFor(TableMetaData table, ColumnMetaData column)
        {
            var model = new ViewInfoWapper(this);
            model.Config = _config;
            model.TableInfo = table;

            return Render("enum_mssql.cshtml", model);
        }

        private string GetStrFromComment(ColumnMetaData column)
        {
            var enumStr = string.Empty;
            var _regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase);
            var match = _regex.Match(column.Comment);
            if (match.Success)
            {
                enumStr = match.Value.Replace("：", " ").Replace("、", " ").Replace("。", " ").Replace("；", " ").Replace(".", " ").Replace(";", " ").Replace(":", " ");
            }

            return enumStr;
        }
    }
}
