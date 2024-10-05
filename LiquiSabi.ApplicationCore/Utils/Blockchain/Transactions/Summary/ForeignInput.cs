using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.Blockchain.Transactions.Summary;

public class ForeignInput : IInput
{
	public Money? Amount => default;
}
