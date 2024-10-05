using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Utils.Rpc;

namespace LiquiSabi.ApplicationCore.Rpc;

public class LiquiSabiRpc : IJsonRpcService
{
    [JsonRpcMethod("rounds")]
    public IEnumerable<CoinjoinStore.SavedRound> GetUnspentCoinList(DateTimeOffset? since = null, DateTimeOffset? until = null, string? coordinatorEndpoint = null)
    {
        return CoinjoinStore.GetSavedRounds(since, until, coordinatorEndpoint);
    }
}