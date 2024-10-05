using NBitcoin;
using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Interfaces;

public interface IRoundDataReaderService
{
    Dictionary<uint256, RoundDataReaderService.ProcessedRound> GetRounds();
    void RemoveRounds(IEnumerable<RoundDataReaderService.ProcessedRound> rounds);
    Task? ExecuteTask { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    void Dispose();
}