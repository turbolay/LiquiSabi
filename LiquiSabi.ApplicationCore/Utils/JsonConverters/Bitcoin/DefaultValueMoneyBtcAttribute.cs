using System.ComponentModel;

namespace LiquiSabi.ApplicationCore.Utils.JsonConverters.Bitcoin;

public class DefaultValueMoneyBtcAttribute : DefaultValueAttribute
{
	public DefaultValueMoneyBtcAttribute(string json) : base(MoneyBtcJsonConverter.Parse(json))
	{
	}
}
