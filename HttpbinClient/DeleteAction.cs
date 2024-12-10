using System.Text.Json;

namespace HttpbinClient;

public class DeleteAction : IHttpbinAction<DeleteActionResponse>
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        RespectNullableAnnotations = true,
    };

    public HttpRequestMessage ToRequest(Uri baseUri) =>
        new(HttpMethod.Delete, new Uri(baseUri, "/delete"))
        {
            Headers = { { "Accept", "application/json" } },
        };

    public async Task<DeleteActionResponse> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(response);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result =
            JsonSerializer.Deserialize<DeleteActionResponse>(content, _options)
            ?? throw new InvalidOperationException("Failed to deserialize /delete response");

        return result;
    }
}

public record DeleteActionResponse(
    Dictionary<string, object> Args,
    Dictionary<string, string> Headers,
    string Data,
    string Origin,
    string Url
) { }
