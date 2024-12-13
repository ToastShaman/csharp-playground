namespace HttpbinClient;

public interface IHttpbinAction<R>
    where R : class
{
    public HttpRequestMessage ToRequest(Uri baseUri);

    public Task<R> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    );
}
