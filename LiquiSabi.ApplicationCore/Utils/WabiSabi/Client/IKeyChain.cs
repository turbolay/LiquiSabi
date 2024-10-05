using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Keys;
using LiquiSabi.ApplicationCore.Utils.Crypto;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client;

public interface IKeyChain
{
	OwnershipProof GetOwnershipProof(IDestination destination, CoinJoinInputCommitmentData committedData);

	Transaction Sign(Transaction transaction, Coin coin, PrecomputedTransactionData precomputeTransactionData);

	void TrySetScriptStates(KeyState state, IEnumerable<Script> scripts);
}
