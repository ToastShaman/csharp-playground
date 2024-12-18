using CSharpFunctionalExtensions;

namespace Middleware;

public class HttpContextLens<T>
    where T : notnull
{
    private readonly Func<HttpContext, Maybe<T>> _get;

    private readonly Action<HttpContext, T> _set;

    public HttpContextLens(Func<HttpContext, Maybe<T>> get, Action<HttpContext, T> set)
    {
        ArgumentNullException.ThrowIfNull(get);
        ArgumentNullException.ThrowIfNull(set);
        _get = get;
        _set = set;
    }

    public HttpContextLens(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _get = context => context.Items[key] is T value ? Maybe<T>.From(value) : Maybe<T>.None;

        _set = (context, value) => context.Items[key] = value;
    }

    public T Get(HttpContext context) => _get(context).GetValueOrThrow();

    public Maybe<T> Maybe(HttpContext context) => _get(context);

    public void Set(HttpContext context, T value) => _set(context, value);

    public static implicit operator HttpContextLens<T>(string key) => new(key);
}
