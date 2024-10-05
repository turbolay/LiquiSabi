using System.Collections.Immutable;
using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Blockchain.TransactionOutputs;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client;

public abstract record CoinJoinResult;

public record SuccessfulCoinJoinResult(
	ImmutableList<SmartCoin> Coins,
	ImmutableList<Script> OutputScripts,
	Transaction UnsignedCoinJoin) : CoinJoinResult;

public record FailedCoinJoinResult : CoinJoinResult;

public record DisruptedCoinJoinResult(ImmutableList<SmartCoin> SignedCoins) : CoinJoinResult;
