using Optional;
using Optional.Unsafe;

namespace Middleware;

public class HttpContextLens<T>
    where T : notnull
{
    private readonly Func<HttpContext, Option<T>> _get;

    private readonly Action<HttpContext, T> _set;

    public HttpContextLens(Func<HttpContext, Option<T>> get, Action<HttpContext, T> set)
    {
        ArgumentNullException.ThrowIfNull(get);
        ArgumentNullException.ThrowIfNull(set);
        _get = get;
        _set = set;
    }

    public HttpContextLens(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _get = context => context.Items[key] is T value ? Option.Some(value) : Option.None<T>();

        _set = (context, value) => context.Items[key] = value;
    }

    public T Get(HttpContext context) => _get(context).ValueOrFailure("Value not found");

    public Option<T> Maybe(HttpContext context) => _get(context);

    public void Set(HttpContext context, T value) => _set(context, value);

    public static implicit operator HttpContextLens<T>(string key) => new(key);
}
