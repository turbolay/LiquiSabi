using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.Affiliation.Models.CoinjoinNotification;

public record Outpoint(byte[] Hash, long Index)
{
	public static Outpoint FromOutPoint(OutPoint outPoint) =>
		new(outPoint.Hash.ToBytes(), outPoint.N);
}
