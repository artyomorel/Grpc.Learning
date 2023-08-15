namespace Server.Models;

public class OpenMeteoResponse
{
    public Hourly hourly { get; set; }
}

public class Hourly
{
    public List<long> time { get; set; }
    public List<double> temperature_2m { get; set; }
}
