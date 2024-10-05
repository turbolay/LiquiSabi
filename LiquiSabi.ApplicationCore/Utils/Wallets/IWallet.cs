using LiquiSabi.ApplicationCore.Utils.Blockchain.TransactionOutputs;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Transactions;
using LiquiSabi.ApplicationCore.Utils.Models;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Client;

namespace LiquiSabi.ApplicationCore.Utils.Wallets;

public interface IWallet
{
	string WalletName { get; }
	bool IsUnderPlebStop { get; }
	bool IsMixable { get; }

	/// <summary>
	/// Watch only wallets have no key chains.
	/// </summary>
	IKeyChain? KeyChain { get; }

	IDestinationProvider DestinationProvider { get; }
	int AnonScoreTarget { get; }
	bool ConsolidationMode { get; }
	TimeSpan FeeRateMedianTimeFrame { get; }
	bool RedCoinIsolation { get; }
	CoinjoinSkipFactors CoinjoinSkipFactors { get; }

	Task<bool> IsWalletPrivateAsync();

	Task<IEnumerable<SmartCoin>> GetCoinjoinCoinCandidatesAsync();

	Task<IEnumerable<SmartTransaction>> GetTransactionsAsync();
}
