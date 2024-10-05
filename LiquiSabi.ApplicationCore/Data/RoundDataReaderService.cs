using Microsoft.Extensions.Hosting;
using NBitcoin;
using LiquiSabi.ApplicationCore.Interfaces;
using LiquiSabi.ApplicationCore.Publishing.Nostr;
using LiquiSabi.ApplicationCore.Utils.Affiliation.Models;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Data;

public class RoundDataReaderService : BackgroundService, IRoundDataReaderService
{
    private readonly Scraper _scraper;
    private Dictionary<uint256, ProcessedRound> Rounds { get; } = new();
    private List<uint256> RemovedRoundsId { get; } = new();
    private object Lock { get; } = new();
    public HumanMonitorResponse? LastHumanMonitor { get; private set; }

    public RoundDataReaderService(Scraper scraper)
    {
        _scraper = scraper;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (await _scraper.ToBeProcessedData.Reader.WaitToReadAsync(token) || !token.IsCancellationRequested)
        {
            var data = await _scraper.ToBeProcessedData.Reader.ReadAsync(token);
            lock (Lock)
            {
                foreach (var round in data.Rounds.RoundStates)
                {
                    if (RemovedRoundsId.Contains(round.Id))
                    {
                        continue;
                    }
                    var humanMonitor = data.HumanMonitor.RoundStates.FirstOrDefault(x => x.RoundId == round.Id);
                    if (!Rounds.TryGetValue(round.Id, out var oldInstance))
                    {
                        var processedRound = new ProcessedRound(
                            data.Coordinator,
                            data.ScrapedAt,
                            humanMonitor,
                            round,
                            data.Rounds.AffiliateInformation,
                            data.Rounds.CoinJoinFeeRateMedians);
                        
                        Rounds.Add(
                            round.Id,
                            processedRound
                            );
                        
                        if (round.EndRoundState == EndRoundState.TransactionBroadcasted)
                        {
                            CoinjoinStore.AddToStore(processedRound);
                        }
                        
                        continue;
                    }

                    // Do not update finished round.
                    if (oldInstance.Round.EndRoundState != EndRoundState.None)
                    {
                        continue;
                    }

                    if (round.EndRoundState == EndRoundState.TransactionBroadcasted)
                    {
                        var processedRound = new ProcessedRound(
                            data.Coordinator,
                            data.ScrapedAt,
                            humanMonitor,
                            round,
                            data.Rounds.AffiliateInformation,
                            data.Rounds.CoinJoinFeeRateMedians);
                        
                        CoinjoinStore.AddToStore(processedRound);
                    }
                    oldInstance.Round = round;
                    oldInstance.LastUpdate = data.ScrapedAt;

                    if (humanMonitor is not null)
                    {
                        oldInstance.HumanMonitor = humanMonitor;
                    }
                }
            }
        }
    }

    public Dictionary<uint256, ProcessedRound> GetRounds()
    {
        lock (Lock)
        {
            return Rounds;
        }
    }

    public void RemoveRounds(IEnumerable<ProcessedRound> rounds)
    {
        lock (Lock)
        {
            foreach (var round in rounds)
            {
                Rounds.Remove(round.Round.Id);
                RemovedRoundsId.Add(round.Round.Id);
            }
        }
    }

    public record ProcessedRound(CoordinatorDiscovery.Coordinator Coordinator, DateTimeOffset LastUpdate, HumanMonitorRoundResponse? HumanMonitor, RoundState Round, AffiliateInformation Affiliates,
        CoinJoinFeeRateMedian[] CoinJoinFeeRateMedian)
    {
        public HumanMonitorRoundResponse? HumanMonitor { get; set; } = HumanMonitor;
        public DateTimeOffset LastUpdate { get; set; } = LastUpdate;
        public RoundState Round { get; set; } = Round;
    }
}