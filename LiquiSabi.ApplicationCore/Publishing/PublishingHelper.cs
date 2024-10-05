using LiquiSabi.ApplicationCore.Data;

namespace LiquiSabi.ApplicationCore.Publishing;

public static class MarketingHelper
{
    public static string BuildContent(List<Analyzer.Analysis> analyses, List<string> freeCoordinatorsWithoutSuccesses)
    {
        var first = analyses.First();
        var result = GetFriendlyDate(first);
        result += $"Full data: {DeploymentConfiguration.Website}\n\n";
        analyses = OrderRounds(analyses);
        foreach (var analysis in analyses)
        {
            result += CoordinatorInfoToString(analysis);
            result = AddStringAnalysis(result, analysis);
        }

        if (freeCoordinatorsWithoutSuccesses.Count <= 0)
        {
            return result;
        }
        
        result += "Free coordinators without any successful round:\n";
        foreach (var coord in freeCoordinatorsWithoutSuccesses)
        {
            result += coord + "\n";
        }
        return result;
    }

    public static string? BuildSummary(List<Analyzer.Analysis> analyses)
    {
        if (!analyses.Any(x => x.TotalBtc > 0))
        {
            return null;
        }
        var first = analyses.First();
        var result = GetFriendlyDate(first);
        
        result += $"Full data: {DeploymentConfiguration.Website}\n\n";

        var summary = new Analyzer.Analysis(
            first.StartTime,
            first.EndTime,
            first.CoordinatorEndpoint,
            first.CoordinatorFee,
            (uint)analyses.Sum(x => x.TotalSuccesses),
            (int)analyses.Average(x => x.AverageInputs),
            analyses.Min(x => x.MinInputs),
            analyses.Max(x => x.MaxInputs),
            analyses.Sum(x => x.TotalBtc),
            analyses.Sum(x => x.EstimateFreshBtc),
            Math.Round(analyses.Average(x => x.AverageFeeRate), 2),
            Math.Round(analyses.Average(x => x.AverageOutputsAnonSet), 2)
        );
        
        result = AddStringAnalysis(result, summary);

        return result;
    }
    
    private static string CoordinatorInfoToString(Analyzer.Analysis analysis, bool addSpace = true)
    {
        var result = $"{(addSpace ? ReplaceLastOccurrence(analysis.CoordinatorEndpoint, '.', " .") : analysis.CoordinatorEndpoint)}\n";
        result += $"Fee: {analysis.CoordinatorFee} %\n";
        return result;
    }
    
    private static List<Analyzer.Analysis> OrderRounds(List<Analyzer.Analysis> analyses)
    {
        return analyses.OrderByDescending(x => x.EstimateFreshBtc).ThenByDescending(x => x.TotalBtc).ToList();
    }

    private static string AddStringAnalysis(string currentMessage, Analyzer.Analysis analysis)
    {
        currentMessage += analysis.TotalSuccesses == 1 ? "1 round\n" : $"{analysis.TotalSuccesses} rounds\n";
        currentMessage += $"New: {analysis.EstimateFreshBtc} BTC (est.) | Remixing: {Math.Round(analysis.TotalBtc - analysis.EstimateFreshBtc, 1)} BTC\n";
        currentMessage += $"Avg: {analysis.AverageInputs} inputs | {analysis.AverageOutputsAnonSet} AS | {analysis.AverageFeeRate} s/vb\n";
        currentMessage += $"Min: {analysis.MinInputs} inputs | Max: {analysis.MaxInputs} inputs\n\n";

        return currentMessage;
    }

    private static string GetFriendlyDate(Analyzer.Analysis analysis)
    {
        return $"WabiSabi coinjoins on {analysis.StartTime:MMMM dd}\n\n";
    }
    
    
    static string ReplaceLastOccurrence(string source, char find, string replace)
    {
        int place = source.LastIndexOf(find);
    
        if (place == -1)
        {
            return source;
        }

        string result = source.Remove(place, 1).Insert(place, replace);
        return result;
    }
}