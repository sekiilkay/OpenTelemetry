﻿using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Shared.Logger
{
    public static class Logging
    {
        public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogging => (builderContext, loggerConfiguration) =>
        {
            var environment = builderContext.HostingEnvironment;

            loggerConfiguration
            .ReadFrom.Configuration(builderContext.Configuration) //Serilog
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Env", environment.EnvironmentName)
            .Enrich.WithProperty("AppName", environment.ApplicationName);

            var elasticsearchBaseUrl = builderContext.Configuration.GetSection("Elasticsearch")["BaseUrl"];

            var userName = builderContext.Configuration.GetSection("Elasticsearch")["UserName"];

            var password = builderContext.Configuration.GetSection("Elasticsearch")["Password"];

            var indexName = builderContext.Configuration.GetSection("Elasticsearch")["IndexName"];

            // Connection to ElasticSearch

            loggerConfiguration.WriteTo.Elasticsearch(new (new Uri(elasticsearchBaseUrl!))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
                IndexFormat = $"{indexName}-{environment.EnvironmentName}-logs-" + "{0:yyyy.MM.dd}",
                ModifyConnectionSettings = x => x.BasicAuthentication(userName, password),
                CustomFormatter = new ElasticsearchJsonFormatter()
            });
        };
    }
}
