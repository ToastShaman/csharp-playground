namespace HttpbinClient;

public interface IHttpbinApi
{
    public Task<R> ExecuteAsync<R>(IHttpbinAction<R> action, CancellationToken cancellationToken)
        where R : class;
}

public static class IHttpbinApiExtensions
{
    public static async Task<R> ExecuteAsync<R>(this IHttpbinApi api, IHttpbinAction<R> action)
        where R : class => await api.ExecuteAsync(action, CancellationToken.None);
}
