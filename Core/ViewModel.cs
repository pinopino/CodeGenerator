using Generator.Core;
using Generator.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Template
{
    public class KeyWordsEscape
    {
        public string Left;
        public string Right;
    }
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
        public KeyWordsEscape KeyWordsEscape;

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
            return ((BaseDALGenerator)_generator).MakeTableName(rawName);
        }

        public string MakeMethodParam(IEnumerable<ColumnMetaData> columns)
        {
            return ((BaseDALGenerator)_generator).MakeMethodParam(columns);
        }

        public string MakeMethodParamComment(IEnumerable<ColumnMetaData> columns)
        {
            return ((BaseDALGenerator)_generator).MakeMethodParamComment(columns);
        }

        public string MakeSQLParamList(IEnumerable<ColumnMetaData> columns)
        {
            return ((BaseDALGenerator)_generator).MakeSQLParamList(columns);
        }

        public string MakeSQLParamValueList(IEnumerable<ColumnMetaData> columns)
        {
            return ((BaseDALGenerator)_generator).MakeSQLParamValueList(columns);
        }

        public string MakeSQLWhere(IEnumerable<ColumnMetaData> columns)
        {
            return ((BaseDALGenerator)_generator).MakeSQLWhere(columns);
        }

        public bool IsUpdateExceptColumn(ColumnMetaData column)
        {
            return ((BaseDALGenerator)_generator).IsUpdateExcludeColumn(TableInfo.Name, column.Name);
        }
		
        public string AppendDALUsing()
        {
            return ((BaseDALGenerator)_generator).AppendDALUsing();
        }

        public string AppendModelUsing()
        {
            return ((BaseModelGenerator)_generator).AppendModelUsing();
        }

        public string AppendEnumUsing()
        {
            return ((BaseEnumGenerator)_generator).AppendEnumUsing();
        }
    }
}
