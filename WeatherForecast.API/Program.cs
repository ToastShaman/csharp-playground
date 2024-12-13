using Events;
using Events.Http;
using Middleware;
using OpenMeteo;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

var events = EventFilters
    .AddServiceName("WeatherForecast.API")
    .Then(EventFilters.AddTimestamp())
    .Then(EventFilters.AddEventName())
    .Then(new JsonEvents());

var handler = new LoggingHttpHandler(events) { InnerHandler = new HttpClientHandler() };

var client = new HttpClient(handler);

var baseUrl = new Uri(builder.Configuration["WeatherForecastAPI:BaseUrl"]!);

var api = new OpenMeteoApi(baseUrl, client);

var app = builder.Build();

app.UseHttpsRedirection();
app.Use(Middlewares.RequestId());
app.Use(Middlewares.EventMiddleware(events));
app.MapGet("/weatherforecast", async () => await api.ExecuteAsync(new GetForecast(52.52, 13.41)))
    .WithName("GetWeatherForecast");

app.Run();
