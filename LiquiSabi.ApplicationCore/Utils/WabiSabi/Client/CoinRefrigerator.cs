using LiquiSabi.ApplicationCore.Utils.Blockchain.TransactionOutputs;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client;

public class CoinRefrigerator
{
	private Dictionary<SmartCoin, DateTimeOffset> FrozenCoins { get; } = new();
	private TimeSpan FreezeTime { get; } = TimeSpan.FromSeconds(90);

	public void Freeze(IEnumerable<SmartCoin> coins)
	{
		foreach (var coin in coins)
		{
			FrozenCoins[coin] = DateTimeOffset.UtcNow;
		}
	}

	public bool IsFrozen(SmartCoin coin)
	{
		if (!FrozenCoins.TryGetValue(coin, out var startTime))
		{
			return false;
		}

		if (startTime.Add(FreezeTime) > DateTimeOffset.UtcNow)
		{
			return true;
		}

		FrozenCoins.Remove(coin);
		return false;
	}
}
