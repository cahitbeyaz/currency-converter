using System.Net;
using CurrencyConverter.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CurrencyConverter.IntegrationTests
{
    public class RateLimitingTests : IClassFixture<CurrencyConverterApiFactory>
    {
        private readonly HttpClient _client;

        public RateLimitingTests(CurrencyConverterApiFactory factory)
        {
            _client = factory.Client;
        }

        [Fact]
        public async Task ExchangeRatesEndpoint_WhenRateLimitExceeded_ReturnsTooManyRequests()
        {
            // Arrange
            string baseCurrency = "EUR";
            string symbols = "USD,GBP";
            string endpoint = $"/api/v1/exchangerates/latest?baseCurrency={baseCurrency}&symbols={symbols}";
            HttpResponseMessage? rateLimitedResponse = null;
            
            // Act - Keep sending requests until rate limit is hit or we reach a max number of attempts
            for (int i = 0; i < 35; i++) // The rate limit is set to 30 per minute in your configuration
            {
                var response = await _client.GetAsync(endpoint);
                
                // If we hit the rate limit, save the response and break
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    rateLimitedResponse = response;
                    break;
                }
                
                // Otherwise ensure that normal requests are succeeding
                if (i < 30) // We expect the first 30 to succeed
                {
                    response.StatusCode.Should().Be(HttpStatusCode.OK, 
                        $"Request {i+1} should have succeeded but failed with {response.StatusCode}");
                }
            }

            // Assert
            rateLimitedResponse.Should().NotBeNull("Rate limit was not triggered");
            rateLimitedResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
            
            // Verify rate limit headers
            rateLimitedResponse.Headers.Should().ContainKey("X-Rate-Limit-Limit");
            rateLimitedResponse.Headers.Should().ContainKey("X-Rate-Limit-Remaining");
            rateLimitedResponse.Headers.Should().ContainKey("X-Rate-Limit-Reset");
        }
    }
}
