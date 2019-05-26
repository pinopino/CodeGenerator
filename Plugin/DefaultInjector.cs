using Generator.Core;
using Generator.Core.Config;
using Generator.Core.Inject;
using System.Collections.Generic;

namespace Plugin
{
    /// <summary>
    /// 用来删除首尾默认添加的插入标记
    /// </summary>
    internal class DefaultInjector : BaseInjector, IModelInjector
    {
        public DefaultInjector(Dictionary<string, TableMetaData> tables, GlobalConfiguration config)
            : base(tables, config)
        { }

        public override string Inject(string originContent, string tableName = "", string columnName = "")
        {
            var o1 = InjectHead(originContent, string.Empty);
            var o2 = InjectTail(o1, string.Empty);

            return o2;
        }
    }
}
