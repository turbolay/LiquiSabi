using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NBitcoin;
using WabiSabi.CredentialRequesting;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record OutputRegistrationRequest(
	uint256 RoundId,
	[property: ValidateNever] Script Script,
	RealCredentialsRequest AmountCredentialRequests,
	RealCredentialsRequest VsizeCredentialRequests
);
