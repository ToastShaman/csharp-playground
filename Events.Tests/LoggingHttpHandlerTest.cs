using System.Text.Unicode;
using Events.Http;
using FluentAssertions;

namespace Events.Tests;

public class LoggingHttpHandlerTest
{
    [Fact(DisplayName = "LoggingHttpHandler should emit events")]
    public async Task LoggingHttpHandlerShouldEmitEvents()
    {
        var capturing = new CapturingEvents();
        var json = new JsonEvents();

        var events = EventFilters
            .AddServiceName("MyService")
            .Then(EventFilters.AddEnvironment("Production"))
            .Then(EventFilters.AddTimestamp())
            .Then(EventFilters.AddEventName())
            .Then(capturing.And(json));

        var handler = new LoggingHttpHandler(events) { InnerHandler = new HttpClientHandler() };

        var client = new HttpClient(handler);

        using var response = await client.PostAsync(
            "https://httpbin.org/anything",
            new StringContent("Hello, world!", System.Text.Encoding.UTF8, "text/plain")
        );

        capturing.Events.Should().HaveCount(2);
    }
}
