using Generator.Core.Config;
using Generator.Core.Inject;

namespace Plugin
{
    /// <summary>
    /// 用来删除首尾默认添加的插入标记
    /// </summary>
    public class DefaultInjector : BaseInjector, IModelInjector
    {
        public DefaultInjector(GlobalConfiguration config)
            : base(config)
        { }

        public override string Inject(string originContent)
        {
            var o1 = InjectHead(originContent, string.Empty);
            var o2 = InjectTail(o1, string.Empty);

            return o2;
        }
    }
}
