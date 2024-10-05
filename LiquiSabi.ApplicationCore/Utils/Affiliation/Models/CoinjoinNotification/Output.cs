using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.Affiliation.Models.CoinjoinNotification;

public record Output(long Amount, byte[] ScriptPubkey)
{
	public static Output FromTxOut(TxOut txOut) =>
		new(txOut.Value, txOut.ScriptPubKey.ToBytes());
}
