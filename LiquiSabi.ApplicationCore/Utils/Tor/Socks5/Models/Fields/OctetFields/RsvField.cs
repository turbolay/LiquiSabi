using LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Bases;

namespace LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Fields.OctetFields;

public class RsvField : OctetSerializableBase
{
	public static readonly RsvField X00 = new(0x00);

	public RsvField(byte byteValue)
	{
		ByteValue = byteValue;
	}
}
