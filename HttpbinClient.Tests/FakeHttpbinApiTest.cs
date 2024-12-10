using FluentAssertions;

namespace HttpbinClient.Tests;

public class FakeHttpbinApiTest
{
    [Fact(DisplayName = "Creates fake Httpbin API")]
    public async Task CreatesFakeHttpbinApi()
    {
        var random = GetActionResponses.Random();

        var handler = FakeActionHandler.FromResult(random);

        var api = new FakeHttpbinApi(handler);

        var action = new GetAction();

        var response = await api.ExecuteAsync(action);

        response.Should().Be(random);
    }

    [Fact(DisplayName = "Chains fake action handlers")]
    public async Task ChainsFakeActionHandlers()
    {
        var random1 = GetActionResponses.Random();
        var random2 = DeleteActionResponses.Random();

        var handler = FakeActionHandler
            .FromResult(random2)
            .Or(FakeActionHandler.FromResult(random1));

        var api = new FakeHttpbinApi(handler);

        var response = await api.ExecuteAsync(new GetAction());

        response.Should().Be(random1);

        var response2 = await api.ExecuteAsync(new DeleteAction());

        response2.Should().Be(random2);
    }
}
