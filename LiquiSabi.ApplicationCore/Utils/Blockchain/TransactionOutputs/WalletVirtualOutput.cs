using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.Blockchain.TransactionOutputs;

public class WalletVirtualOutput
{
	public WalletVirtualOutput(byte[] id, ISet<SmartCoin> coins)
	{
		Id = id;
		Coins = coins;
		Amount = coins.Sum(x => x.Amount);
	}

	public byte[] Id { get; }
	public Money Amount { get; }
	public ISet<SmartCoin> Coins { get; }
}
