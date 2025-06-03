using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CurrencyConverter.API.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Xunit;

namespace CurrencyConverter.Tests.Infrastructure
{
    public class HttpResiliencePoliciesTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public HttpResiliencePoliciesTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public async Task RetryPolicy_ShouldRetry_OnTransientError()
        {
            // Arrange
            var policy = GetRetryPolicy();
            var context = new Context().WithValue("ILogger", _mockLogger.Object);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            int executionCount = 0;

            // Act
            try
            {
                await policy.ExecuteAsync(ctx => 
                {
                    executionCount++;
                    return Task.FromResult(httpResponseMessage);
                }, context);
            }
            catch (Exception)
            {
                // Expected to throw after retries
            }

            // Assert
            Assert.Equal(4, executionCount); // 1 original + 3 retries
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Delaying for")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(10), // Fast retry for tests
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        context.GetLogger()?.LogWarning("Delaying for {RetryTime}ms, then making retry {RetryCount}",
                            timespan.TotalMilliseconds, retryAttempt);
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromMilliseconds(100), // Fast break for tests
                    onBreak: (outcome, timespan, context) =>
                    {
                        context.GetLogger()?.LogWarning("Circuit tripped! Circuit is now open and requests won't be made for {BreakTime}ms",
                            timespan.TotalMilliseconds);
                    },
                    onReset: context =>
                    {
                        context.GetLogger()?.LogInformation("Circuit reset! Requests are now allowed again");
                    });
        }
    }

    // Extension to add Context.WithValue for testing
    internal static class ContextExtensions
    {
        public static Context WithValue(this Context context, string key, object value)
        {
            context[key] = value;
            return context;
        }
    }
}
