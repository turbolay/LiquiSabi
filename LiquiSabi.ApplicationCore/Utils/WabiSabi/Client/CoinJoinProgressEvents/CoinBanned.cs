using LiquiSabi.ApplicationCore.Utils.Blockchain.TransactionOutputs;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.CoinJoinProgressEvents;

public class CoinBanned : CoinJoinProgressEventArgs
{
	public CoinBanned(SmartCoin coin, DateTimeOffset banUntilUtc)
	{
		Coin = coin;
		BanUntilUtc = banUntilUtc;
	}

	public SmartCoin Coin { get; }
	public DateTimeOffset BanUntilUtc { get; }
}
