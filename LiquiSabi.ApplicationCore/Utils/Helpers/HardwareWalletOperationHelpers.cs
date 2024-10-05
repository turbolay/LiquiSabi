using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Keys;
using LiquiSabi.ApplicationCore.Utils.Hwi;
using LiquiSabi.ApplicationCore.Utils.Hwi.Models;
using LiquiSabi.ApplicationCore.Utils.Logging;

namespace LiquiSabi.ApplicationCore.Utils.Helpers;

public static class HardwareWalletOperationHelpers
{
	public static async Task<KeyManager> GenerateWalletAsync(HwiEnumerateEntry device, string walletFilePath, Network network, CancellationToken cancelToken)
	{
		if (device.Fingerprint is null)
		{
			throw new Exception("Fingerprint cannot be null.");
		}

		var client = new HwiClient(network);
		var fingerPrint = (HDFingerprint)device.Fingerprint;

		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
		using var genCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancelToken);

		var segwitExtPubKey = await client.GetXpubAsync(
			device.Model,
			device.Path,
			KeyManager.GetAccountKeyPath(network, ScriptPubKeyType.Segwit),
			genCts.Token).ConfigureAwait(false);

		var taprootExtPubKey = await client.GetXpubAsync(
			device.Model,
			device.Path,
			KeyManager.GetAccountKeyPath(network, ScriptPubKeyType.TaprootBIP86),
			genCts.Token).ConfigureAwait(false);

		return KeyManager.CreateNewHardwareWalletWatchOnly(fingerPrint, segwitExtPubKey, taprootExtPubKey, network, walletFilePath);
	}

	public static async Task InitHardwareWalletAsync(HwiEnumerateEntry device, Network network, CancellationToken cancelToken)
	{
		var client = new HwiClient(network);
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(21));
		using var initCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancelToken);

		// Trezor T doesn't require interactive mode.
		var interactiveMode = !(device.Model == HardwareWalletModels.Trezor_T || device.Model == HardwareWalletModels.Trezor_T_Simulator);

		try
		{
			await client.SetupAsync(device.Model, device.Path, interactiveMode, initCts.Token).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
		}
	}

	public static async Task<HwiEnumerateEntry[]> DetectAsync(Network network, CancellationToken cancelToken)
	{
		var client = new HwiClient(network);
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
		using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancelToken);

		var detectedHardwareWallets = (await client.EnumerateAsync(timeoutCts.Token).ConfigureAwait(false)).ToArray();

		cancelToken.ThrowIfCancellationRequested();

		return detectedHardwareWallets;
	}
}
