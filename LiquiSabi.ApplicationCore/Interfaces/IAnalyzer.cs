using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Interfaces;

public interface IAnalyzer
{
    Analyzer.Analysis? AnalyzeRounds(List<CoinjoinStore.SavedRound> successes, string coordinatorEndpoint, DateTimeOffset start, DateTimeOffset end);
    public Task StartAsync(CancellationToken cancellationToken);
}