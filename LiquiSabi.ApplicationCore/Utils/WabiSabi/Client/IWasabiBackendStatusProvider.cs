using LiquiSabi.ApplicationCore.Utils.Backend.Models.Responses;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client;

public interface IWasabiBackendStatusProvider
{
	SynchronizeResponse? LastResponse { get; }
}
