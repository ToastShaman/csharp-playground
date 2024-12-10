using FluentAssertions;

namespace HttpbinClient.Tests;

public class HttpbinApiTest
{
    private readonly Uri _uri = new("https://httpbin.org");
    
    private readonly HttpClient _client = new();
    
    [Fact(DisplayName = "Calls httpbin.org/get and returns response")]
    public async Task CallsGetAndReturnsResponseAsync()
    {
        var api = new HttpbinApi(_uri, _client);

        var action = new GetAction();

        var response = await api.ExecuteAsync(action);

        response.Should().NotBeNull();
        response.Args.Should().NotBeNull();
        response.Url.Should().Be("https://httpbin.org/get");
        response.Headers.Should().ContainKey("Accept").And.ContainValue("application/json");
        response.Headers.Should().ContainKey("Host").And.ContainValue("httpbin.org");
    }
}
