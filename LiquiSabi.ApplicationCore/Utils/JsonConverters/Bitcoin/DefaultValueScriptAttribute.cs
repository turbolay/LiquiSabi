using System.ComponentModel;

namespace LiquiSabi.ApplicationCore.Utils.JsonConverters.Bitcoin;

public class DefaultValueScriptAttribute : DefaultValueAttribute
{
	public DefaultValueScriptAttribute(string json) : base(ScriptJsonConverter.Parse(json))
	{
	}
}
