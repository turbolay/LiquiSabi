using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class WalletStoppedCoinJoinEventArgs : StatusChangedEventArgs
{
	public WalletStoppedCoinJoinEventArgs(IWallet wallet) : base(wallet)
	{
	}
}
