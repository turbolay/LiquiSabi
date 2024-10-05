using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Utils.Rpc;

namespace LiquiSabi.ApplicationCore.Rpc;

public class LiquiSabiRpc : IJsonRpcService
{
    [JsonRpcMethod("rounds")]
    public IEnumerable<CoinjoinStore.SavedRound> GetRounds(DateTimeOffset? since = null, DateTimeOffset? until = null, IEnumerable<string>? coordinatorEndpoint = null)
    {
        return CoinjoinStore.GetSavedRounds(since, until, coordinatorEndpoint);
    }
    
    [JsonRpcMethod("average")]
    public CoinjoinStore.SavedRound GetSummary(DateTimeOffset? since = null, DateTimeOffset? until = null, IEnumerable<string>? coordinatorEndpoint = null)
    {
        var rounds = GetRounds(since, until, coordinatorEndpoint).ToList();
        return new CoinjoinStore.SavedRound(
            CoordinatorEndpoint: string.Join(';', rounds.Select(x => x.CoordinatorEndpoint).Distinct()),
            EstimatedCoordinatorEarningsSats: (int)rounds.Average(x => x.EstimatedCoordinatorEarningsSats),
            RoundId: rounds.Count.ToString(),
            IsBlame: false,
            CoordinationFeeRate: Math.Round(rounds.Average(x => x.CoordinationFeeRate), 4),
            MinInputCount: (int)rounds.Average(x => x.MinInputCount),
            ParametersMiningFeeRate: Math.Round(rounds.Average(x => x.ParametersMiningFeeRate), 2),
            RoundStartTime: rounds.Min(x => x.RoundStartTime),
            RoundEndTime: rounds.Max(x => x.RoundEndTime),
            TxId: rounds.Count.ToString(),
            FinalMiningFeeRate: Math.Round(rounds.Average(x => x.FinalMiningFeeRate), 2),
            VirtualSize: (int)rounds.Average(x => x.VirtualSize),
            TotalMiningFee: (long)rounds.Average(x => x.TotalMiningFee),
            InputCount: (int)rounds.Average(x => x.InputCount),
            TotalInputAmount: (int)rounds.Average(x => x.TotalInputAmount),
            FreshInputsEstimateBtc: Math.Round(rounds.Average(x => x.FreshInputsEstimateBtc), 8),
            AverageStandardInputsAnonSet: Math.Round(rounds.Average(x => x.AverageStandardInputsAnonSet), 2),
            OutputCount: (int)rounds.Average(x => x.OutputCount),
            TotalOutputAmount: (long)rounds.Average(x => x.TotalOutputAmount),
            ChangeOutputsAmountRatio: Math.Round(rounds.Average(x => x.ChangeOutputsAmountRatio), 2),
            AverageStandardOutputsAnonSet: Math.Round(rounds.Average(x => x.AverageStandardOutputsAnonSet), 2),
            TotalLeftovers: (int)rounds.Average(x => x.TotalLeftovers));
    }
}