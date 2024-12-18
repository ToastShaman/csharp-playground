using Events;
using Microsoft.Extensions.Primitives;

namespace Middleware;

public record IncomingHttpRequest(
    string Id,
    string Method,
    string Path,
    Dictionary<string, StringValues> RequestHeaders
) : IEvent;

public record OutgoingHttpResponse(
    string Id,
    int StatusCode,
    string Method,
    string Path,
    Dictionary<string, StringValues> ResponseHeaders
) : IEvent;

public static class Middlewares
{
    public static readonly HttpContextLens<string> RequestIdLens = "RequestId";

    public static Func<HttpContext, RequestDelegate, Task> RequestId(
        string headerName = "X-Request-ID"
    ) =>
        async (context, next) =>
        {
            var existingRequestId = context.Request.Headers[headerName].FirstOrDefault();

            var requestId = string.IsNullOrWhiteSpace(existingRequestId)
                ? Ulid.NewUlid().ToString()
                : existingRequestId;

            context.Response.Headers[headerName] = requestId;
            RequestIdLens.Set(context, requestId);
            await next(context);
        };

    public static Func<HttpContext, RequestDelegate, Task> EventMiddleware(IEvents events) =>
        async (context, next) =>
        {
            var id = RequestIdLens.Get(context);

            var request = context.Request;

            var method = request.Method;

            var path = request.Path;

            var requestHeaders = request.Headers.ToDictionary();

            events.Emit(
                new IncomingHttpRequest(
                    Id: id,
                    Method: method,
                    Path: path,
                    RequestHeaders: requestHeaders
                )
            );

            await next(context);

            var response = context.Response;

            var status = response.StatusCode;

            var responseHeaders = response.Headers.ToDictionary();

            events.Emit(
                new OutgoingHttpResponse(
                    Id: id,
                    StatusCode: status,
                    Method: method,
                    Path: path,
                    ResponseHeaders: responseHeaders
                )
            );
        };
}
