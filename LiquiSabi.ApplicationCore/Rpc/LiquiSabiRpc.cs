using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Utils.Rpc;

namespace LiquiSabi.ApplicationCore.Rpc;

public class LiquiSabiRpc : IJsonRpcService
{
    [JsonRpcMethod("rounds")]
    public IEnumerable<CoinjoinStore.SavedRound> GetRounds(DateTimeOffset? since = null, DateTimeOffset? until = null, string? coordinatorEndpoint = null)
    {
        return CoinjoinStore.GetSavedRounds(since, until, coordinatorEndpoint);
    }
    
    [JsonRpcMethod("summary")]
    public CoinjoinStore.SavedRound GetSummary(DateTimeOffset? since = null, DateTimeOffset? until = null, string? coordinatorEndpoint = null)
    {
        var rounds = GetRounds(since, until, coordinatorEndpoint).ToList();
        return new CoinjoinStore.SavedRound(
            CoordinatorEndpoint: string.Join(';', rounds.Select(x => x.CoordinatorEndpoint).Distinct()),
            EstimatedCoordinatorEarningsSats: rounds.Sum(x => x.EstimatedCoordinatorEarningsSats),
            RoundId: rounds.Count.ToString(),
            IsBlame: false,
            CoordinationFeeRate: Math.Round(rounds.Average(x => x.CoordinationFeeRate), 4),
            MinInputCount: (int)Math.Round(rounds.Average(x => x.MinInputCount)),
            ParametersMiningFeeRate: Math.Round(rounds.Average(x => x.ParametersMiningFeeRate), 2),
            RoundStartTime: rounds.Min(x => x.RoundStartTime),
            RoundEndTime: rounds.Max(x => x.RoundEndTime),
            TxId: rounds.Count.ToString(),
            FinalMiningFeeRate: Math.Round(rounds.Average(x => x.FinalMiningFeeRate), 2),
            VirtualSize: (int)Math.Round(rounds.Average(x => x.VirtualSize)),
            TotalMiningFee: rounds.Sum(x => x.TotalMiningFee),
            InputCount: rounds.Sum(x => x.InputCount),
            TotalInputAmount: rounds.Sum(x => x.TotalInputAmount),
            FreshInputsEstimateBtc: rounds.Sum(x => x.FreshInputsEstimateBtc),
            AverageStandardInputsAnonSet: Math.Round(rounds.Average(x => x.AverageStandardInputsAnonSet), 2),
            OutputCount: rounds.Sum(x => x.OutputCount),
            TotalOutputAmount: rounds.Sum(x => x.TotalOutputAmount),
            ChangeOutputsAmountRatio: Math.Round(rounds.Average(x => x.ChangeOutputsAmountRatio), 2),
            AverageStandardOutputsAnonSet: Math.Round(rounds.Average(x => x.AverageStandardOutputsAnonSet), 2),
            TotalLeftovers: rounds.Sum(x => x.TotalLeftovers));
    }
}