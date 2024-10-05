using LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Bases;
using LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Fields.ByteArrayFields;

namespace LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Fields.OctetFields;

public class ULenField : OctetSerializableBase
{
	public ULenField(byte byteValue)
	{
		ByteValue = byteValue;
	}

	public ULenField(UNameField uName)
	{
		ByteValue = (byte)uName.ToBytes().Length;
	}

	public int Value => ByteValue;
}
