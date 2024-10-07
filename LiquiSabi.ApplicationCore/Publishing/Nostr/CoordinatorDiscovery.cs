using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;
using NNostr.Client;

namespace LiquiSabi.ApplicationCore.Publishing.Nostr;

public class CoordinatorDiscovery : BackgroundService
{
    private const string NetworkTypeDetected = "Mainnet";

    private static List<string> Relays { get; } =
    [
        "wss://relay.primal.net",
        "wss://relay.damus.io",
        "wss://relay.mostr.pub/",
        "wss://nostr.fmt.wiz.biz",
        "wss://nostr.wine/",
        "wss://nos.lol",
        "wss://nostr.land/",
        "wss://offchain.pub/",
        "wss://nostr.oxtr.dev/",
        "wss://eden.nostr.land/",
        "wss://nostr.oxtr.dev",
        "wss://relay.bitcoinpark.com",
        "wss://soloco.nl",
        "wss://relay.nostr.bg/",
        "wss://relay.snort.social/",
        "wss://relay.nostr.band",
        "wss://relay.noderunners.network"
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
        Coordinators.Add(new Coordinator("","https://coinjoin.nl/", "", DateTimeOffset.UtcNow));
        
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

        var cts = new CancellationTokenSource(20000);
        await Client.ConnectAndWaitUntilConnected(cts.Token, cancellationToken);

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