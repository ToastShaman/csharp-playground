using Events;
using Events.Http;
using Middleware;
using Polly;
using Polly.Retry;

namespace OpenMeteo;

public static class OpenMeteoClientFactory
{
    public static Func<string> RequestIdFromContext(IServiceProvider provider) =>
        () =>
        {
            var context =
                provider.GetRequiredService<IHttpContextAccessor>().HttpContext
                ?? throw new InvalidOperationException("No HttpContext");

            return Middlewares.RequestIdLens.Get(context);
        };

    public static Func<HttpClient> HttpClientFactory(
        Func<string> requestIdProvider,
        TimeSpan timeout,
        IEvents events
    ) =>
        () =>
        {
            var handler = new LoggingHttpHandler(events, requestIdProvider)
            {
                InnerHandler = new HttpClientHandler(),
            };

            return new HttpClient(handler) { Timeout = timeout };
        };

    public static Func<IOpenMeteoApi> Create(
        Uri baseUrl,
        RetryStrategyOptions options,
        HttpClient client
    ) =>
        () =>
            new ResilientOpenMeteoApi(
                new ResiliencePipelineBuilder().AddRetry(options).Build(),
                new OpenMeteoApi(baseUrl, client)
            );
}
