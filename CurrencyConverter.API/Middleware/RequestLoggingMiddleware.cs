using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                var clientId = context.User.FindFirstValue("ClientId") ?? "anonymous";
                var clientIp = context.Connection.RemoteIpAddress.ToString();
                var method = context.Request.Method;
                var endpoint = context.Request.Path;
                var statusCode = context.Response.StatusCode;
                var responseTime = stopwatch.ElapsedMilliseconds;
                
                _logger.LogInformation(
                    "Request: {ClientIp} ClientId={ClientId} Method={Method} Path={Endpoint} " +
                    "StatusCode={StatusCode} ResponseTime={ResponseTime}ms",
                    clientIp, clientId, method, endpoint, statusCode, responseTime);
            }
        }
    }
}
