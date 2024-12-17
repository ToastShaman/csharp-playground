using Events;
using Microsoft.Extensions.Options;
using Middleware;
using OpenMeteo;
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

builder
    .Services.AddOptions<OpenMeteoConfig>()
    .Bind(builder.Configuration.GetSection("OpenMeteo"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(provider =>
{
    var config =
        provider.GetService<IOptions<OpenMeteoConfig>>()?.Value
        ?? throw new InvalidOperationException("OpenMeteoConfig not configured");

    Console.WriteLine($"Configured OpenMeteo API with BaseUrl: {config}");

    var httpContextAccessor =
        provider.GetService<IHttpContextAccessor>()
        ?? throw new InvalidOperationException("No HttpContextAccessor");

    var requestId = OpenMeteoClientFactory.RequestIdFromContext(httpContextAccessor);

    var client = OpenMeteoClientFactory.HttpClientFactory(requestId, config.Timeout, events);

    var retryOptions = new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        MaxDelay = TimeSpan.FromSeconds(30),
    };

    return OpenMeteoClientFactory.Create(config.BaseUrl, retryOptions, client())();
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
