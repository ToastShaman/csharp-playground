using Bogus;

namespace HttpbinClient.Tests;

public static class GetActionResponses
{
    public static GetActionResponse Random() =>
        new Faker<GetActionResponse>()
            .CustomInstantiator(f => new GetActionResponse(
                Args: [],
                Headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Host", "httpbin.org" },
                    { "X-Amzn-Trace-Id", f.Random.Uuid().ToString() },
                },
                Origin: f.Internet.Ip(),
                Url: "https://httpbin.org/get"
            ))
            .Generate();
}

public static class DeleteActionResponses
{
    public static DeleteActionResponse Random() =>
        new Faker<DeleteActionResponse>()
            .CustomInstantiator(f => new DeleteActionResponse(
                Args: [],
                Headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Host", "httpbin.org" },
                    { "X-Amzn-Trace-Id", f.Random.Uuid().ToString() },
                },
                Data: "Deleted",
                Origin: f.Internet.Ip(),
                Url: "https://httpbin.org/get"
            ))
            .Generate();
}
