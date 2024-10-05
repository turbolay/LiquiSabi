using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Keys;

namespace LiquiSabi.ApplicationCore.Utils.Blockchain.TransactionOutputs;

public class WalletVirtualInput
{
	public WalletVirtualInput(byte[] id, ISet<SmartCoin> coins)
	{
		Id = id;
		Coins = coins;
		HdPubKey = coins.Select(x => x.HdPubKey).Distinct().Single();
		Amount = coins.Sum(x => x.Amount);
	}

	public byte[] Id { get; }
	public ISet<SmartCoin> Coins { get; }
	public HdPubKey HdPubKey { get; }
	public Money Amount { get; }
}
