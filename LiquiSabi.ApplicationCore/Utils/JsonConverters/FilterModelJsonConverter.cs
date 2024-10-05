using Newtonsoft.Json;
using LiquiSabi.ApplicationCore.Utils.Backend.Models;
using LiquiSabi.ApplicationCore.Utils.Helpers;

namespace LiquiSabi.ApplicationCore.Utils.JsonConverters;

public class FilterModelJsonConverter : JsonConverter<FilterModel>
{
	/// <inheritdoc />
	public override FilterModel? ReadJson(JsonReader reader, Type objectType, FilterModel? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		var value = Guard.Correct((string?)reader.Value);

		return string.IsNullOrWhiteSpace(value) ? default : FilterModel.FromLine(value);
	}

	/// <inheritdoc />
	public override void WriteJson(JsonWriter writer, FilterModel? value, JsonSerializer serializer)
	{
		var filterModel = value?.ToLine() ?? throw new ArgumentNullException(nameof(value));
		writer.WriteValue(filterModel);
	}
}
