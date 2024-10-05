using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.Helpers;

public class CoinEqualityComparer : IEqualityComparer<Coin>
{
	public static readonly CoinEqualityComparer Default = new();

	public bool Equals(Coin? x, Coin? y) => x?.Outpoint == y?.Outpoint;

	public int GetHashCode(Coin coin) => coin.Outpoint.GetHashCode();
}
