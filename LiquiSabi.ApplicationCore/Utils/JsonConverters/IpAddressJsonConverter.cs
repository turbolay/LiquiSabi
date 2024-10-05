using System.Net;
using Newtonsoft.Json;

namespace LiquiSabi.ApplicationCore.Utils.JsonConverters;

public class IpAddressJsonConverter : JsonConverter<IPAddress>
{
    public override void WriteJson(JsonWriter writer, IPAddress? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            throw new NotSupportedException($"{nameof(EndPointJsonConverter)} can only convert {nameof(EndPoint)}.");
        }
        else
        {
            var ipAddressString = value.ToString();
            writer.WriteValue(ipAddressString);
        }
    }

    public override IPAddress? ReadJson(JsonReader reader, Type objectType, IPAddress? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var addressString = reader.Value as string;
        if (IPAddress.TryParse(addressString, out IPAddress? address))
        {
            return address;
        }
        else
        {
            throw new FormatException($"{nameof(addressString)} is in the wrong format: {addressString}.");
        }
    }
}