namespace LiquiSabi.ApplicationCore.Interfaces;

public interface IRpcServerController
{
    Task StartRpcServerAsync(CancellationToken cancel);
}