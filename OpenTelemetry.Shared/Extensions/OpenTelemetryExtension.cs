using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Shared.Constants;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTelemetry.Shared.Extensions
{
    public static class OpenTelemetryExtension
    {
        public static void AddOpenTelemetryExtension(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<OpenTelemetryConstants>(configuration.GetSection("OpenTelemetry"));

            var openTelemetryConstants = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConstants>();

            ActivitySourceProvider.Source = new ActivitySource(openTelemetryConstants.ActivitySourceName);

            services.AddOpenTelemetry().WithTracing(options =>
            {
                options.AddSource(openTelemetryConstants.ActivitySourceName)
                .AddSource(DiagnosticHeaders.DefaultListenerName) //MassTransit (RabbitMQ)
                .ConfigureResource(resource =>
                {
                    resource.AddService(openTelemetryConstants.ServiceName, serviceVersion: openTelemetryConstants.ServiceVersion);
                });

                //AspNetCore
                options.AddAspNetCoreInstrumentation(aspnetcoreOptions =>
                {
                    //Endpoint

                    aspnetcoreOptions.Filter = (context) =>
                    {
                        if (!string.IsNullOrEmpty(context.Request.Path.Value))
                            return context.Request.Path.Value!.Contains("api", StringComparison.InvariantCulture);

                        return false;
                    };

                    //aspnetcoreOptions.RecordException = true;
                });

                //EntityFrameworkCore
                options.AddEntityFrameworkCoreInstrumentation(efCoreOptions =>
                {
                    efCoreOptions.SetDbStatementForText = true;
                    efCoreOptions.SetDbStatementForStoredProcedure = true;
                    efCoreOptions.EnrichWithIDbCommand = (activity, dbCommand) =>
                    {

                    };
                });

                //Http
                options.AddHttpClientInstrumentation(httpOptions =>
                {
                    //ElasticSearch --> http://localhost:9200

                    httpOptions.FilterHttpRequestMessage = (request) =>
                    {
                         return !request.RequestUri.AbsoluteUri.Contains("9200", StringComparison.InvariantCulture);

                    };

                    httpOptions.EnrichWithHttpRequestMessage = async (activity, request) =>
                    {
                        var requestContent = "empty";

                        if (request.Content != null)
                        {
                            requestContent = await request.Content.ReadAsStringAsync();
                        }

                        activity.SetTag("http.request.body", requestContent);
                    };

                    httpOptions.EnrichWithHttpResponseMessage = async (activity, response) =>
                    {
                        if (response.Content != null)
                        {
                            activity.SetTag("http.response.body", await response.Content.ReadAsStringAsync());
                        }
                    };
                });

                //Redis
                options.AddRedisInstrumentation(redisOptions =>
                {
                    redisOptions.SetVerboseDatabaseStatements = true;
                });

                options.AddConsoleExporter(); //Console
                options.AddOtlpExporter(); //Jeager
            });
        }
    }
}
