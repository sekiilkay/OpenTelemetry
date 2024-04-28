using Common.Shared.Middlewares;
using OpenTelemetry.Shared.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenTelemetryExtension(builder.Configuration);
builder.Host.UseSerilog(Logging.Shared.Logger.Logging.ConfigureLogging);

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
