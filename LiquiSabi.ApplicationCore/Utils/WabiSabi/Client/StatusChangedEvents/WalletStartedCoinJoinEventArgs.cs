using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class WalletStartedCoinJoinEventArgs : StatusChangedEventArgs
{
	public WalletStartedCoinJoinEventArgs(IWallet wallet) : base(wallet)
	{
	}
}
