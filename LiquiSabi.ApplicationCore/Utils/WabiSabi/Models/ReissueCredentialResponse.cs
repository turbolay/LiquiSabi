using WabiSabi.CredentialRequesting;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record ReissueCredentialResponse(
	CredentialsResponse RealAmountCredentials,
	CredentialsResponse RealVsizeCredentials,
	CredentialsResponse ZeroAmountCredentials,
	CredentialsResponse ZeroVsizeCredentials
);
