# CodeGenerator
关于数据访问层的代码主要有几种思路：
- **原生**\
  代表有`dapper`。可能有些分类中会把dapper归为orm，但个人实在不是太认可，dapper它太轻量了，设计决策也非常清晰明确就指着`DbConnection`凶猛扩展就完事儿了
- **orm**\
  代表有`ef/ef core`。社区新近的有[freesql](https://github.com/dotnetcore/FreeSql)、[linq2db](https://github.com/linq2db/linq2db)等优秀的rom
- **代码生成**\
  最早比较出名的是一款名叫`动软代码生成`的软件（暴露年龄了）；当然基本上这条线上思路都是非常简单清晰的

`CodeGenerator`采用的即是第三种方式，走的就是生成流；底层套用了dapper，dapper虽然方便但是如果在项目中从头开始撸代码也会被很多重复的代码搞得受不了，所以dapper+代码生成便成了一个不错的选择。

至于为什么不选用第二种方式，原因也很简单倒不是说第二种不好，而是毕竟是orm，八仙过海各显神通，每个框架的作者都有自己的一套实现方式。因人而异，这会造成比较高的学习曲。特别是在使用中遇到问题的时候再去查查原代码，想想那可怕的代码量复杂的抽象模型，你真的要一路f12追上去吗：）

## Feature
指着`经典三层架构`那套来生成的，整个概念几乎洗脑了一两代程序员。因此也算得上是一个普适概念，所以拿来用成本算是最低的了。CodeGenerator支持直接生成：
- **数据model代码**
- **DAL访问层代码**
- **枚举值代码**

BLL层代码不支持，那块还是留给程序员自己搞定好了。

所有生成的代码不求一定满足场景，但求能是一个不错的基本盘（或者叫模板、底板）。你能通过这些生成好的代码 ***相对轻松*** 的修改出符合自己项目需求的工程来。当然，如果项目本身很简单，那么生成好的代码直接用起来也是有可能的：）

另外一个优势是整个库的实现思路。照理说这个不应该算是feature的内容，但也正是因为其简单的实现使得你能非常轻松的驾驭住这个库，hence the feature ：）

库本身的代码量并不大，抽象也不多，对象模型也不复杂。思路甚至就是平铺直叙的你给定一个模板（最终要生成的架子），然后库读取数据库对应的元数据，再根据这些元数据套上模板生成出最终成品代码。

最后是一个小小的例子：
## 字段备注生成枚举
+ 例如字段备注为：代理类型 [1 个人收款 2 支付宝红包 3 支付宝WAP]
+ 生成如下：
```csharp
/// <summary>
/// ChannelAgentInfo_Type_Enum枚举
/// 1 个人收款 2 支付宝红包 3 支付宝WAP
/// </summary>
public static class ChannelAgentInfo_Type_Enum
{
	/// <summary>
	/// 个人收款 1
	/// </summary>
	public static readonly short 个人收款 = 1;
	/// <summary>
	/// 支付宝红包 2
	/// </summary>
	public static readonly short 支付宝红包 = 2;
	/// <summary>
	/// 支付宝WAP 3
	/// </summary>
	public static readonly short 支付宝WAP = 3;
}
```
比如你不满意这样的生成结果，那么对应修改`Template\Enum\`路径下enum生成模板`enum.cshtml`即可，是不是很简单：）
