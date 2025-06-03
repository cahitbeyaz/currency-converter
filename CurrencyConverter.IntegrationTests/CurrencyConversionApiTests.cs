using System.Net;
using System.Net.Http.Json;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CurrencyConverter.IntegrationTests
{
    public class CurrencyConversionApiTests : IClassFixture<CurrencyConverterApiFactory>
    {
        private readonly HttpClient _client;

        public CurrencyConversionApiTests(CurrencyConverterApiFactory factory)
        {
            _client = factory.Client;
        }

        [Fact]
        public async Task ConvertCurrency_WithValidParameters_ReturnsSuccessStatusCode()
        {
            // Arrange
            string fromCurrency = "EUR";
            string toCurrency = "USD";
            decimal amount = 100;

            // Act
            var response = await _client.GetAsync($"/api/v1/currencyconversion/convert?from={fromCurrency}&to={toCurrency}&amount={amount}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ConvertCurrency_WithValidParameters_ReturnsConvertedAmount()
        {
            // Arrange
            string fromCurrency = "EUR";
            string toCurrency = "USD";
            decimal amount = 100;

            // Act
            var response = await _client.GetAsync($"/api/v1/currencyconversion/convert?from={fromCurrency}&to={toCurrency}&amount={amount}");
            var conversionResult = await response.Content.ReadFromJsonAsync<CurrencyConversionResult>();

            // Assert
            conversionResult.Should().NotBeNull();
            conversionResult!.FromCurrency.Should().Be(fromCurrency);
            conversionResult.ToCurrency.Should().Be(toCurrency);
            conversionResult.OriginalAmount.Should().Be(amount);
            conversionResult.ConvertedAmount.Should().BeGreaterThan(0);
            conversionResult.ExchangeRate.Should().BeGreaterThan(0);
            conversionResult.ConversionDate.Should().BeCloseTo(DateTime.UtcNow.Date, TimeSpan.FromDays(1));
        }

        [Fact]
        public async Task ConvertCurrency_WithInvalidFromCurrency_ReturnsBadRequest()
        {
            // Arrange
            string fromCurrency = "INVALID";
            string toCurrency = "USD";
            decimal amount = 100;

            // Act
            var response = await _client.GetAsync($"/api/v1/currencyconversion/convert?from={fromCurrency}&to={toCurrency}&amount={amount}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ConvertCurrency_WithNegativeAmount_ReturnsBadRequest()
        {
            // Arrange
            string fromCurrency = "EUR";
            string toCurrency = "USD";
            decimal amount = -100;

            // Act
            var response = await _client.GetAsync($"/api/v1/currencyconversion/convert?from={fromCurrency}&to={toCurrency}&amount={amount}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
