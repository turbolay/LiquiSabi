using System.Net.WebSockets;
using NNostr.Client;
using NNostr.Client.Protocols;
using LiquiSabi.ApplicationCore.Data;

namespace LiquiSabi.ApplicationCore.Publishing.Nostr;

public static class StatisticsPublishing
{
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
        "wss://relay.nostr.band"
    ];

    public static async Task<string?> PublishToNostrWithRetries(List<Analyzer.Analysis> analyses, List<string> freeCoordinatorsWithoutSuccesses,
        CancellationToken cancellationToken)
    {
        int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await PublishToNostr(analyses, freeCoordinatorsWithoutSuccesses, cancellationToken);
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1)
                {
                    Console.WriteLine($"Final attempt failed: {ex.Message}. Swallowing exception.");
                }
                else
                {
                    Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}. Retrying...");
                }
            }
        }

        return null;
    }

    private static async Task<string?> PublishToNostr(List<Analyzer.Analysis> analyses,  List<string> freeCoordinatorsWithoutSuccesses, CancellationToken cancellationToken)
    {
        if (analyses.Count == 0)
        {
            return null;
        }
        
        var key = DeploymentConfiguration.Nsec.FromNIP19Nsec();
        
        var newEvent = new NostrEvent()
        {
            Kind = 1,
            Content = MarketingHelper.BuildContent(analyses, freeCoordinatorsWithoutSuccesses)
            
        };
        await newEvent.ComputeIdAndSignAsync(key);

        var tasks = Relays.Select(relay => Task.Run(async () =>
        {
            if (!Uri.TryCreate(relay, UriKind.Absolute, out var uri))
            {
                return false;
            }

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var client = new NostrClient(uri);

            try
            {
                await client.ConnectAndWaitUntilConnected(linkedCts.Token);
                await client.SendEventsAndWaitUntilReceived(new[] { newEvent }, linkedCts.Token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                try
                {
                    client.Dispose();
                }
                catch (Exception)
                {
                    // Disposal exceptions are ignored
                }
            }
        }, CancellationToken.None)).ToList();

        var results = await Task.WhenAll(tasks);

        if (!results.Any(x => x))
        {
            throw new Exception("Didn't manage to publish the nostr event to a single relay");
        }
        
        return newEvent.Id;
    }
}