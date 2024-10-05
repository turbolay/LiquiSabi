using LiquiSabi.ApplicationCore.Utils.BitcoinCore.Rpc.Models;

namespace LiquiSabi.ApplicationCore.Utils.Blockchain.BlockFilters;

public static class IndexTypeConverter
{
	public static RpcPubkeyType[] ToRpcPubKeyTypes(IndexType indexType)
		=> indexType switch
		{
			IndexType.SegwitTaproot => new[] { RpcPubkeyType.TxWitnessV0Keyhash, RpcPubkeyType.TxWitnessV1Taproot },
			IndexType.Taproot => new[] { RpcPubkeyType.TxWitnessV1Taproot },
			_ => throw new NotSupportedException("Index type not supported."),
		};
}
