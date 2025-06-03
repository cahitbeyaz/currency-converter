using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Infrastructure.Http;
using CurrencyConverter.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Infrastructure
{
    public class FrankfurterCurrencyProviderTests
    {
        private readonly Mock<IFrankfurterApiClient> _mockApiClient;
        private readonly Mock<ILogger<FrankfurterCurrencyProvider>> _mockLogger;
        private readonly FrankfurterCurrencyProvider _provider;

        public FrankfurterCurrencyProviderTests()
        {
            _mockApiClient = new Mock<IFrankfurterApiClient>();
            _mockLogger = new Mock<ILogger<FrankfurterCurrencyProvider>>();
            
            _provider = new FrankfurterCurrencyProvider(_mockApiClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLatestExchangeRatesAsync_CallsApiClientWithCorrectParameters()
        {
            // Arrange
            var baseCurrency = "USD";
            var symbols = new List<string> { "EUR", "GBP" };
            
            var expectedResponse = new ExchangeRate
            {
                Base = baseCurrency,
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };
            
            _mockApiClient
                .Setup(x => x.GetLatestRatesAsync(baseCurrency, symbols))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _provider.GetLatestExchangeRatesAsync(baseCurrency, symbols);

            // Assert
            Assert.Equal(expectedResponse.Base, result.Base);
            Assert.Equal(expectedResponse.Date, result.Date);
            Assert.Equal(expectedResponse.Rates.Count, result.Rates.Count);
            Assert.Equal(expectedResponse.Rates["EUR"], result.Rates["EUR"]);
            Assert.Equal(expectedResponse.Rates["GBP"], result.Rates["GBP"]);
            
            _mockApiClient.Verify(x => x.GetLatestRatesAsync(baseCurrency, symbols), Times.Once);
        }

        [Fact]
        public async Task GetExchangeRatesByDateAsync_CallsApiClientWithCorrectParameters()
        {
            // Arrange
            var date = DateTime.Today.AddDays(-5);
            var baseCurrency = "USD";
            var symbols = new List<string> { "EUR", "GBP" };
            
            var expectedResponse = new ExchangeRate
            {
                Base = baseCurrency,
                Date = date,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };
            
            _mockApiClient
                .Setup(x => x.GetRatesByDateAsync(date, baseCurrency, symbols))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _provider.GetExchangeRatesByDateAsync(date, baseCurrency, symbols);

            // Assert
            Assert.Equal(expectedResponse.Base, result.Base);
            Assert.Equal(expectedResponse.Date, result.Date);
            Assert.Equal(expectedResponse.Rates.Count, result.Rates.Count);
            Assert.Equal(expectedResponse.Rates["EUR"], result.Rates["EUR"]);
            Assert.Equal(expectedResponse.Rates["GBP"], result.Rates["GBP"]);
            
            _mockApiClient.Verify(x => x.GetRatesByDateAsync(date, baseCurrency, symbols), Times.Once);
        }

        [Fact]
        public async Task GetHistoricalExchangeRatesAsync_CallsApiClientWithCorrectParameters()
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(-5);
            var endDate = DateTime.Today;
            var baseCurrency = "USD";
            var symbols = new List<string> { "EUR", "GBP" };
            
            var ratesDictionary = new Dictionary<string, Dictionary<string, decimal>>
            {
                { 
                    startDate.ToString("yyyy-MM-dd"), 
                    new Dictionary<string, decimal> { { "EUR", 0.85m }, { "GBP", 0.75m } }
                },
                { 
                    endDate.ToString("yyyy-MM-dd"), 
                    new Dictionary<string, decimal> { { "EUR", 0.86m }, { "GBP", 0.76m } }
                }
            };
            
            var historicalRates = new HistoricalExchangeRate
            {
                Base = baseCurrency,
                StartDate = startDate,
                EndDate = endDate,
                Rates = ratesDictionary
            };
            
            _mockApiClient
                .Setup(x => x.GetHistoricalRatesAsync(startDate, endDate, baseCurrency, symbols))
                .ReturnsAsync(historicalRates);

            // Act
            var result = await _provider.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, symbols);

            // Assert
            Assert.Equal(historicalRates.Base, result.Base);
            Assert.Equal(historicalRates.StartDate, result.StartDate);
            Assert.Equal(historicalRates.EndDate, result.EndDate);
            Assert.Equal(historicalRates.Rates.Count, result.Rates.Count);
            
            _mockApiClient.Verify(x => x.GetHistoricalRatesAsync(startDate, endDate, baseCurrency, symbols), Times.Once);
        }


        
        [Fact]
        public async Task GetLatestExchangeRatesAsync_WithNullSymbols_PassesNullToApiClient()
        {
            // Arrange
            var baseCurrency = "USD";
            
            var expectedResponse = new ExchangeRate
            {
                Base = baseCurrency,
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };
            
            _mockApiClient
                .Setup(x => x.GetLatestRatesAsync(baseCurrency, null))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _provider.GetLatestExchangeRatesAsync(baseCurrency);

            // Assert
            _mockApiClient.Verify(x => x.GetLatestRatesAsync(baseCurrency, null), Times.Once);
        }
        
        [Fact]
        public async Task GetExchangeRatesByDateAsync_WithNullSymbols_PassesNullToApiClient()
        {
            // Arrange
            var date = DateTime.Today.AddDays(-5);
            var baseCurrency = "USD";
            
            var expectedResponse = new ExchangeRate
            {
                Base = baseCurrency,
                Date = date,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };
            
            _mockApiClient
                .Setup(x => x.GetRatesByDateAsync(date, baseCurrency, null))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _provider.GetExchangeRatesByDateAsync(date, baseCurrency);

            // Assert
            _mockApiClient.Verify(x => x.GetRatesByDateAsync(date, baseCurrency, null), Times.Once);
        }
        
        [Fact]
        public async Task GetHistoricalExchangeRatesAsync_WithNullSymbols_PassesNullToApiClient()
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(-5);
            var endDate = DateTime.Today;
            var baseCurrency = "USD";
            
            var historicalRates = new HistoricalExchangeRate
            {
                Base = baseCurrency,
                StartDate = startDate,
                EndDate = endDate,
                Rates = new Dictionary<string, Dictionary<string, decimal>>()
            };
            
            _mockApiClient
                .Setup(x => x.GetHistoricalRatesAsync(startDate, endDate, baseCurrency, null))
                .ReturnsAsync(historicalRates);

            // Act
            var result = await _provider.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency);

            // Assert
            _mockApiClient.Verify(x => x.GetHistoricalRatesAsync(startDate, endDate, baseCurrency, null), Times.Once);
        }
    }
}
