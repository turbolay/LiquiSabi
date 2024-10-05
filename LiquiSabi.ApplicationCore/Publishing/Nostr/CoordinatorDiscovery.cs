using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;
using NNostr.Client;

namespace LiquiSabi.ApplicationCore.Publishing.Nostr;

public class CoordinatorDiscovery : BackgroundService
{
    private const string NetworkTypeDetected = "Mainnet";

    private List<string> Relays { get; } =
    [
        "wss://relay.primal.net",
        "wss://relay.damus.io",
        "wss://relay.nostr.band",
        "wss://nostr.wine",
        "wss://nostr-pub.wellorder.net"
    ];

    private const int Kind = 15750;
    private const string TypeTagIdentifier = "type";
    private const string TypeTagValue = "wabisabi";
    private const string NetworkTagIdentifier = "network";
    private const string EndpointTagIdentifier = "endpoint";
    
    private List<Coordinator> Coordinators { get; } = new();
    private object Lock { get; } = new();

    private CompositeNostrClient? Client { get; set; }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Coordinators.Add(new Coordinator("","https://api.opencoordinator.org/", "", DateTimeOffset.UtcNow));
        Client = new CompositeNostrClient(Relays
            .Select(s => Uri.TryCreate(s, UriKind.Absolute, out var uri) ? uri : null)
            .Where(u => u is not null).ToArray()!);

        Client.StateChanged += (sender, args) =>
        {
            if (args.Item2 == WebSocketState.Closed)
            {
                // ReSharper disable once AccessToDisposedClosure
                _ = Client.Connect(cancellationToken);
            }
        };

        await Client.ConnectAndWaitUntilConnected(cancellationToken);

        await foreach (var evt in Client.SubscribeForEvents([
                           new NostrSubscriptionFilter
                           {
                               Kinds = [Kind],
                           }
                       ], false, cancellationToken))
        {

            if (evt.GetTaggedData(TypeTagIdentifier).Length == 0 || !evt.GetTaggedData(TypeTagIdentifier)
                    .Contains(TypeTagValue, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            var network = evt.GetTaggedData(NetworkTagIdentifier);
            var options = new[] { NetworkTypeDetected.ToLower(), $"{NetworkTypeDetected.ToLower()}" };
            if (network.Any(n => options.Contains(n, StringComparer.InvariantCultureIgnoreCase)) is not true)
            {
                continue;
            }

            lock (Lock)
            {
                var endpoint = evt.GetTaggedData(EndpointTagIdentifier).First();
                if (!endpoint.StartsWith("http") || endpoint.Contains("localhost:") ||
                    evt.CreatedAt < DateTimeOffset.UtcNow - TimeSpan.FromHours(2))
                {
                    continue;
                }

                if (endpoint.Contains("kruw") && !endpoint.Contains("coinjoin"))
                {
                    continue;
                }
                
                if (endpoint.Contains("wasabicoordinator")) 
                {
                    continue;
                }
                
                var coordinator = Coordinators.FirstOrDefault(x => x.Endpoint == endpoint);
                if (coordinator is not null && evt.CreatedAt < coordinator.LastUpdate)
                {
                    Coordinators.Remove(coordinator);
                    coordinator = null;
                }

                if (coordinator is null)
                {
                    Coordinators.Add(new Coordinator(
                        evt.PublicKey,
                        endpoint,
                        evt.Content ?? "",
                        evt.CreatedAt ?? DateTimeOffset.UtcNow));
                }
            }
        }

        Client.Dispose();
    }
    
    public List<Coordinator> GetCoordinators()
    {
        lock(Lock)
        {
            return Coordinators;
        }
    }

    public record Coordinator(string PubKey, string Endpoint, string Content, DateTimeOffset LastUpdate);
}