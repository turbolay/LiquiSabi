using System.Text;
using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Utils.Nito.AsyncEx;
using LiquiSabi.ApplicationCore.Utils.Rpc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiquiSabi.ApplicationCore.Rpc;

public class LiquiSabiRpc : IJsonRpcService
{
    [JsonRpcMethod("rounds")]
    public IEnumerable<CoinjoinStore.SavedRound> GetRounds(DateTimeOffset? since = null, DateTimeOffset? until = null, IEnumerable<string>? coordinatorEndpoint = null)
    {
        return CoinjoinStore.GetSavedRounds(since, until, coordinatorEndpoint);
    }

    public record GraphEntry(string Date, CoinjoinStore.SavedRound? Averages );
    
    [JsonRpcMethod("graph")]
    public List<GraphEntry> GetGraph(IEnumerable<string>? coordinatorEndpoint = null)
    {
        List<GraphEntry> result = [];
        DateTimeOffset until = DateTimeOffset.UtcNow.Date - TimeSpan.FromDays(1);
        DateTimeOffset since = until - TimeSpan.FromDays(30);

        var currentDate = since;
        
        while (currentDate <= until)
        {
            var dayStart = currentDate;
            var dayEnd = currentDate.AddDays(1);
        
            var summary = GetSummary(dayStart, dayEnd, coordinatorEndpoint);
            result.Add(new GraphEntry(
                currentDate.ToString("dd/MM"),
                summary
            ));
        
            currentDate = currentDate.AddDays(1);
        }
    
        return result;
    }
    
    [JsonRpcMethod("average")]
    public CoinjoinStore.SavedRound? GetSummary(DateTimeOffset? since = null, DateTimeOffset? until = null, IEnumerable<string>? coordinatorEndpoint = null)
    {
        var rounds = GetRounds(since, until, coordinatorEndpoint).ToList();
        if (rounds.Count == 0)
        {
            return null;
        }
        return new CoinjoinStore.SavedRound(
            CoordinatorEndpoint: string.Join(';', rounds.Select(x => x.CoordinatorEndpoint).Distinct()),
            EstimatedCoordinatorEarningsSats: (long)rounds.Average(x => x.EstimatedCoordinatorEarningsSats),
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
            TotalInputAmount: (long)rounds.Average(x => x.TotalInputAmount),
            FreshInputsEstimateBtc: Math.Round(rounds.Average(x => x.FreshInputsEstimateBtc), 8),
            AverageStandardInputsAnonSet: Math.Round(rounds.Average(x => x.AverageStandardInputsAnonSet), 2),
            OutputCount: (int)rounds.Average(x => x.OutputCount),
            TotalOutputAmount: (long)rounds.Average(x => x.TotalOutputAmount),
            ChangeOutputsAmountRatio: Math.Round(rounds.Average(x => x.ChangeOutputsAmountRatio), 2),
            AverageStandardOutputsAnonSet: Math.Round(rounds.Average(x => x.AverageStandardOutputsAnonSet), 5),
            TotalLeftovers: (int)rounds.Average(x => x.TotalLeftovers));
    }

    private DateTime _lastRequestDonation = DateTime.MinValue;
    private AsyncLock _lock = new();
    [JsonRpcMethod("donation-address")]
    public async Task<string?> GetDonationAddress()
    {
        using (await _lock.LockAsync())
        {
            var now = DateTime.UtcNow;
            var timeSinceLastRequest = now - _lastRequestDonation;
            var minimumWaitTime = TimeSpan.FromSeconds(30);

            if (timeSinceLastRequest < minimumWaitTime)
            {
                var delayTime = minimumWaitTime - timeSinceLastRequest;
                await Task.Delay(delayTime);
            }

            _lastRequestDonation = DateTime.UtcNow;
        }

        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("http://127.0.0.1:37128/LiquiSabi");
    
        var requestBody = new
        {
            jsonrpc = "2.0",
            id = "1",
            method = "getnewaddress",
            @params = new object[] { "LiquiSabi", true }
        };
    
        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage result = await client.PostAsync("", content);

        if (!result.IsSuccessStatusCode)
        {
            return null;
        }
        
        var response = await result.Content.ReadAsStringAsync();
        var json = JObject.Parse(response);

        return !json.TryGetValue("result", out var resultJson) ? 
            null : 
            resultJson["address"]?.ToString();
    }
    
}