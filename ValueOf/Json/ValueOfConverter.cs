using System.Text.Json;
using System.Text.Json.Serialization;

namespace ValueOf.Json;

public class ValueOfConverter<T, TDerived> : JsonConverter<TDerived>
    where T : notnull
    where TDerived : ValueOf<T, TDerived>
{
    public override TDerived Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        // Deserialize the underlying value (T) from JSON
        T value =
            JsonSerializer.Deserialize<T>(ref reader, options)
            ?? throw new JsonException("Failed to deserialize value");

        // Create an instance of TDerived using the factory method
        return ValueOf<T, TDerived>.Create(value);
    }

    public override void Write(Utf8JsonWriter writer, TDerived value, JsonSerializerOptions options)
    {
        // Serialize the underlying value (_value) to JSON
        JsonSerializer.Serialize(writer, value.Unwrap(), options);
    }
}
