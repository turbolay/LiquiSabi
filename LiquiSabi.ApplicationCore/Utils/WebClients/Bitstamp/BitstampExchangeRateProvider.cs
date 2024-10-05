using LiquiSabi.ApplicationCore.Utils.Backend.Models;
using LiquiSabi.ApplicationCore.Utils.Interfaces;
using LiquiSabi.ApplicationCore.Utils.Tor.Http.Extensions;

namespace LiquiSabi.ApplicationCore.Utils.WebClients.Bitstamp;

public class BitstampExchangeRateProvider : IExchangeRateProvider
{
	public async Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken)
	{
		using var httpClient = new HttpClient
		{
			BaseAddress = new Uri("https://www.bitstamp.net")
		};
		using var response = await httpClient.GetAsync("api/v2/ticker/btcusd", cancellationToken).ConfigureAwait(false);
		using var content = response.Content;
		var rate = await content.ReadAsJsonAsync<BitstampExchangeRate>().ConfigureAwait(false);

		var exchangeRates = new List<ExchangeRate>
			{
				new ExchangeRate { Rate = rate.Rate, Ticker = "USD" }
			};

		return exchangeRates;
	}
}
