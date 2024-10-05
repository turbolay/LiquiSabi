using LiquiSabi.ApplicationCore.Utils.Backend.Models;

namespace LiquiSabi.ApplicationCore.Utils.Interfaces;

public interface IExchangeRateProvider
{
	Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken);
}
