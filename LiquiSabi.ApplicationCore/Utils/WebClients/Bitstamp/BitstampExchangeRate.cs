using Newtonsoft.Json;

namespace LiquiSabi.ApplicationCore.Utils.WebClients.Bitstamp;

public class BitstampExchangeRate
{
	[JsonProperty(PropertyName = "bid")]
	public decimal Rate { get; set; }
}
