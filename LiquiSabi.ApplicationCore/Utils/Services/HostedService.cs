using Microsoft.Extensions.Hosting;
using LiquiSabi.ApplicationCore.Utils.Helpers;

namespace LiquiSabi.ApplicationCore.Utils.Services;

public class HostedService
{
	public HostedService(IHostedService service, string friendlyName)
	{
		Service = Guard.NotNull(nameof(service), service);
		FriendlyName = Guard.NotNull(nameof(friendlyName), friendlyName);
	}

	public IHostedService Service { get; }
	public string FriendlyName { get; }
}
