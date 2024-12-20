using System.Text.Json;
using Polly;

namespace OpenMeteo;

public interface IOpenMeteoAction<R>
    where R : class
{
    public HttpRequestMessage ToRequest(Uri baseUri);

    public Task<R> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    );
}

public interface IOpenMeteoApi
{
    public Task<R> ExecuteAsync<R>(IOpenMeteoAction<R> action, CancellationToken cancellationToken)
        where R : class;
}

public class OpenMeteoApi : IOpenMeteoApi
{
    private readonly Uri _baseUri;

    private readonly HttpClient _client;

    public OpenMeteoApi(Uri baseUri, HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        ArgumentNullException.ThrowIfNull(client);
        _baseUri = baseUri;
        _client = client;
    }

    public async Task<R> ExecuteAsync<R>(IOpenMeteoAction<R> action)
        where R : class => await ExecuteAsync(action, CancellationToken.None);

    public async Task<R> ExecuteAsync<R>(
        IOpenMeteoAction<R> action,
        CancellationToken cancellationToken
    )
        where R : class
    {
        ArgumentNullException.ThrowIfNull(action);

        var request = action.ToRequest(_baseUri);

        using var response = await _client.SendAsync(request, cancellationToken);

        return await action.FromResponseAsync(response, cancellationToken);
    }
}

public class RecordingOpenMeteoApi : IOpenMeteoApi
{
    private readonly IOpenMeteoApi _next;

    public List<IOpenMeteoAction<object>> Actions { get; } = [];

    public RecordingOpenMeteoApi(IOpenMeteoApi next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public async Task<R> ExecuteAsync<R>(
        IOpenMeteoAction<R> action,
        CancellationToken cancellationToken
    )
        where R : class
    {
        Actions.Add((IOpenMeteoAction<object>)action);
        return await _next.ExecuteAsync(action, cancellationToken);
    }
}

public class ResilientOpenMeteoApi(ResiliencePipeline pipeline, IOpenMeteoApi api) : IOpenMeteoApi
{
    private readonly IOpenMeteoApi api = api ?? throw new ArgumentNullException(nameof(api));

    public async Task<R> ExecuteAsync<R>(
        IOpenMeteoAction<R> action,
        CancellationToken cancellationToken
    )
        where R : class
    {
        ArgumentNullException.ThrowIfNull(action);

        return await pipeline.ExecuteAsync(
            async state => await api.ExecuteAsync(action, cancellationToken),
            cancellationToken
        );
    }
}

public record ForecastResponse(double Latitude, double Longitude);

public class GetForecast(double latitude, double longitude) : IOpenMeteoAction<ForecastResponse>
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        RespectNullableAnnotations = true,
    };

    public HttpRequestMessage ToRequest(Uri baseUri)
    {
        var builder = new UriBuilder(baseUri)
        {
            Path = "/v1/forecast",
            Query = $"latitude={latitude}&longitude={longitude}",
        };

        var uri = builder.Uri;

        return new(HttpMethod.Get, uri) { Headers = { { "Accept", "application/json" } } };
    }

    public async Task<ForecastResponse> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(response);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result =
            JsonSerializer.Deserialize<ForecastResponse>(content, _options)
            ?? throw new InvalidOperationException("Failed to deserialize /delete response");

        return result;
    }
}
