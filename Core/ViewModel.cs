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

        public string GetPartialViewPath(string method)
        {
            return ((BaseGenerator_DAL)_generator).GetPartialViewPath(method);
        }

        public string MakeTableName(string rawName)
        {
            return ((BaseGenerator_DAL)_generator).MakeTableName(rawName);
        }

        public string MakeParamComment(List<ColumnMetaData> columns)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamComment(columns);
        }

        public string MakeMethodParam(List<ColumnMetaData> columns)
        {
            return ((BaseGenerator_DAL)_generator).MakeMethodParam(columns);
        }

        public string MakeParamList(List<ColumnMetaData> columns, Func<ColumnMetaData, bool> predicate = null)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamList(columns.Where(p => predicate == null ? true : predicate(p)));
        }

        public string MakeParamValList(List<ColumnMetaData> columns, Func<ColumnMetaData, bool> predicate = null)
        {
            return ((BaseGenerator_DAL)_generator).MakeParamValueList(columns.Where(p => predicate == null ? true : predicate(p)));
        }

        public string MakeWhere(List<ColumnMetaData> columns, Func<ColumnMetaData, bool> predicate = null)
        {
            return ((BaseGenerator_DAL)_generator).MakeWhere(columns.Where(p => predicate == null ? true : predicate(p)));
        }

        public string MakeConnectionInit()
        {
            return ((BaseGenerator_DAL)_generator).MakeConnectionInit();
        }

        public string MakeGetOpenConnection()
        {
            return ((BaseGenerator_DAL)_generator).MakeGetOpenConnection();
        }

        public string MakeBasePaging(TableMetaData tableMetaData)
        {
            return ((BaseGenerator_DAL)_generator).MakeBasePaging(tableMetaData);
        }

        public string Render(string template, ViewInfoWapper model)
        {
            return _generator.Render(template, model);
        }

        public bool IsUpdateExceptColumn(ColumnMetaData column)
        {
            return ((BaseGenerator_DAL)_generator).IsUpdateExceptColumn(TableInfo.Name, column.Name);
        }
        public string MakeBaseParseExpression()
        {
            return ((BaseGenerator_DAL)_generator).MakeBaseParseExpression();
        }

        public string AppendDALUsing()
        {
            return ((BaseGenerator_DAL)_generator).AppendDALUsing();
        }

        public string AppendModelUsing()
        {
            return ((BaseGenerator_Model)_generator).AppendModelUsing();
        }

        public string AppendEnumUsing()
        {
            return ((BaseGenerator_Enum)_generator).AppendEnumUsing();
        }
    }
}
