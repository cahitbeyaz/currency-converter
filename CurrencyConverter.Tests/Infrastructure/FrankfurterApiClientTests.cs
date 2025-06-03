using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Infrastructure.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CurrencyConverter.Tests.Infrastructure
{
    public class FrankfurterApiClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<FrankfurterApiClient>> _mockLogger;
        private readonly FrankfurterApiClient _client;

        public FrankfurterApiClientTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };
            _mockLogger = new Mock<ILogger<FrankfurterApiClient>>();
            
            _client = new FrankfurterApiClient(_httpClient, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ReturnsExchangeRate()
        {
            // Arrange
            var baseCurrency = "USD";
            var symbols = new List<string> { "EUR", "GBP" };
            
            var response = new ExchangeRate
            {
                Base = baseCurrency,
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };
            
            var responseJson = JsonSerializer.Serialize(response);
            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            // Act
            var result = await _client.GetLatestRatesAsync(baseCurrency, symbols);

            // Assert
            Assert.Equal(baseCurrency, result.Base);
            Assert.Equal(DateTime.Today, result.Date);
            Assert.Equal(2, result.Rates.Count);
            Assert.Equal(0.85m, result.Rates["EUR"]);
            Assert.Equal(0.75m, result.Rates["GBP"]);
            
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get && 
                        req.RequestUri.ToString().Contains("latest") &&
                        req.RequestUri.ToString().Contains("base=USD") &&
                        req.RequestUri.ToString().Contains("symbols=EUR,GBP")),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetRatesByDateAsync_ReturnsExchangeRate()
        {
            // Arrange
            var date = new DateTime(2023, 1, 15);
            var baseCurrency = "USD";
            var symbols = new List<string> { "EUR", "GBP" };
            
            var response = new ExchangeRate
            {
                Base = baseCurrency,
                Date = date,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };
            
            var responseJson = JsonSerializer.Serialize(response);
            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            // Act
            var result = await _client.GetRatesByDateAsync(date, baseCurrency, symbols);

            // Assert
            Assert.Equal(baseCurrency, result.Base);
            Assert.Equal(date, result.Date);
            Assert.Equal(2, result.Rates.Count);
            Assert.Equal(0.85m, result.Rates["EUR"]);
            Assert.Equal(0.75m, result.Rates["GBP"]);
            
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get && 
                        req.RequestUri.ToString().Contains("2023-01-15") &&
                        req.RequestUri.ToString().Contains("base=USD") &&
                        req.RequestUri.ToString().Contains("symbols=EUR,GBP")),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetLatestRatesAsync_WithApiError_ThrowsException()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"error\": \"Invalid base currency\"}")
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _client.GetLatestRatesAsync("INVALID", null));
        }

        [Fact]
        public async Task GetRatesByDateAsync_WithApiError_ThrowsException()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("{\"error\": \"Not found\"}")
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _client.GetRatesByDateAsync(DateTime.Today.AddYears(-100), "USD", null));
        }
    }
}
