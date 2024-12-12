using System.Net;

namespace Events.Http;

public record OutgoingHttpRequest(
    string Method,
    string RequestURI,
    Dictionary<string, IEnumerable<string>> RequestHeaders,
    string? Payload = null
) : IEvent { }

public record IncomingHttpResponse(
    string Method,
    string RequestURI,
    Dictionary<string, IEnumerable<string>> RequestHeaders,
    Dictionary<string, IEnumerable<string>> ResponseHeaders,
    HttpStatusCode StatusCode,
    string? Payload = null,
    string? Content = null
) : IEvent { }

public class LoggingHttpHandler : DelegatingHandler
{
    private readonly IEvents _events;

    public LoggingHttpHandler(IEvents events)
    {
        ArgumentNullException.ThrowIfNull(events);
        _events = events;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", Guid.NewGuid().ToString());

        var requestHeaders = request.Headers.ToDictionary();

        var method = request.Method;

        var uri = request.RequestUri?.ToString() ?? "null";

        var payload =
            request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

        if (payload != null)
        {
            request.Content = new StringContent(
                payload,
                System.Text.Encoding.UTF8,
                request.Content?.Headers.ContentType?.MediaType
            );
        }

        var outgoing = new OutgoingHttpRequest(method.Method, uri, requestHeaders, payload);

        _events.Emit(outgoing);

        var response = await base.SendAsync(request, cancellationToken);

        var responseHeaders = response.Headers.ToDictionary();

        var statusCode = response.StatusCode;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        response.Content = new StringContent(
            content,
            System.Text.Encoding.UTF8,
            response.Content.Headers.ContentType?.MediaType
        );

        var incoming = new IncomingHttpResponse(
            method.Method,
            uri,
            requestHeaders,
            responseHeaders,
            statusCode,
            payload,
            content
        );

        _events.Emit(incoming);

        return response;
    }
}
