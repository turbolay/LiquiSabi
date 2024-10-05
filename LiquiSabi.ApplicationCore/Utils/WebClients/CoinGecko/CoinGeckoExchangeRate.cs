using Newtonsoft.Json;

namespace LiquiSabi.ApplicationCore.Utils.WebClients.CoinGecko;

public class CoinGeckoExchangeRate
{
	[JsonProperty(PropertyName = "current_price")]
	public decimal Rate { get; set; }
}
