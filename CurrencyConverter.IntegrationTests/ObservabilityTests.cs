using System.Net;
using CurrencyConverter.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CurrencyConverter.IntegrationTests
{
    public class ObservabilityTests : IClassFixture<CurrencyConverterApiFactory>
    {
        private readonly HttpClient _client;

        public ObservabilityTests(CurrencyConverterApiFactory factory)
        {
            _client = factory.Client;
        }

        [Fact]
        public async Task Request_ShouldIncludeCorrelationHeaders()
        {
            // Arrange
            string baseCurrency = "EUR";
            string symbols = "USD,GBP";

            // Act
            var response = await _client.GetAsync($"/api/v1/exchangerates/latest?baseCurrency={baseCurrency}&symbols={symbols}");

            // Assert
            response.EnsureSuccessStatusCode();
            
            // Check for trace correlation headers
            response.Headers.Should().ContainKey("traceparent");
            // W3C Trace Context format: version-trace-id-parent-id-flags
            response.Headers.GetValues("traceparent").First().Should().MatchRegex("^00-[a-f0-9]{32}-[a-f0-9]{16}-01$");
            
            // Check for activity ID in response headers
            response.Headers.Should().ContainKey("Request-Id");
        }

        [Fact]
        public async Task Request_WithCustomTraceId_ShouldPropagateTraceId()
        {
            // Arrange
            string baseCurrency = "EUR";
            string symbols = "USD,GBP";
            
            // Create a custom trace ID in W3C format
            string traceId = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
            _client.DefaultRequestHeaders.Add("traceparent", traceId);

            // Act
            var response = await _client.GetAsync($"/api/v1/exchangerates/latest?baseCurrency={baseCurrency}&symbols={symbols}");

            // Assert
            response.EnsureSuccessStatusCode();
            
            // Check that the trace ID was propagated (may have a different parent ID but same trace ID)
            response.Headers.Should().ContainKey("traceparent");
            string returnedTraceId = response.Headers.GetValues("traceparent").First();
            
            // Extract the trace ID portion (the 32 chars after "00-")
            string originalTraceIdPart = traceId.Substring(3, 32);
            string returnedTraceIdPart = returnedTraceId.Substring(3, 32);
            
            // The trace ID part should be preserved while the parent ID may change
            returnedTraceIdPart.Should().Be(originalTraceIdPart);
        }
    }
}
