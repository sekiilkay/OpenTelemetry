using Common.Shared.Middlewares;
using OpenTelemetry.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Order.API.OrderServices;
using Order.API.StockServices;
using MassTransit;
using Order.API.RedisServices;
using StackExchange.Redis;
using Serilog;
using Logging.Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog(Logging.Shared.Logger.Logging.ConfigureLogging);

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddOpenTelemetryExtension(builder.Configuration);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisService = sp.GetService<RedisService>();

    return redisService.GetConnectionMultiplexer;
});

builder.Services.AddSingleton(sp =>
{
    return new RedisService(builder.Configuration);
});

builder.Services.AddHttpClient<StockService>(options =>
{
    options.BaseAddress = new Uri(builder.Configuration.GetSection("ApiService")["StockApi"]);
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlCon"));
});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();
app.ExceptionMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();
