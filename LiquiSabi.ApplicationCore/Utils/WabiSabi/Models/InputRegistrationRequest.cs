using NBitcoin;
using WabiSabi.CredentialRequesting;
using LiquiSabi.ApplicationCore.Utils.Crypto;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record InputRegistrationRequest(
	uint256 RoundId,
	OutPoint Input,
	OwnershipProof OwnershipProof,
	ZeroCredentialsRequest ZeroAmountCredentialRequests,
	ZeroCredentialsRequest ZeroVsizeCredentialRequests
);
