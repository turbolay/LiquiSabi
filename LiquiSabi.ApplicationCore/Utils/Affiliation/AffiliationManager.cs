using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using LiquiSabi.ApplicationCore.Utils.Affiliation.Models;
using LiquiSabi.ApplicationCore.Utils.Tor.Http;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds;

namespace LiquiSabi.ApplicationCore.Utils.Affiliation;

public class AffiliationManager : BackgroundService
{
	public AffiliationManager(Arena arena, WabiSabi.Backend.WabiSabiConfig wabiSabiConfig, IHttpClientFactory httpClientFactory)
	{
		Signer = new(wabiSabiConfig.AffiliationMessageSignerKey);
		Clients = wabiSabiConfig.AffiliateServers.ToDictionary(
			 x => x.Key,
			  x =>
			  {
				  HttpClient httpClient = httpClientFactory.CreateClient(AffiliationConstants.LogicalHttpClientName);
				  ClearnetHttpClient client = new(httpClient, baseUriGetter: () => new Uri(x.Value));
				  return new AffiliateServerHttpApiClient(client);
			  }).ToImmutableDictionary();
		AffiliateServerStatusUpdater = new(Clients);
		AffiliateDataCollector = new AffiliateDataCollector(arena);
		AffiliateDataUpdater = new(AffiliateDataCollector, Clients, Signer);
	}

	private AffiliationMessageSigner Signer { get; }
	private AffiliateDataCollector AffiliateDataCollector { get; }
	private ImmutableDictionary<string, AffiliateServerHttpApiClient> Clients { get; }
	private AffiliateServerStatusUpdater AffiliateServerStatusUpdater { get; }
	private AffiliateDataUpdater AffiliateDataUpdater { get; }

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await AffiliateServerStatusUpdater.StartAsync(stoppingToken).ConfigureAwait(false);
		await AffiliateDataUpdater.StartAsync(stoppingToken).ConfigureAwait(false);
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		await AffiliateDataUpdater.StopAsync(cancellationToken).ConfigureAwait(false);
		await AffiliateServerStatusUpdater.StopAsync(cancellationToken).ConfigureAwait(false);
	}

	public override void Dispose()
	{
		AffiliateDataCollector.Dispose();
		AffiliateDataUpdater.Dispose();
		Signer.Dispose();
		base.Dispose();
	}

	public AffiliateInformation GetAffiliateInformation()
	{
		return new AffiliateInformation(AffiliateServerStatusUpdater.GetRunningAffiliateServers(), AffiliateDataUpdater.GetAffiliateData());
	}
}
