using NBitcoin;
using LiquiSabi.ApplicationCore.Interfaces;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models.MultipartyTransaction;

namespace LiquiSabi.ApplicationCore.Data;

public static class RoundStateExtensions
{
    public static uint GetOutputsCount(this RoundState roundState) => (uint)roundState.CoinjoinState.Outputs.Count();

    public static uint GetSignaturesCount(this RoundState roundState)
    {
        try
        {
            var signingState = roundState.Assert<SigningState>();
            return (uint)signingState.Witnesses.Count;
        }
        catch
        {
            return 0;
        }
    }

    public static Money GetTotalInputsAmount(this RoundState roundState) => roundState.CoinjoinState.Inputs.Sum(x => x.Amount);

    public static IEnumerable<Money> GetInputAmounts(this RoundState roundState) => roundState.CoinjoinState.Inputs.Select(x => x.Amount);

    public static Money GetTotalOutputsAmount(this RoundState roundState) => roundState.CoinjoinState.Outputs.Sum(x => x.Value);

    public static List<Money> GetOutputsAmounts(this RoundState roundState) => roundState.CoinjoinState.Outputs.Select(x => x.Value).ToList();

    public static bool IsBlame(this RoundState roundState) => roundState.BlameOf != uint256.Zero;

    public static Money GetFee(this RoundState roundState) => GetTotalInputsAmount(roundState) - GetTotalOutputsAmount(roundState);

    public static int GetEstimatedVSize(this RoundState roundState) => roundState.CoinjoinState.EstimatedVsize;

    public static FeeRate GetFeeRate(this RoundState roundState) => roundState.IsSuccess() ? 
        new FeeRate(GetFee(roundState), GetEstimatedVSize(roundState)) : 
        FeeRate.Zero;

    public static Dictionary<Money, uint> GetInputsAnonSet(this RoundState roundState, List<Money>? specificInputs = null) => (specificInputs ?? GetInputAmounts(roundState))
        .GroupBy(n => n)
        .ToDictionary(group => group.Key, group => (uint)group.Count());

    public static Dictionary<Money, uint> GetOutputsAnonSet(this RoundState roundState, List<Money>? specificOutputs = null) => (specificOutputs ?? GetOutputsAmounts(roundState))
        .GroupBy(n => n)
        .ToDictionary(group => group.Key, group => (uint)group.Count());

    public static Phase GetCurrentPhase(this RoundState roundState) => roundState.Phase;

    public static EndRoundState GetEndRoundState(this RoundState roundState) => roundState.EndRoundState;

    public static bool IsOngoing(this RoundState roundState) => roundState.EndRoundState == EndRoundState.None;

    public static bool IsSuccess(this RoundState roundState) => roundState.EndRoundState == EndRoundState.TransactionBroadcasted;

    public static bool IsCancelled(this RoundState roundState) => roundState.EndRoundState is 
        EndRoundState.AbortedLoadBalancing or 
        EndRoundState.AbortedNotEnoughAlices or 
        EndRoundState.AbortedNotAllAlicesConfirmed;

    public static bool IsFailed(this RoundState roundState) => !IsOngoing(roundState) && !IsSuccess(roundState) && !IsCancelled(roundState);

    // This function is special: It is the only one why we need human monitor:
    // it contains this extra info during InputsRegistrationPhase.
    public static uint GetInputsCount(this RoundState roundState, HumanMonitorRoundResponse? humanMonitorRoundResponse = null)
    {
        uint count;
        if (roundState.Phase == Phase.TransactionSigning ||
            roundState.EndRoundState == EndRoundState.TransactionBroadcasted ||
            roundState.EndRoundState == EndRoundState.TransactionBroadcastFailed ||
            roundState.EndRoundState == EndRoundState.NotAllAlicesSign ||
            roundState.EndRoundState == EndRoundState.AbortedNotEnoughAlicesSigned ||
            humanMonitorRoundResponse is null)
        {
            count = (uint)roundState.CoinjoinState.Inputs.Count();
        }
        else
        {
            count = (uint)humanMonitorRoundResponse.InputCount;
        }

        return count;
    }

    public static uint GetConfirmedInputsCount(this RoundState roundState) => (uint)roundState.CoinjoinState.Inputs.Count();
}