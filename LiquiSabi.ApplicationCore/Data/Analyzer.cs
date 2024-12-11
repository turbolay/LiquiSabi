using Microsoft.Extensions.Hosting;
using NBitcoin;
using LiquiSabi.ApplicationCore.Interfaces;
using LiquiSabi.ApplicationCore.Publishing.Nostr;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Data;

public class Analyzer : BackgroundService, IAnalyzer
{
    private static readonly bool FilterOutForProfitCoordinators = true;
    public List<Analysis> CurrentAnalysis { get; set; } = new();
    private object Lock { get; } = new();
    private IRoundDataReaderService RoundDataReaderService { get; }
    public Analyzer(IRoundDataReaderService roundDataReaderService)
    {
        RoundDataReaderService = roundDataReaderService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var endOfDay = now.Date.AddDays(1);
            var timeUntilEndOfDay = endOfDay - now;

            await Task.Delay(timeUntilEndOfDay, cancellationToken);
            var startOfDayDateTime = now.Date;
            var endOfDayDateTime = endOfDay;

            var rounds = CoinjoinStore.GetSavedRounds(startOfDayDateTime, endOfDayDateTime).ToList();

            var coordinatorGroups = rounds.GroupBy(x => x.CoordinatorEndpoint);
            List<Analysis?> analyses = [];
            foreach (var group in coordinatorGroups)
            {
                analyses.Add(AnalyzeRounds(group.Select(x => x).ToList(), group.Key, startOfDayDateTime, endOfDayDateTime));
            }

            List<Analysis> toPublishCoordinators;
            lock (Lock)
            {
                CurrentAnalysis = analyses.Where(x => x is not null).ToList()!;

                if (FilterOutForProfitCoordinators)
                {
                    toPublishCoordinators = CurrentAnalysis
                        .Where(x =>
                            rounds
                                .Where(y => y.CoordinatorEndpoint == x.CoordinatorEndpoint)
                                .All(y => y.CoordinationFeeRate  == 0.0m))
                        .ToList();
                }
                else
                {
                    toPublishCoordinators = CurrentAnalysis.ToList();
                }
            }
            if (toPublishCoordinators.Count == 0 ||
                toPublishCoordinators.Max(x => x.TotalSuccesses) == 0)
            {
                return;
            }

            var allCoordinatorsWithFinishedRounds = RoundDataReaderService.GetRounds().ToList();
            RoundDataReaderService.RemoveRounds(allCoordinatorsWithFinishedRounds.Select(x => x.Value));

            var freeCoordinatorsWithoutSuccesses = allCoordinatorsWithFinishedRounds
                .GroupBy(x => x.Value.Coordinator.Endpoint)
                .Where(x => x.Max(y => y.Value.Round.CoinjoinState.Parameters.CoordinationFeeRate.Rate == 0.0m))
                .Select(x => x.Key)
                .Where(x => toPublishCoordinators.All(y => y.CoordinatorEndpoint != x))
                .ToList();

            try
            {
                await StatisticsPublishing.PublishToNostr(toPublishCoordinators, freeCoordinatorsWithoutSuccesses, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogInfo("Suppression of Nostr publishing exception");
            }

            try
            {
                await Publishing.Twitter.StatisticsPublishing.PublishToTwitter(toPublishCoordinators, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogInfo("Suppression of Twitter publishing exception");
            }
        }
    }
    
    public Analysis? AnalyzeRounds(List<CoinjoinStore.SavedRound> successes, string coordinatorEndpoint, DateTimeOffset start, DateTimeOffset end)
    {
        if (successes.Count == 0)
        {
            return null;
        }
        
        decimal averageFeeRate = 0;
        var outputsAnonSet = 0.0m;
        var minInputsCount = 0;
        var maxInputsCount = 0;
        if (successes.Count != 0)
        {
            averageFeeRate = successes.Average(x => x.FinalMiningFeeRate);
            outputsAnonSet = (decimal)successes.Average(x => x.AverageStandardOutputsAnonSet);
            minInputsCount = successes.Min(x => x.InputCount);
            maxInputsCount = successes.Max(x => x.InputCount);
        }

        var coordinationFeePercent = Math.Round(successes.Max(x => x.CoordinationFeeRate * 100), 3);
        var totalInputsCount = successes.Sum(x => x.InputCount);
        var totalBtc = successes.Sum(x => x.TotalInputAmount) / 100000000.0m;
        var totalEstimateFreshBtc = successes.Sum(x => x.FreshInputsEstimateBtc);
        
        return new Analysis(
            start,
            end,
            coordinatorEndpoint,
            coordinationFeePercent,
            (uint)successes.Count,
            (int)Math.Round((totalInputsCount / (double)successes.Count)),
            minInputsCount,
            maxInputsCount,
            Math.Round(totalBtc, 1),
            Math.Round(totalEstimateFreshBtc, 1),
            Math.Round(averageFeeRate, 2),
            Math.Round(outputsAnonSet, 2)
            );
    }
    
    public static decimal EstimateFreshBtc(List<RoundDataReaderService.ProcessedRound> successes)
    {
        if (successes.Count == 0)
        {
            return 0.0m;
        }
        
        return successes.SelectMany(x => x.Round.CoinjoinState.Inputs)
            .GroupBy(x => x.Outpoint.Hash)
            .Where(x => x.Count() == 1) // Only transactions where a single coin is originated
            .Where(x => x.First().Outpoint.N < 5) // Only if index is lower than 5
            .Where(x =>!CoinjoinStore.IsTxIdKnown(x.First().Outpoint.Hash.ToString())) // Our ID store doesn't have it
            .Sum(x => x.Sum(y => y.Amount.ToUnit(MoneyUnit.BTC)));
    }
    
    private decimal CalculateAverageFeeRate(List<RoundState> successes)
    {
        return Math.Round(successes.Average(x => x.GetFeeRate().SatoshiPerByte));
    }
    
    public record Analysis(DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        string CoordinatorEndpoint,
        decimal CoordinatorFee,
        uint TotalSuccesses,
        int AverageInputs,
        int MinInputs,
        int MaxInputs,
        decimal TotalBtc,
        decimal EstimateFreshBtc,
        decimal AverageFeeRate,
        decimal AverageOutputsAnonSet);
}