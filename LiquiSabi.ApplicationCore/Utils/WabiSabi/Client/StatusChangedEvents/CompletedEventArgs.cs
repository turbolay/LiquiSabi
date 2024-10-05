using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class CompletedEventArgs : StatusChangedEventArgs
{
	public CompletedEventArgs(IWallet wallet, CompletionStatus completionStatus)
		: base(wallet)
	{
		CompletionStatus = completionStatus;
	}

	public CompletionStatus CompletionStatus { get; }
}
