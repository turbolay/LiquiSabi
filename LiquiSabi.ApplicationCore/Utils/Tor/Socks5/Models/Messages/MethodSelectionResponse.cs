using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Bases;
using LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Fields.OctetFields;

namespace LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Models.Messages;

public class MethodSelectionResponse : ByteArraySerializableBase
{
	/// <param name="bytes">2 bytes are required to be passed in.</param>
	public MethodSelectionResponse(byte[] bytes)
	{
		Guard.Same($"{nameof(bytes)}.{nameof(bytes.Length)}", 2, bytes.Length);

		Ver = new VerField(bytes[0]);
		Method = new MethodField(bytes[1]);
	}

	public VerField Ver { get; }

	public MethodField Method { get; }

	public override byte[] ToBytes()
		=> new byte[]
		{
				Ver.ToByte(),
				Method.ToByte()
		};
}
