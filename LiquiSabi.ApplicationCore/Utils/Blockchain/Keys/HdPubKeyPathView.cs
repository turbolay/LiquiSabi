using System.Collections;
using System.Collections.Immutable;

namespace LiquiSabi.ApplicationCore.Utils.Blockchain.Keys;

public class HdPubKeyPathView : IEnumerable<HdPubKey>
{
	internal HdPubKeyPathView(ImmutableList<HdPubKey> hdPubKeys)
	{
		Keys = hdPubKeys;
	}

	protected ImmutableList<HdPubKey> Keys { get; }
	public IEnumerable<HdPubKey> CleanKeys => GetKeysByState(KeyState.Clean);
	public IEnumerable<HdPubKey> LockedKeys => GetKeysByState(KeyState.Locked);
	public IEnumerable<HdPubKey> UsedKeys => GetKeysByState(KeyState.Used);
	public IEnumerable<HdPubKey> UnusedKeys => Keys.Except(UsedKeys);

	private IEnumerable<HdPubKey> GetKeysByState(KeyState keyState) =>
		Keys.Where(x => x.KeyState == keyState);

	public IEnumerator<HdPubKey> GetEnumerator() =>
		Keys.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() =>
		GetEnumerator();
}
