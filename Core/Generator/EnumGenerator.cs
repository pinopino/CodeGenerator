using Generator.Core.Config;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Generator.Core
{
    public class EnumGenerator : BaseGenerator_Enum
    {
        private string _enum_name;
        private Regex _regex;
        // 同一张表不同字段甚至不同的表可能生成的enum从名字到内容都是一样的，
        // 所以这里提供一个简单的排除方案
        protected readonly List<string> _exist_enum = new List<string>();
        public override string FileName { get { return this._enum_name; } }

        public EnumGenerator(GlobalConfiguration config)
            : base(config)
        {
            _regex = new Regex(@"(?<=\[)[^\]]+(?=\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public override string RenderEnumFor(TableMetaData table, ColumnMetaData column)
        {
            var match = _regex.Match(column.Comment);
            var comment = match.Value.Replace("：", " ").Replace("、", " ").Replace("。", " ").Replace("；", " ").Replace(".", " ").Replace(";", " ").Replace(":", " ");
            var temp = Regex.Replace(table.Name, @"\d", string.Empty).Replace("_", string.Empty);
            _enum_name = $"{temp}_{column.Name}_Enum";

            var enum_info = new EnumInfo
            {
                EnumName = _enum_name,
                Comment = comment,
                Values = comment.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                DbType = column.DbType
            };
            var model = new ViewInfoWapper(this);
            model.Config = _config;
            model.TableInfo = table;
            model.EnumInfo = enum_info;

            return Render("Enum/enum.cshtml", model);
        }

        public override bool CanGenerateEnum(TableMetaData table, ColumnMetaData column)
        {
            if (string.IsNullOrWhiteSpace(column.Comment))
                return false;
            return _regex.Match(column.Comment).Success;
        }
    }
}
