namespace HttpbinClient;

public class HttpbinApi(Uri baseUri, HttpClient client)
{
    private readonly Uri _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
    
    private readonly HttpClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public async Task<R> ExecuteAsync<R>(IHttpbinAction<R> action) where R : class => await ExecuteAsync(action, CancellationToken.None);

    public async Task<R> ExecuteAsync<R>(IHttpbinAction<R> action, CancellationToken cancellationToken) where R : class
    {
        ArgumentNullException.ThrowIfNull(action);

        var request = action.ToRequest(_baseUri);

        using var response = await _client.SendAsync(request, cancellationToken);

        return await action.FromResponseAsync(response, cancellationToken);
    }
}