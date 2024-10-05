using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record InputsRemovalRequest(
	uint256 RoundId,
	Guid AliceId
);
