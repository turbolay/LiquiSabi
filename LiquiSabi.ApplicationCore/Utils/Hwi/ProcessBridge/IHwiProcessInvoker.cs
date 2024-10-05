namespace LiquiSabi.ApplicationCore.Utils.Hwi.ProcessBridge;

public interface IHwiProcessInvoker
{
	Task<(string response, int exitCode)> SendCommandAsync(string arguments, bool openConsole, CancellationToken cancel, Action<StreamWriter>? standardInputWriter = null);
}
