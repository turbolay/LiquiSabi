using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using LiquiSabi.ApplicationCore.Publishing.Nostr;
using LiquiSabi.ApplicationCore.Utils.Bases;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.Tor.Http;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Client;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Data;

public class Scraper(CoordinatorDiscovery coordinatorDiscovery)
    : PeriodicRunner(Interval)
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    public readonly Channel<PublicStatus> ToBeProcessedData = Channel.CreateUnbounded<PublicStatus>();

    private CoordinatorDiscovery CoordinatorDiscovery { get; } = coordinatorDiscovery;

    private Dictionary<string, WabiSabiHttpApiClient> ApiClientPerCoordinator { get; } = new();

    protected override async Task ActionAsync(CancellationToken token)
    {
        List<ApiClientWithCoordinator> toUseApiClients = [];
        foreach (var coordinator in CoordinatorDiscovery.GetCoordinators())
        {
            if (!ApiClientPerCoordinator.TryGetValue(coordinator.Endpoint, out var apiClient))
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(coordinator.Endpoint);
                var clearnetHttpClient = new ClearnetHttpClient(httpClient);
                apiClient = new WabiSabiHttpApiClient(clearnetHttpClient);
                ApiClientPerCoordinator.Add(coordinator.Endpoint, apiClient);
            }
            
            toUseApiClients.Add(new ApiClientWithCoordinator(coordinator, apiClient));
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);
        
        List<Task> tasks = [];
        foreach (var apiClientAndCoordinator in toUseApiClients)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    // ReSharper disable AccessToDisposedClosure
                    var status =
                        await apiClientAndCoordinator.ApiClient.GetStatusAsync(RoundStateRequest.Empty,
                            linkedCts.Token);
                    var humanMonitor =
                        await apiClientAndCoordinator.ApiClient.GetHumanMonitor(new HumanMonitorRequest(),
                            linkedCts.Token);
                    // ReSharper restore AccessToDisposedClosure
                    var publicStatus = new PublicStatus(apiClientAndCoordinator.Coordinator, DateTimeOffset.UtcNow,
                        status, humanMonitor);

                    await ToBeProcessedData.Writer.WriteAsync(publicStatus, token);
                }
                catch (Exception)
                {
                    Logger.LogWarning($"Coordinator: {apiClientAndCoordinator.Coordinator.Endpoint}");
                }
            }, linkedCts.Token);
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);
    }

    private record ApiClientWithCoordinator(
        CoordinatorDiscovery.Coordinator Coordinator,
        WabiSabiHttpApiClient ApiClient);
}