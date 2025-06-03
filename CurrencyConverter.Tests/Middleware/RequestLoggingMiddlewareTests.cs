using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CurrencyConverter.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Middleware
{
    public class RequestLoggingMiddlewareTests
    {
        private readonly Mock<ILogger<RequestLoggingMiddleware>> _mockLogger;
        private readonly RequestLoggingMiddleware _middleware;
        private readonly RequestDelegate _nextMiddleware;

        public RequestLoggingMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();
            _nextMiddleware = (HttpContext context) => Task.CompletedTask;
            _middleware = new RequestLoggingMiddleware(_nextMiddleware, _mockLogger.Object);
        }

        [Fact]
        public async Task InvokeAsync_LogsRequestInformation_WhenRequestCompletes()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/exchange-rates";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

            // Create a ClaimsPrincipal with a ClientId claim
            var claims = new[]
            {
                new Claim("ClientId", "test-user-123")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            context.User = principal;

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            // Verify that the logger.LogInformation was called with appropriate parameters
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ClientId=test-user-123") && 
                                                  v.ToString().Contains("Method=GET") && 
                                                  v.ToString().Contains("Path=/api/exchange-rates") && 
                                                  v.ToString().Contains("StatusCode=200")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_LogsAnonymousUser_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/exchange-rates";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            
            // Explicitly set an empty ClaimsPrincipal to ensure no null reference
            var identity = new ClaimsIdentity();
            context.User = new ClaimsPrincipal(identity);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ClientId=anonymous")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_LogsRequestInformation_WhenNextMiddlewareThrowsException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/api/currency-conversion";
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            
            // Explicitly set an empty ClaimsPrincipal to ensure no null reference
            var identity = new ClaimsIdentity();
            context.User = new ClaimsPrincipal(identity);

            var exceptionMessage = "Test exception";
            RequestDelegate nextMiddlewareWithException = (HttpContext httpContext) => 
            {
                throw new Exception(exceptionMessage);
            };
            
            var middlewareWithException = new RequestLoggingMiddleware(nextMiddlewareWithException, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await middlewareWithException.InvokeAsync(context));

            // Verify logging still occurred despite the exception
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Method=POST") && 
                                                  v.ToString().Contains("Path=/api/currency-conversion")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_IncludesResponseTime_InLogMessage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/exchange-rates";
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            
            // Explicitly set an empty ClaimsPrincipal to ensure no null reference
            var identity = new ClaimsIdentity();
            context.User = new ClaimsPrincipal(identity);
            
            // Create middleware with delayed next delegate to ensure measurable response time
            RequestDelegate delayedNext = async (HttpContext httpContext) => 
            {
                await Task.Delay(10); // Small delay to ensure measurable response time
            };
            
            var middlewareWithDelay = new RequestLoggingMiddleware(delayedNext, _mockLogger.Object);

            // Act
            await middlewareWithDelay.InvokeAsync(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ResponseTime=")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
