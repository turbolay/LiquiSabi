using Microsoft.Data.Sqlite;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Analysis;
using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models.MultipartyTransaction;

namespace LiquiSabi.ApplicationCore.Data;

public static class CoinjoinStore
{
    private static readonly string CoinjoinStorePath = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("LiquiSabi", "Client")), "CoinjoinStore.sqlite");

    /// <summary>Coinjoin disk storage.</summary>
    /// <remarks>Guarded by <see cref="Lock"/>.</remarks>
    private static CoinjoinStoreSqliteStorage Store { get; }
    private static object Lock { get; } = new();

    static CoinjoinStore()
    {
        Store = CreateCoinjoinStoreSqliteStorage();
    }

    private static CoinjoinStoreSqliteStorage CreateCoinjoinStoreSqliteStorage()
    {
        try
        {
            return CoinjoinStoreSqliteStorage.FromFile(dataSource: CoinjoinStorePath);
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == 11) // 11 ~ SQLITE_CORRUPT error code
        {
            Logger.LogError("Failed to open SQLite storage file because it's corrupted.");
            throw;
        }
    }


    public static void AddToStore(RoundDataReaderService.ProcessedRound round)
    {
        var signingState = (SigningState)round.Round.CoinjoinState;
        var transaction = signingState.CreateTransaction();
        var txId = transaction.GetHash().ToString();
        var coordinatorEndpoint = round.Coordinator.Endpoint;
        var roundId = round.Round.Id.ToString();
        var isBlame = round.Round.IsBlame();
        var roundStartTime = round.Round.InputRegistrationStart;
        var roundEndTime = round.LastUpdate;
        var coordinationFeeRate = signingState.Parameters.CoordinationFeeRate.Rate;
        var minInputCount = signingState.Parameters.MinInputCountByRound;
        var parametersMiningFeeRate = signingState.Parameters.MiningFeeRate.SatoshiPerByte;
        var totalInputAmount = signingState.Inputs.Sum(x => x.Amount);
        var inputCount = signingState.Inputs.Count();
        var standardInputs = signingState.Inputs.Where(x => BlockchainAnalyzer.StdDenoms.Contains(x.Amount.Satoshi));
        var averageStandardInputsAnonSet = Math.Round(round.Round.GetInputsAnonSet(standardInputs.Select(x => x.Amount).ToList()).Average(x => x.Value), 2);
        var freshInputsEstimateBtc = Analyzer.EstimateFreshBtc([round]);
        var totalOutputAmount = signingState.Outputs.Sum(x => x.Value);
        var outputCount = signingState.Outputs.Count();
        var standardOutputs = signingState.Outputs.Where(x => BlockchainAnalyzer.StdDenoms.Contains(x.Value.Satoshi));
        var changeOutputs = signingState.Outputs.Where(x => !BlockchainAnalyzer.StdDenoms.Contains(x.Value.Satoshi));
        var changeOutputsAmountRatio = Math.Round((double)changeOutputs.Sum(x => x.Value.Satoshi) / totalOutputAmount, 4);
        var standardOutputsAnonSet = round.Round.GetOutputsAnonSet(standardOutputs.Select(x => x.Value).ToList());
        var averageStandardOutputsAnonSet = Math.Round(standardOutputsAnonSet.Average(y => y.Value), 2);
        var totalMiningFee = totalInputAmount - totalOutputAmount;
        var virtualSize = transaction.GetVirtualSize();
        var finalMiningFeeRate = Math.Round(totalMiningFee / (decimal) virtualSize, 2);
        var coordinatorOutput = ((OutputAdded)signingState.Events.Last()).Output.Value!.Satoshi;
        if (BlockchainAnalyzer.StdDenoms.Contains(coordinatorOutput))
        {
            // Ignore coordinator output if it's a standard denom.
            // Better to have false negatives sometimes than false positives more often.
            coordinatorOutput = 0;
        }
        var coordinationFee = (long)(coordinationFeeRate * 100000000 * freshInputsEstimateBtc);
        var totalLeftovers = coordinatorOutput == 0 ? 0 : coordinatorOutput - coordinationFee;

        if (totalLeftovers > signingState.Parameters.AllowedInputAmounts.Min.Satoshi * (inputCount / 10.0m))
        {
            totalLeftovers = 0;
        }
        
        var estimatedCoordinatorEarningsSats = Math.Max(0, totalLeftovers + coordinationFee);

        var savedRound = new SavedRound(
            coordinatorEndpoint,
            estimatedCoordinatorEarningsSats,
            roundId,
            isBlame,
            coordinationFeeRate,
            minInputCount,
            parametersMiningFeeRate,
            roundStartTime,
            roundEndTime,
            txId,
            finalMiningFeeRate,
            virtualSize,
            totalMiningFee,
            inputCount,
            totalInputAmount,
            freshInputsEstimateBtc,
            averageStandardInputsAnonSet,
            outputCount,
            totalOutputAmount,
            changeOutputsAmountRatio,
            averageStandardOutputsAnonSet,
            totalLeftovers);

        lock (Lock)
        {
            if (Store.IsTxIdKnown(txId))
            {
                return;
            }
            
            Logger.LogInfo($"Coinjoin: {savedRound.CoordinatorEndpoint} - {savedRound.TxId}");
            Store.Add(savedRound);
        }
    }

    public static IEnumerable<SavedRound> GetSavedRounds(DateTimeOffset? since = null,
        DateTimeOffset? until = null,
        IEnumerable<string>? coordinatorEndpoint = null)
    {
        lock (Lock)
        {
            return Store.Get(since, until, coordinatorEndpoint);
        }
    }

    public static bool IsTxIdKnown(string txId)
    {
        lock (Lock)
        {
            return Store.IsTxIdKnown(txId);
        }
    }
    
    public record SavedRound(
        string CoordinatorEndpoint,
        long EstimatedCoordinatorEarningsSats,
        string RoundId,
        bool IsBlame,
        decimal CoordinationFeeRate,
        int MinInputCount,
        decimal ParametersMiningFeeRate,
        DateTimeOffset RoundStartTime,
        DateTimeOffset RoundEndTime,
        string TxId,
        decimal FinalMiningFeeRate,
        int VirtualSize,
        long TotalMiningFee,
        int InputCount,
        long TotalInputAmount,
        decimal FreshInputsEstimateBtc,
        double AverageStandardInputsAnonSet,
        int OutputCount,
        long TotalOutputAmount,
        double ChangeOutputsAmountRatio,
        double AverageStandardOutputsAnonSet,
        long TotalLeftovers);
}