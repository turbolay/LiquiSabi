using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.BitcoinCore.Configuration.Whitening;

public class WhiteBind : WhiteEntry
{
	public static bool TryParse(string value, Network network, [NotNullWhen(true)] out WhiteBind? white)
		=> TryParse<WhiteBind>(value, network, out white);
}
