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

    private readonly Func<string> _requestId;
    
    private readonly string _headerName;

    public LoggingHttpHandler(IEvents events) : this(events, () => string.Empty) { }

    public LoggingHttpHandler(IEvents events, Func<string> requestId, string headerName = "X-Request-ID")
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(requestId);
        ArgumentNullException.ThrowIfNull(headerName);
        _events = events;
        _requestId = requestId;
        _headerName = headerName;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.TryAddWithoutValidation(_headerName, _requestId());

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
