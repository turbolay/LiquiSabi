using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class StoppedEventArgs : StatusChangedEventArgs
{
	public StoppedEventArgs(IWallet wallet, StopReason reason)
		: base(wallet)
	{
		Reason = reason;
	}

	public StopReason Reason { get; }
}
