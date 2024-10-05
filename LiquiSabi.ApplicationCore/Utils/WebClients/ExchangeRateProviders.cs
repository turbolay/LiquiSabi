using LiquiSabi.ApplicationCore.Utils.Backend.Models;
using LiquiSabi.ApplicationCore.Utils.Interfaces;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.WebClients.Bitstamp;
using LiquiSabi.ApplicationCore.Utils.WebClients.BlockchainInfo;
using LiquiSabi.ApplicationCore.Utils.WebClients.Coinbase;
using LiquiSabi.ApplicationCore.Utils.WebClients.CoinGecko;
using LiquiSabi.ApplicationCore.Utils.WebClients.Gemini;

namespace LiquiSabi.ApplicationCore.Utils.WebClients;

public class ExchangeRateProvider : IExchangeRateProvider
{
	private readonly IExchangeRateProvider[] _exchangeRateProviders =
	{
		new BlockchainInfoExchangeRateProvider(),
		new BitstampExchangeRateProvider(),
		new CoinGeckoExchangeRateProvider(),
		new CoinbaseExchangeRateProvider(),
		new GeminiExchangeRateProvider()
	};

	public async Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken)
	{
		foreach (var provider in _exchangeRateProviders)
		{
			try
			{
				return await provider.GetExchangeRateAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				// Ignore it and try with the next one
				Logger.LogTrace(ex);
			}
		}
		return Enumerable.Empty<ExchangeRate>();
	}
}
