using Microsoft.Extensions.Hosting;
using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Affiliation;
using LiquiSabi.ApplicationCore.Utils.BitcoinCore.Rpc;
using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.Services;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Banning;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.DoSPrevention;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds.CoinJoinStorage;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Statistics;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi;

public class WabiSabiCoordinator : BackgroundService
{
	public WabiSabiCoordinator(CoordinatorParameters parameters, IRPCClient rpc, ICoinJoinIdStore coinJoinIdStore, CoinJoinScriptStore coinJoinScriptStore, IHttpClientFactory httpClientFactory, CoinVerifier? coinVerifier = null)
	{
		Parameters = parameters;

		Warden = new(parameters.PrisonFilePath, coinJoinIdStore, Config);
		ConfigWatcher = new(parameters.ConfigChangeMonitoringPeriod, Config, () => Logger.LogInfo("WabiSabi configuration has changed."));
		CoinJoinIdStore = coinJoinIdStore;
		CoinVerifier = coinVerifier;
		CoinJoinTransactionArchiver transactionArchiver = new(Path.Combine(parameters.CoordinatorDataDir, "CoinJoinTransactions"));

		CoinJoinFeeRateStatStore = CoinJoinFeeRateStatStore.LoadFromFile(parameters.CoinJoinFeeRateStatStoreFilePath, Config, rpc);
		IoHelpers.EnsureContainingDirectoryExists(Parameters.CoinJoinFeeRateStatStoreFilePath);
		CoinJoinFeeRateStatStore.NewStat += FeeRateStatStore_NewStat;

		IoHelpers.EnsureContainingDirectoryExists(Parameters.CoinJoinScriptStoreFilePath);

		RoundParameterFactory roundParameterFactory = new(Config, rpc.Network);
		Arena = new(
			parameters.RoundProgressSteppingPeriod,
			Config,
			rpc,
			Warden.Prison,
			coinJoinIdStore,
			roundParameterFactory,
			transactionArchiver,
			coinJoinScriptStore,
			coinVerifier);
		AffiliationManager = new(Arena, Config, httpClientFactory);

		IoHelpers.EnsureContainingDirectoryExists(Parameters.CoinJoinIdStoreFilePath);
		Arena.CoinJoinBroadcast += Arena_CoinJoinBroadcast;
	}

	public ConfigWatcher ConfigWatcher { get; }
	public ICoinJoinIdStore CoinJoinIdStore { get; private set; }
	public CoinVerifier? CoinVerifier { get; private set; }
	public Warden Warden { get; }

	public CoordinatorParameters Parameters { get; }
	public Arena Arena { get; }

	public CoinJoinFeeRateStatStore CoinJoinFeeRateStatStore { get; }

	public Backend.WabiSabiConfig Config => Parameters.RuntimeCoordinatorConfig;
	public DateTimeOffset LastSuccessfulCoinJoinTime { get; private set; } = DateTimeOffset.UtcNow;

	public AffiliationManager AffiliationManager { get; }

	private void Arena_CoinJoinBroadcast(object? sender, Transaction transaction)
	{
		LastSuccessfulCoinJoinTime = DateTimeOffset.UtcNow;

		CoinJoinIdStore.TryAdd(transaction.GetHash());

		var coinJoinScriptStoreFilePath = Parameters.CoinJoinScriptStoreFilePath;
		try
		{
			File.AppendAllLines(coinJoinScriptStoreFilePath, transaction.Outputs.Select(x => x.ScriptPubKey.ToHex()));
		}
		catch (Exception ex)
		{
			Logger.LogError($"Could not write file {coinJoinScriptStoreFilePath}.", ex);
		}
	}

	private void FeeRateStatStore_NewStat(object? sender, CoinJoinFeeRateStat feeRateStat)
	{
		var filePath = Parameters.CoinJoinFeeRateStatStoreFilePath;
		try
		{
			File.AppendAllLines(filePath, new[] { feeRateStat.ToLine() });
		}
		catch (Exception ex)
		{
			Logger.LogError($"Could not write file {filePath}.", ex);
		}
	}

	public void BanDescendant(object? sender, Block block)
	{
		var now = DateTimeOffset.UtcNow;

		bool IsInputBanned(TxIn input) => Warden.Prison.IsBanned(input.PrevOut, now);
		OutPoint[] BannedInputs(Transaction tx) => tx.Inputs.Where(IsInputBanned).Select(x => x.PrevOut).ToArray();

		var outpointsToBan = block.Transactions
			.Select(tx => (Tx: tx, BannedInputs: BannedInputs(tx)))
			.Where(x => x.BannedInputs.Any())
			.SelectMany(x => x.Tx.Outputs.Select((_, i) => (new OutPoint(x.Tx, i), x.BannedInputs)));

		foreach (var (outpoint, ancestors) in outpointsToBan)
		{
			Warden.Prison.InheritPunishment(outpoint, ancestors);
		}
	}

	public void BanDoubleSpenders(object? sender, Transaction tx)
	{
		var outPoints = tx.Inputs.Select(x => x.PrevOut);

		// Detect and punish double spending coins
		var disrupters = Arena.RoundStates
			.Where(r => r.Phase != Phase.Ended)
			.SelectMany(r => r.CoinjoinState.Inputs.Select(a => (RoundId: r.Id, Coin: a)))
			.Where(x => outPoints.Any(outpoint => outpoint == x.Coin.Outpoint))
			.ToArray();

		foreach (var (roundId, offender) in disrupters)
		{
			Warden.Prison.DoubleSpent(offender.Outpoint, offender.Amount, roundId);
		}

		// Abort disrupted rounds
		var disruptedRounds = disrupters.Select(x => x.RoundId).Distinct();
		foreach (var roundId in disruptedRounds)
		{
			Arena.AbortRound(roundId);
		}
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await ConfigWatcher.StartAsync(stoppingToken).ConfigureAwait(false);
		await Warden.StartAsync(stoppingToken).ConfigureAwait(false);
		await Arena.StartAsync(stoppingToken).ConfigureAwait(false);

		await CoinJoinFeeRateStatStore.StartAsync(stoppingToken).ConfigureAwait(false);
		await AffiliationManager.StartAsync(stoppingToken).ConfigureAwait(false);
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		await base.StopAsync(cancellationToken).ConfigureAwait(false);

		await Arena.StopAsync(cancellationToken).ConfigureAwait(false);
		await ConfigWatcher.StopAsync(cancellationToken).ConfigureAwait(false);
		await Warden.StopAsync(cancellationToken).ConfigureAwait(false);

		await CoinJoinFeeRateStatStore.StopAsync(cancellationToken).ConfigureAwait(false);
		await AffiliationManager.StopAsync(cancellationToken).ConfigureAwait(false);
	}

	public override void Dispose()
	{
		CoinJoinFeeRateStatStore.NewStat -= FeeRateStatStore_NewStat;
		Arena.CoinJoinBroadcast -= Arena_CoinJoinBroadcast;
		ConfigWatcher.Dispose();
		Warden.Dispose();
		AffiliationManager.Dispose();
		base.Dispose();
	}
}
