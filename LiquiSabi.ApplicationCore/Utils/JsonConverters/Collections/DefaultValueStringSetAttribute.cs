using System.ComponentModel;
using Newtonsoft.Json;

namespace LiquiSabi.ApplicationCore.Utils.JsonConverters.Collections;

public class DefaultValueStringSetAttribute : DefaultValueAttribute
{
	public DefaultValueStringSetAttribute(string json)
		: base(JsonConvert.DeserializeObject<ISet<string>>(json))
	{
	}
}
