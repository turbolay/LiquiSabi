using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Keys;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client;

public class InternalDestinationProvider : IDestinationProvider
{
	public InternalDestinationProvider(KeyManager keyManager)
	{
		KeyManager = keyManager;
	}

	private KeyManager KeyManager { get; }

	public IEnumerable<IDestination> GetNextDestinations(int count, bool preferTaproot)
	{
		// Get all locked internal keys we have and assert we have enough.
		KeyManager.AssertLockedInternalKeysIndexedAndPersist(count, preferTaproot);

		var allKeys = KeyManager.GetNextCoinJoinKeys().ToList();
		var taprootKeys = allKeys
			.Where(x => x.FullKeyPath.GetScriptTypeFromKeyPath() == ScriptPubKeyType.TaprootBIP86)
			.ToList();

		var segwitKeys = allKeys
			.Where(x => x.FullKeyPath.GetScriptTypeFromKeyPath() == ScriptPubKeyType.Segwit)
			.ToList();

		var destinations = preferTaproot && taprootKeys.Count >= count
			? taprootKeys
			: segwitKeys;
		return destinations.Select(x => x.GetAddress(KeyManager.GetNetwork()));
	}
}
