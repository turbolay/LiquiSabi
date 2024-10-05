using System.Net;
using LiquiSabi.ApplicationCore.Interfaces;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.Rpc;

namespace LiquiSabi.ApplicationCore.Rpc;

public class RpcServerController : IRpcServerController
{
    private readonly JsonRpcServer _rpcServer;
    private readonly JsonRpcServerConfiguration _jsonRpcServerConfiguration;

    public RpcServerController(JsonRpcServer rpcServer, JsonRpcServerConfiguration jsonRpcServerConfiguration)
    {
        _rpcServer = rpcServer;
        _jsonRpcServerConfiguration = jsonRpcServerConfiguration;
    }

    public async Task StartRpcServerAsync(CancellationToken cancel)
    {
        if (_jsonRpcServerConfiguration.IsEnabled)
        {
            try
            {
                await _rpcServer
                    .StartAsync(cancel)
                    .ConfigureAwait(false);
            }
            catch (HttpListenerException e)
            {
                Logger.LogWarning($"Failed to start {nameof(JsonRpcServer)} with error: {e.Message}.");
            }
        }
    }
}