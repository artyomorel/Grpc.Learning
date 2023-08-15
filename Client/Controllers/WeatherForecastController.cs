using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private readonly Weather.WeatherClient _weatherClient;
    
    public WeatherForecastController(Weather.WeatherClient weatherClient)
    {
        _weatherClient = weatherClient;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> Get(string city)
    {
        try
        {
            var result = await _weatherClient.GetCurrentWeatherAsync(new GetCurrentWeatherRequest
            {
                City = city
            });
            return Ok(result);
        }
        catch (RpcException e)
        {
            switch (e.StatusCode)
            {
                case Grpc.Core.StatusCode.NotFound:
                    return NotFound(e.Status.Detail);
                default:
                    return BadRequest();
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetStream()
    {
        var result =  _weatherClient.GetStreamWeather(new EmptyRequest());
        while (await result.ResponseStream.MoveNext(new CancellationToken()))
        {
            var response = result.ResponseStream.Current;
            Console.WriteLine($"Time {response.Time.ToDateTime()}, Temperature {response.Temperature}");
        }

        return Ok(result);
    }
}