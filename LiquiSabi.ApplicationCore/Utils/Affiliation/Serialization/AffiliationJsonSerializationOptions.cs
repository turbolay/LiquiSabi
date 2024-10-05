using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LiquiSabi.ApplicationCore.Utils.Affiliation.Serialization;

public static class AffiliationJsonSerializationOptions
{
	public static readonly List<JsonConverter> Converters = new()
	{
		new AffiliationByteArrayJsonConverter(),
		new AffiliationCoordinatorFeeRateJsonConverter()
	};

	public static readonly JsonSerializerSettings Settings = new()
	{
		Converters = Converters,
		ContractResolver = new DefaultContractResolver
		{
			NamingStrategy = new SnakeCaseNamingStrategy()
		}
	};
}
