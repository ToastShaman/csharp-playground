using Events;
using Events.Http;
using OpenMeteo;

var events = EventFilters
    .AddServiceName("WeatherForecast.API")
    .Then(EventFilters.AddTimestamp())
    .Then(EventFilters.AddEventName())
    .Then(new JsonEvents());

var handler = new LoggingHttpHandler(events) { InnerHandler = new HttpClientHandler() };

var client = new HttpClient(handler);

var api = new OpenMeteoApi(new Uri("https://api.open-meteo.com"), client);

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async () => await api.ExecuteAsync(new GetForecast(52.52, 13.41)))
    .WithName("GetWeatherForecast");

app.Run();
