using GrpcService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpcClient<Weather.WeatherClient>(client =>
{
    var urlServer = builder.Configuration.GetSection("GrpcServer");
    client.Address = new Uri(urlServer.Value);
}).ConfigureChannel(x => { x.MaxRetryAttempts = 3; });


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();