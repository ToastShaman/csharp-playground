using Events;
using Events.Http;
using Middleware;
using OpenMeteo;
using Polly;
using Polly.Retry;

var events = EventFilters
    .AddServiceName("WeatherForecast.API")
    .Then(EventFilters.AddTimestamp())
    .Then(EventFilters.AddEventName())
    .Then(new JsonEvents());

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IOpenMeteoApi>(provider =>
{
    var requestId = OpenMeteoClientFactory.RequestIdFromContext(provider);

    var client = OpenMeteoClientFactory.HttpClientFactory(
        requestId,
        TimeSpan.FromSeconds(5),
        events
    );

    var baseUrl = new Uri(
        builder.Configuration["WeatherForecastAPI:BaseUrl"]
            ?? throw new InvalidOperationException("Missing WeatherForecastAPI:BaseUrl")
    );

    var retryOptions = new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        MaxDelay = TimeSpan.FromSeconds(30),
    };

    return OpenMeteoClientFactory.Create(baseUrl, retryOptions, client())();
});

var app = builder.Build();

app.UseHttpsRedirection();
app.Use(Middlewares.RequestId());
app.Use(Middlewares.EventMiddleware(events));
app.MapGet(
        "/weatherforecast",
        async (IOpenMeteoApi api, HttpContext httpContext) =>
            await api.ExecuteAsync(new GetForecast(52.52, 13.41), httpContext.RequestAborted)
    )
    .WithName("GetWeatherForecast");

app.Run();
