using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Shared.Middlewares
{
    public class OpenTelemetryTraceIdMiddleware
    {
        private readonly RequestDelegate _next;
        public OpenTelemetryTraceIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<OpenTelemetryTraceIdMiddleware>>();

            // ILogger create log --> TraceId

            using (logger.BeginScope("{@traceId}",Activity.Current?.TraceId.ToString()))
            {
                await _next(context);
            }
        }
    }
}
