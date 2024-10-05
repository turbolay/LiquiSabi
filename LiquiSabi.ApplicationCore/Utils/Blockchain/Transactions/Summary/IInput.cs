using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.Blockchain.Transactions.Summary;

public interface IInput
{
	Money? Amount { get; }
}
