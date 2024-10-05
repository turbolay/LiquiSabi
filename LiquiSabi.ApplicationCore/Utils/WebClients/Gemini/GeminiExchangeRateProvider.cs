using LiquiSabi.ApplicationCore.Utils.Backend.Models;
using LiquiSabi.ApplicationCore.Utils.Interfaces;
using LiquiSabi.ApplicationCore.Utils.Tor.Http.Extensions;

namespace LiquiSabi.ApplicationCore.Utils.WebClients.Gemini;

public class GeminiExchangeRateProvider : IExchangeRateProvider
{
	public async Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken)
	{
		using var httpClient = new HttpClient
		{
			BaseAddress = new Uri("https://api.gemini.com")
		};
		using var response = await httpClient.GetAsync("/v1/pubticker/btcusd", cancellationToken).ConfigureAwait(false);
		using var content = response.Content;
		var data = await content.ReadAsJsonAsync<GeminiExchangeRateInfo>().ConfigureAwait(false);

		var exchangeRates = new List<ExchangeRate>
		{
			new ExchangeRate { Rate = data.Bid, Ticker = "USD" }
		};

		return exchangeRates;
	}

	private class GeminiExchangeRateInfo
	{
		public decimal Bid { get; set; }
	}
}
