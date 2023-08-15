using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService;
using Server.Models;

namespace Server.Services;

public class WeatherService : Weather.WeatherBase
{
    private readonly HttpClient _httpClient;

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private Dictionary<string, List<WeatherData>> _weatherData = new()
    {
        {
            "Oryol", new List<WeatherData>
            {
                new()
                {
                    Time = Timestamp.FromDateTime(DateTime.UtcNow.Date),
                    Temperature = 32
                },
                new()
                {
                    Time = Timestamp.FromDateTime(DateTime.UtcNow.Date.AddDays(-5)),
                    Temperature = 30
                }
            }
        },
        {
            "Moscow", new List<WeatherData>()
            {
                new()
                {
                    Time = Timestamp.FromDateTime(DateTime.UtcNow.Date),
                    Temperature = 20
                },
                new()
                {
                    Time = Timestamp.FromDateTime(DateTime.UtcNow.Date.AddDays(-5)),
                    Temperature = 25
                }
            }
        },
        {
            "Krasnodar", new List<WeatherData>()
            {
                new()
                {
                    Time = Timestamp.FromDateTime(DateTime.UtcNow.Date),
                    Temperature = 35
                },
                new()
                {
                    Time = Timestamp.FromDateTime(DateTime.UtcNow.Date.AddDays(-5)),
                    Temperature = 38
                }
            }
        }
    };


    public override async Task<GetCurrentWeatherReply> GetCurrentWeather(GetCurrentWeatherRequest request, ServerCallContext context)
    {
        var isSuccess = _weatherData.TryGetValue(request.City, out var data);
        if (!isSuccess)
            throw new RpcException(new Status(StatusCode.NotFound, "City don't found"));

        var reply = new GetCurrentWeatherReply
        {
            City = request.City
        };

        reply.Data.AddRange(data);

        return reply;
    }

    public override async Task GetStreamWeather(EmptyRequest request, IServerStreamWriter<WeatherData> responseStream, ServerCallContext context)
    {
        var weatherResponse = await _httpClient.GetAsync("https://api.open-meteo.com/v1/forecast?latitude=52.9651&longitude=36.0785&hourly=temperature_2m&timeformat=unixtime&timezone=auto");
        var openMeteoResponse = await JsonSerializer.DeserializeAsync<OpenMeteoResponse>(await weatherResponse.Content.ReadAsStreamAsync());
        if (openMeteoResponse == null)
            return;

        for (var i = 0; i < openMeteoResponse.hourly.time.Count; i++)
        {
            var addSeconds = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(openMeteoResponse.hourly.time[i]);
            await responseStream.WriteAsync(new WeatherData
            {
                Time = Timestamp.FromDateTime(addSeconds),
                Temperature = openMeteoResponse.hourly.temperature_2m[i]
            });
            
            // для имитации работы делаем задержку в 1 секунду
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}