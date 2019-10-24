# CodeGenerator
sql server数据交互层代码生成器

#### 字段备注生成枚举规则
+ 例如字段备注为：代理类型 [1 个人收款 2 支付宝红包 3 支付宝WAP]
+ 生成如下：
```
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
