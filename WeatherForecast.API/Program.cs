using Events;
using Events.Http;
using Middleware;
using OpenMeteo;

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
    string requestId()
    {
        var context =
            provider.GetRequiredService<IHttpContextAccessor>().HttpContext
            ?? throw new InvalidOperationException("No HttpContext");
            return Middlewares.RequestIdLens.Get(context);
    }

    var handler = new LoggingHttpHandler(events, requestId)
    {
        InnerHandler = new HttpClientHandler(),
    };
    var client = new HttpClient(handler);
    var baseUrl = new Uri(builder.Configuration["WeatherForecastAPI:BaseUrl"]!);
    return new OpenMeteoApi(baseUrl, client);
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
