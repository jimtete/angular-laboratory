using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearningLab.Data.Serialization;

public sealed class StringOrStringArrayJsonConverter : JsonConverter<string?>
{
    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.StartArray => ReadStringArray(ref reader),
            _ => throw new JsonException("Expected a string or an array of strings.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        string? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }

    private static string? ReadStringArray(ref Utf8JsonReader reader)
    {
        var values = new List<string>();

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
            {
                return values.Count == 0
                    ? null
                    : string.Join('\n', values);
            }

            if (reader.TokenType is not JsonTokenType.String)
            {
                throw new JsonException("Expected every array item to be a string.");
            }

            var value = reader.GetString();

            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value.Trim());
            }
        }

        throw new JsonException("Expected the rewards array to end.");
    }
}
