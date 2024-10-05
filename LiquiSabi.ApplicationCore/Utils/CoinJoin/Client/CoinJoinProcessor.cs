using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Backend.Models.Responses;
using LiquiSabi.ApplicationCore.Utils.BitcoinCore.Rpc;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Transactions;
using LiquiSabi.ApplicationCore.Utils.Extensions;
using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.Models;
using LiquiSabi.ApplicationCore.Utils.Nito.AsyncEx;
using LiquiSabi.ApplicationCore.Utils.Services;
using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.CoinJoin.Client;

public class CoinJoinProcessor : IDisposable
{
	private volatile bool _disposedValue = false; // To detect redundant calls

	public CoinJoinProcessor(Network network, WasabiSynchronizer synchronizer, WalletManager walletManager, IRPCClient? rpc)
	{
		Synchronizer = Guard.NotNull(nameof(synchronizer), synchronizer);
		WalletManager = Guard.NotNull(nameof(walletManager), walletManager);
		Network = network;
		RpcClient = rpc;
		ProcessLock = new AsyncLock();
		Synchronizer.ResponseArrived += Synchronizer_ResponseArrivedAsync;
	}

	public WasabiSynchronizer Synchronizer { get; }
	public WalletManager WalletManager { get; }
	public Network Network { get; }
	public IRPCClient? RpcClient { get; private set; }
	private AsyncLock ProcessLock { get; }

	private async void Synchronizer_ResponseArrivedAsync(object? sender, SynchronizeResponse response)
	{
		try
		{
			using (await ProcessLock.LockAsync())
			{
				var unconfirmedCoinJoinHashes = response.UnconfirmedCoinJoins;
				if (!unconfirmedCoinJoinHashes.Any())
				{
					return;
				}

				var txsNotKnownByAWallet = WalletManager.FilterUnknownCoinjoins(unconfirmedCoinJoinHashes);

				var client = Synchronizer.HttpClientFactory.SharedWasabiClient;
				var unconfirmedCoinJoins = await client.GetTransactionsAsync(Network, txsNotKnownByAWallet, CancellationToken.None).ConfigureAwait(false);

				foreach (var tx in unconfirmedCoinJoins.Select(x => new SmartTransaction(x, Height.Mempool)))
				{
					if (RpcClient is null
						|| await TryBroadcastTransactionWithRpcAsync(tx).ConfigureAwait(false)
						|| (await RpcClient.TestAsync().ConfigureAwait(false)) is { }) // If the test throws exception then I believe it, because RPC is down and the backend is the god.
					{
						WalletManager.ProcessCoinJoin(tx);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
		}
	}

	private async Task<bool> TryBroadcastTransactionWithRpcAsync(SmartTransaction transaction)
	{
		try
		{
			if (RpcClient is null)
			{
				throw new InvalidOperationException("RpcClient is not available");
			}
			await RpcClient.SendRawTransactionAsync(transaction.Transaction).ConfigureAwait(false);
			Logger.LogInfo($"Transaction is successfully broadcasted with RPC: {transaction.GetHash()}.");

			return true;
		}
		catch
		{
			return false;
		}
	}

	#region IDisposable Support

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Synchronizer.ResponseArrived -= Synchronizer_ResponseArrivedAsync;
			}

			_disposedValue = true;
		}
	}

	// This code added to correctly implement the disposable pattern.
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		Dispose(true);
	}

	#endregion IDisposable Support
}
