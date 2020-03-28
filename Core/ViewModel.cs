using Generator.Common;
using Generator.Core;
using Generator.Core.Config;
using System.Collections.Generic;
using System.Text;

namespace Generator.Template
{
    public class EnumInfo
    {
        public string EnumName;
        public string Comment;
        public string[] Values;
        public string DbType;
    }

    public class ViewInfoWapper
    {
        private BaseGenerator _generator;
        public GlobalConfiguration Config;
        public TableMetaData TableInfo;
        public EnumInfo EnumInfo;

        public ViewInfoWapper(BaseGenerator generator)
        {
            _generator = generator;
        }

        public string Render(string template, ViewInfoWapper model)
        {
            return _generator.Render(template, model);
        }

        public string GetPartialViewPath(string method)
        {
            return ((BaseDALGenerator)_generator).GetPartialViewPath(method);
        }

        public string MakeTableName(string rawName)
        {
            return ((BaseDALGenerator)_generator).NormalizeTableName(rawName);
        }

        public string MakeFieldName(string rawName)
        {
            return ((BaseDALGenerator)_generator).NormalizeFieldName(rawName);
        }

        public string MakeMethodParam(params ColumnMetaData[] columns)
        {
            return ((BaseDALGenerator)_generator).MakeMethodParam(columns);
        }

        public string MakeMethodParam(IEnumerable<ColumnMetaData> columns)
        {
            return ((BaseDALGenerator)_generator).MakeMethodParam(columns);
        }

        public string MakeMethodParamComment(params ColumnMetaData[] columns)
        {
            return ((BaseDALGenerator)_generator).MakeMethodParamComment(columns);
        }

        public string MakeMethodParamComment(IEnumerable<ColumnMetaData> columns)
        {
            return ((BaseDALGenerator)_generator).MakeMethodParamComment(columns);
        }

        public string MakeSQLParamList(params ColumnMetaData[] columns)
        {
            return MakeSQLParamList((IEnumerable<ColumnMetaData>)columns);
        }

        public string MakeSQLParamList(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
            {
                if (!item.IsIdentity)
                    sb.Append($"@{item.Name}, ");
            }
            return sb.TrimEnd(", ").ToString();
        }

        public string MakeSQLParamValueList(params ColumnMetaData[] columns)
        {
            return MakeSQLParamValueList((IEnumerable<ColumnMetaData>)columns);
        }

        public string MakeSQLParamValueList(IEnumerable<ColumnMetaData> columns)
        {
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"@{item.Name}={item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        public string MakeSQLColumnList(params ColumnMetaData[] columns)
        {
            return MakeSQLColumnList((IEnumerable<ColumnMetaData>)columns);
        }

        public string MakeSQLColumnList(IEnumerable<ColumnMetaData> columns)
        {
            var gen = ((BaseDALGenerator)_generator);
            var sb = new StringBuilder();
            foreach (var item in columns)
            {
                if (!item.IsIdentity)
                    sb.Append(gen.NormalizeFieldName(item.Name));
            }
            return sb.TrimEnd(", ").ToString();
        }

        public string MakeSQLWhere(params ColumnMetaData[] columns)
        {
            return MakeSQLWhere((IEnumerable<ColumnMetaData>)columns);
        }

        public string MakeSQLWhere(IEnumerable<ColumnMetaData> columns)
        {
            var gen = ((BaseDALGenerator)_generator);
            var sb = new StringBuilder();
            foreach (var item in columns)
                sb.Append($"{gen.NormalizeFieldName(item.Name)}={item.Name}, ");
            return sb.TrimEnd(", ").ToString();
        }

        public bool IsUpdateExcludeColumn(ColumnMetaData column)
        {
            return ((BaseDALGenerator)_generator).IsUpdateExcludeColumn(TableInfo.Name, column.Name);
        }
    }
}
