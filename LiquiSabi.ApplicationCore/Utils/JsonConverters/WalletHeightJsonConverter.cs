using Newtonsoft.Json;
using LiquiSabi.ApplicationCore.Utils.Models;

namespace LiquiSabi.ApplicationCore.Utils.JsonConverters;

public class WalletHeightJsonConverter : HeightJsonConverter
{
	/// <inheritdoc />
	public override void WriteJson(JsonWriter writer, Height? height, JsonSerializer serializer)
	{
		if (height is null)
		{
			writer.WriteNull();
		}
		else
		{
			var safeHeight = Math.Max(0, height.Value.Value - 101 /* maturity */);

			writer.WriteValue(safeHeight.ToString());
		}
	}
}
