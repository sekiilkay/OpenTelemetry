    using Common.Shared.Middlewares;
using MassTransit;
using OpenTelemetry.Shared.Extensions;
using Serilog;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenTelemetryExtension(builder.Configuration);
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<PaymentService>();

builder.Host.UseSerilog(Logging.Shared.Logger.Logging.ConfigureLogging);

builder.Services.AddHttpClient<PaymentService>(options =>
{
    options.BaseAddress = new Uri(builder.Configuration.GetSection("ApiService")["PaymentApi"]);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEventConsomer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });

        cfg.ReceiveEndpoint("stock.order-created-event.queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsomer>(context);
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
app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.ExceptionMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();
