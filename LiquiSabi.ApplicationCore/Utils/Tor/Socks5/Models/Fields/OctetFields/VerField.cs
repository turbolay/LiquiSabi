using LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Bases;

namespace LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Fields.OctetFields;

public class VerField : OctetSerializableBase
{
	public static readonly VerField Socks5 = new(5);

	public VerField(byte value)
	{
		ByteValue = value;
	}

	public byte Value => ByteValue;
}
