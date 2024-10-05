using LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.CoinJoinProgressEvents;
using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class CoinJoinStatusEventArgs : StatusChangedEventArgs
{
	public CoinJoinStatusEventArgs(IWallet wallet, CoinJoinProgressEventArgs coinJoinProgressEventArgs) : base(wallet)
	{
		CoinJoinProgressEventArgs = coinJoinProgressEventArgs;
	}

	public CoinJoinProgressEventArgs CoinJoinProgressEventArgs { get; }
}
