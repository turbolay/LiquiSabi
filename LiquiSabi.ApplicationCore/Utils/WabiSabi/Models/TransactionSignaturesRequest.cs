using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record TransactionSignaturesRequest(uint256 RoundId, uint InputIndex, WitScript Witness);
