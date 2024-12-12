using System.Text.Json;
using FluentAssertions;
using ValueOf.Json;

namespace ValueOf.Tests;

public class ValueOfConverterTest
{
    class Description(string value) : NonBlankString<Description>(value) { }

    record Foo(Description Description);

    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
        Converters = { new ValueOfConverter<string, Description>() },
    };

    [Fact]
    public void CanConvertValueOf()
    {
        var foo = new Foo(new Description("test"));

        var json = JsonSerializer.Serialize(foo, options);

        json.Should().Be("{\"Description\":\"test\"}");

        var deserialized = JsonSerializer.Deserialize<Foo>(json, options);

        deserialized.Should().Be(foo);
    }
}
