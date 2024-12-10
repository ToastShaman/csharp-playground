using System.Text.Json;

namespace HttpbinClient;

public class GetAction : IHttpbinAction<GetActionResponse>
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        RespectNullableAnnotations = true
    };

    public HttpRequestMessage ToRequest(Uri baseUri) => new(HttpMethod.Get, new Uri(baseUri, "/get"))
    {
        Headers = { { "Accept", "application/json" } }
    };

    public async Task<GetActionResponse> FromResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<GetActionResponse>(content, _options)
            ?? throw new InvalidOperationException("Failed to deserialize /get response");

        return result;
    }
}

public record GetActionResponse(
     Dictionary<string, object> Args,
     Dictionary<string, string> Headers,
     string Origin,
     string Url)
{

}