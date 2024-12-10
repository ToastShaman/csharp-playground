namespace HttpbinClient.Tests;

public interface IFakeActionHandler
{
    public R? Handle<R>(IHttpbinAction<R> action)
        where R : class;
}

public static class IFakeActionHandlerExtensions
{
    public static IFakeActionHandler Or(this IFakeActionHandler first, IFakeActionHandler second) =>
        new CompositeFakeActionHandler(first, second);
}

public class CompositeFakeActionHandler(IFakeActionHandler first, IFakeActionHandler second)
    : IFakeActionHandler
{
    private readonly IFakeActionHandler _first =
        first ?? throw new ArgumentNullException(nameof(first));

    private readonly IFakeActionHandler _second =
        second ?? throw new ArgumentNullException(nameof(second));

    public R? Handle<R>(IHttpbinAction<R> action)
        where R : class => _first.Handle(action) ?? _second.Handle(action);
}

public class FakeActionHandler(object response) : IFakeActionHandler
{
    private readonly object _response =
        response ?? throw new ArgumentNullException(nameof(response));

    public R? Handle<R>(IHttpbinAction<R> action)
        where R : class => _response is R cast ? cast : null;

    public static IFakeActionHandler FromResult<R>(R response)
        where R : class => new FakeActionHandler(response);
}

public class FakeHttpbinApi(IFakeActionHandler handler) : IHttpbinApi
{
    private readonly IFakeActionHandler _handler =
        handler ?? throw new ArgumentNullException(nameof(handler));

    public Task<R> ExecuteAsync<R>(IHttpbinAction<R> action, CancellationToken cancellationToken)
        where R : class
    {
        ArgumentNullException.ThrowIfNull(action);

        return Task.FromResult(_handler.Handle(action) ?? throw new InvalidOperationException());
    }
}
