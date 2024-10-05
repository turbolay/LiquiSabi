using System.ComponentModel;

namespace LiquiSabi.ApplicationCore.Utils.JsonConverters;

public class DefaultValueIntegerArrayAttribute : DefaultValueAttribute
{
	public DefaultValueIntegerArrayAttribute(string json) : base(IntegerArrayJsonConverter.Parse(json))
	{
	}
}
