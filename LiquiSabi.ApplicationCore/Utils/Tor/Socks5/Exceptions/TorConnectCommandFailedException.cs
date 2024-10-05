using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Fields.OctetFields;

namespace LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Exceptions;

/// <summary>
/// Thrown when Tor SOCKS5 responds with an error code to previously sent <see cref="CmdField.Connect"/> command.
/// </summary>
public class TorConnectCommandFailedException : TorConnectionException
{
	public TorConnectCommandFailedException(RepField rep) : base($"Tor SOCKS5 proxy responded with {rep}.")
	{
		RepField = Guard.NotNull(nameof(rep), rep);
	}

	public RepField RepField { get; }
}
