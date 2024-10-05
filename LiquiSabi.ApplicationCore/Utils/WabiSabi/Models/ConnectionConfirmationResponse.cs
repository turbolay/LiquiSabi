using WabiSabi.CredentialRequesting;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record ConnectionConfirmationResponse(
	CredentialsResponse ZeroAmountCredentials,
	CredentialsResponse ZeroVsizeCredentials,
	CredentialsResponse? RealAmountCredentials = null,
	CredentialsResponse? RealVsizeCredentials = null
);
