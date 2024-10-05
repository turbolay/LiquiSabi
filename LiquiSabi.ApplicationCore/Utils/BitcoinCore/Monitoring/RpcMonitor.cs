using LiquiSabi.ApplicationCore.Utils.Bases;
using LiquiSabi.ApplicationCore.Utils.BitcoinCore.Rpc;
using LiquiSabi.ApplicationCore.Utils.Extensions;

namespace LiquiSabi.ApplicationCore.Utils.BitcoinCore.Monitoring;

public class RpcMonitor : PeriodicRunner
{
	private RpcStatus _rpcStatus;

	public RpcMonitor(TimeSpan period, IRPCClient rpcClient) : base(period)
	{
		_rpcStatus = RpcStatus.Unresponsive;
		RpcClient = rpcClient;
	}

	public event EventHandler<RpcStatus>? RpcStatusChanged;

	public IRPCClient RpcClient { get; set; }

	public RpcStatus RpcStatus
	{
		get => _rpcStatus;
		private set
		{
			if (value != _rpcStatus)
			{
				_rpcStatus = value;
				RpcStatusChanged?.Invoke(this, value);
			}
		}
	}

	protected override async Task ActionAsync(CancellationToken cancel)
	{
		RpcStatus = await RpcClient.GetRpcStatusAsync(cancel).ConfigureAwait(false);
	}
}
