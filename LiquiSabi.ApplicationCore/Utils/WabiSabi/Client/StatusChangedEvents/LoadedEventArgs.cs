using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class LoadedEventArgs : StatusChangedEventArgs
{
	public LoadedEventArgs(IWallet wallet)
		: base(wallet)
	{
	}
}
