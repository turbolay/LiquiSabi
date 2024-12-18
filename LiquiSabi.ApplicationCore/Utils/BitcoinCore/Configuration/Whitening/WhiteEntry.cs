using System.Diagnostics.CodeAnalysis;
using System.Net;
using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Userfacing;

namespace LiquiSabi.ApplicationCore.Utils.BitcoinCore.Configuration.Whitening;

public abstract class WhiteEntry
{
	public string Permissions { get; private set; } = "";
	public EndPoint? EndPoint { get; private set; } = null;

	public static bool TryParse<T>(string value, Network network, [NotNullWhen(true)] out T? whiteEntry) where T : WhiteEntry, new()
	{
		whiteEntry = null;

		// https://github.com/bitcoin/bitcoin/pull/16248
		var parts = value?.Split('@');
		if (parts is { })
		{
			if (EndPointParser.TryParse(parts.LastOrDefault(), network.DefaultPort, out EndPoint? endPoint))
			{
				whiteEntry = new T
				{
					EndPoint = endPoint
				};
				if (parts.Length > 1)
				{
					whiteEntry.Permissions = parts.First();
				}

				return true;
			}
		}

		return false;
	}
}
