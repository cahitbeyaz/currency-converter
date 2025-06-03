using System.Net;
using System.Net.Http.Json;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CurrencyConverter.IntegrationTests
{
    public class ExchangeRatesApiTests : IClassFixture<CurrencyConverterApiFactory>
    {
        private readonly HttpClient _client;

        public ExchangeRatesApiTests(CurrencyConverterApiFactory factory)
        {
            _client = factory.Client;
        }

        [Fact]
        public async Task GetLatestRates_WithValidParameters_ReturnsSuccessStatusCode()
        {
            // Arrange
            string baseCurrency = "EUR";
            string symbols = "USD,GBP";

            // Act
            var response = await _client.GetAsync($"/api/v1/exchangerates/latest?baseCurrency={baseCurrency}&symbols={symbols}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetLatestRates_WithValidParameters_ReturnsExpectedContent()
        {
            // Arrange
            string baseCurrency = "EUR";
            string symbols = "USD,GBP";

            // Act
            var response = await _client.GetAsync($"/api/v1/exchangerates/latest?baseCurrency={baseCurrency}&symbols={symbols}");
            var exchangeRate = await response.Content.ReadFromJsonAsync<ExchangeRate>();

            // Assert
            exchangeRate.Should().NotBeNull();
            exchangeRate!.Base.Should().Be(baseCurrency);
            exchangeRate.Rates.Should().ContainKeys("USD", "GBP");
            exchangeRate.Date.Should().BeCloseTo(DateTime.UtcNow.Date, TimeSpan.FromDays(1));
        }

        [Fact]
        public async Task GetLatestRates_WithInvalidBaseCurrency_ReturnsBadRequest()
        {
            // Arrange
            string baseCurrency = "INVALID";
            string symbols = "USD,GBP";

            // Act
            var response = await _client.GetAsync($"/api/v1/exchangerates/latest?baseCurrency={baseCurrency}&symbols={symbols}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
