using System.Net.WebSockets;
using LiquiSabi.ApplicationCore.Utils.Logging;
using Microsoft.Extensions.Hosting;
using NNostr.Client;

namespace LiquiSabi.ApplicationCore.Publishing.Nostr;

public class CoordinatorDiscovery : BackgroundService
{
    private const string NetworkTypeDetected = "Mainnet";

    private static List<string> Relays { get; } =
    [
        "wss://relay.primal.net"
    ];

    private const int Kind = 15750;
    private const string TypeTagIdentifier = "type";
    private const string TypeTagValue = "wabisabi";
    private const string NetworkTagIdentifier = "network";
    private const string EndpointTagIdentifier = "endpoint";
    
    public static List<Coordinator> Coordinators { get; } = new();
    private object Lock { get; } = new();

    private CompositeNostrClient? Client { get; set; }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Coordinators.Add(
            new Coordinator(
                "",
                "https://api.opencoordinator.org/",
                DateTimeOffset.UtcNow, 
                "OpenCoordinator", 
                "OpenCoordinator is bitcoin coinjoin coordinator for Wasabi Wallet 2.x.\n\nIt is open to everyone, no country blocklists, no UTXO blocklists.\n\nOpenCoordinator is run by experienced team of anonymous freedom activists.",
                "https://www.opencoordinator.org/", 
                "21"));
        Logger.LogInfo("Listening to https://api.opencoordinator.org/");
        
        Coordinators.Add(
            new Coordinator(
                "",
                "https://coinjoin.nl/", 
                DateTimeOffset.UtcNow,
                "Noderunners Batched Transaction Coordinator", 
                "WabiSabi Coordinator for Wasabi Wallet / BTCPay / Trezor",
                "https://coinjoin.nl/",
                "21"));
        Logger.LogInfo("Listening to https://coinjoin.nl/");
        
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

        var cts = new CancellationTokenSource(5000);
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

                if (endpoint.Contains("ginger"))
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
                    var name =
                        evt.Tags.FirstOrDefault(x => x.TagIdentifier == "name")?.Data.FirstOrDefault() ?? "";
                    var readMore =
                        evt.Tags.FirstOrDefault(x => x.TagIdentifier == "readmore")?.Data.FirstOrDefault() ?? "";
                    var absolutemininputcount =
                        evt.Tags.FirstOrDefault(x => x.TagIdentifier == "absolutemininputcount")?.Data.FirstOrDefault() ?? "21";
                    Coordinators.Add(new Coordinator(
                        evt.PublicKey,
                        endpoint,
                        evt.CreatedAt ?? DateTimeOffset.UtcNow,
                        name,
                        evt.Content ?? "",
                        readMore,
                        absolutemininputcount
                        ));
                    
                    Logger.LogInfo($"Listening to {endpoint}");
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

    public record Coordinator(string PubKey, string Endpoint, DateTimeOffset LastUpdate, string Name, string Content, string ReadMore, string AbsoluteMinInputCount);
}