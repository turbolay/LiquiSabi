using System.Text;
using Newtonsoft.Json;
using LiquiSabi.ApplicationCore.Utils.Affiliation.Serialization;

namespace LiquiSabi.ApplicationCore.Utils.Affiliation.Models.CoinjoinNotification;

public record Payload(Header Header, Body Body)
{
	public byte[] GetCanonicalSerialization() =>
		Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this, CanonicalJsonSerializationOptions.Settings));
}
