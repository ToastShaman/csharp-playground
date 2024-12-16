using Polly;
using Polly.Retry;

namespace HttpbinClient;

public class HttpbinApi(Uri baseUri, HttpClient client) : IHttpbinApi
{
    private readonly Uri _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));

    private readonly HttpClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public async Task<R> ExecuteAsync<R>(IHttpbinAction<R> action)
        where R : class => await ExecuteAsync(action, CancellationToken.None);

    public async Task<R> ExecuteAsync<R>(
        IHttpbinAction<R> action,
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

public class ResilientHttpbinApi(ResiliencePipeline pipeline, IHttpbinApi api) : IHttpbinApi
{
    private readonly IHttpbinApi api = api ?? throw new ArgumentNullException(nameof(api));

    public async Task<R> ExecuteAsync<R>(
        IHttpbinAction<R> action,
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

    public static Func<IHttpbinApi, IHttpbinApi> Create(
        RetryStrategyOptions options,
        TimeSpan timeout
    )
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(options)
            .AddTimeout(timeout)
            .Build();

        return next => new ResilientHttpbinApi(pipeline, next);
    }
}
