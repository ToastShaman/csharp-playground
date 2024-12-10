namespace HttpbinClient;

public interface IHttpbinAction<R>
    where R : class
{
    HttpRequestMessage ToRequest(Uri baseUri);

    Task<R> FromResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken);
}
