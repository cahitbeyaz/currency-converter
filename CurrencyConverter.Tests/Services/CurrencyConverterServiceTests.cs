using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Services
{
    public class CurrencyConverterServiceTests
    {
        private readonly Mock<ICurrencyProviderFactory> _mockProviderFactory;
        private readonly Mock<ICurrencyProvider> _mockProvider;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<CurrencyConverterService>> _mockLogger;
        private readonly CurrencyConverterService _service;

        public CurrencyConverterServiceTests()
        {
            _mockProviderFactory = new Mock<ICurrencyProviderFactory>();
            _mockProvider = new Mock<ICurrencyProvider>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<CurrencyConverterService>>();
            
            // Set up memory cache mock to handle Set method
            var cacheMock = new Mock<ICacheEntry>();
            _mockCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheMock.Object);
            
            // Set up memory cache mock to handle TryGetValue method
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Returns(false); // Default to cache miss
            
            _mockProviderFactory
                .Setup(x => x.GetProvider(null))
                .Returns(_mockProvider.Object);
            
            _service = new CurrencyConverterService(
                _mockProviderFactory.Object,
                _mockCache.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ConvertCurrencyAsync_WithValidRequest_ReturnsConversionResult()
        {
            // Arrange
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100m
            };
            
            var exchangeRate = new ExchangeRate
            {
                Base = "USD",
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m }
                }
            };
            
            object cachedValue = null;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);
            
            _mockProvider
                .Setup(x => x.GetLatestExchangeRatesAsync("USD", It.IsAny<List<string>>()))
                .ReturnsAsync(exchangeRate);

            // Act
            var result = await _service.ConvertCurrencyAsync(request);

            // Assert
            Assert.Equal("USD", result.FromCurrency);
            Assert.Equal("EUR", result.ToCurrency);
            Assert.Equal(100m, result.Amount);
            Assert.Equal(85m, result.ConvertedAmount);
            Assert.Equal(0.85m, result.ExchangeRate);
            Assert.Equal(DateTime.Today, result.Date);
            
            _mockProvider.Verify(x => x.GetLatestExchangeRatesAsync("USD", It.IsAny<List<string>>()), Times.Once);
            _mockCache.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ConvertCurrencyAsync_WithCachedRate_UsesCache()
        {
            // Arrange
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100m
            };
            
            var exchangeRate = new ExchangeRate
            {
                Base = "USD",
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m }
                }
            };
            
            var cacheEntry = new Mock<ICacheEntry>();
            
            object cachedExchangeRate = exchangeRate;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedExchangeRate))
                .Returns(true);

            // Act
            var result = await _service.ConvertCurrencyAsync(request);

            // Assert
            Assert.Equal("USD", result.FromCurrency);
            Assert.Equal("EUR", result.ToCurrency);
            Assert.Equal(100m, result.Amount);
            Assert.Equal(85m, result.ConvertedAmount);
            Assert.Equal(0.85m, result.ExchangeRate);
            
            _mockProvider.Verify(x => x.GetLatestExchangeRatesAsync(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public async Task ConvertCurrencyAsync_WithSpecificDate_FetchesHistoricalRates()
        {
            // Arrange
            var date = DateTime.Today.AddDays(-5);
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100m,
                Date = date
            };
            
            var exchangeRate = new ExchangeRate
            {
                Base = "USD",
                Date = date,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.82m }
                }
            };
            
            object cachedValue = null;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);
            
            _mockProvider
                .Setup(x => x.GetExchangeRatesByDateAsync(date, "USD", It.IsAny<List<string>>()))
                .ReturnsAsync(exchangeRate);

            // Act
            var result = await _service.ConvertCurrencyAsync(request);

            // Assert
            Assert.Equal("USD", result.FromCurrency);
            Assert.Equal("EUR", result.ToCurrency);
            Assert.Equal(100m, result.Amount);
            Assert.Equal(82m, result.ConvertedAmount);
            Assert.Equal(0.82m, result.ExchangeRate);
            Assert.Equal(date, result.Date);
            
            _mockProvider.Verify(x => x.GetExchangeRatesByDateAsync(date, "USD", It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task ConvertCurrencyAsync_WithRestrictedCurrency_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "USD",
                ToCurrency = "TRY", // Restricted currency
                Amount = 100m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ConvertCurrencyAsync(request));
            Assert.Contains("restricted currencies", exception.Message);
            Assert.Contains("TRY", exception.Message);
            
            _mockProvider.Verify(x => x.GetLatestExchangeRatesAsync(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public async Task ConvertCurrencyAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ConvertCurrencyAsync(null));
        }

        [Fact]
        public async Task ConvertCurrencyAsync_WithEmptyCurrencies_ThrowsArgumentException()
        {
            // Arrange
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "",
                ToCurrency = "EUR",
                Amount = 100m
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ConvertCurrencyAsync(request));
        }

        [Fact]
        public async Task GetLatestExchangeRatesAsync_ReturnsRatesFromProvider()
        {
            // Arrange
            var baseCurrency = "USD";
            var symbols = new List<string> { "EUR", "GBP" };
            
            var expectedRates = new ExchangeRate
            {
                Base = baseCurrency,
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };
            
            object cachedValue = null;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);
            
            _mockProvider
                .Setup(x => x.GetLatestExchangeRatesAsync(baseCurrency, symbols))
                .ReturnsAsync(expectedRates);

            // Act
            var result = await _service.GetLatestExchangeRatesAsync(baseCurrency, symbols);

            // Assert
            Assert.Equal(expectedRates.Base, result.Base);
            Assert.Equal(expectedRates.Date, result.Date);
            Assert.Equal(expectedRates.Rates.Count, result.Rates.Count);
            Assert.Equal(expectedRates.Rates["EUR"], result.Rates["EUR"]);
            Assert.Equal(expectedRates.Rates["GBP"], result.Rates["GBP"]);
            
            _mockProvider.Verify(x => x.GetLatestExchangeRatesAsync(baseCurrency, symbols), Times.Once);
        }

        [Fact]
        public async Task GetLatestExchangeRatesAsync_WithCachedRates_ReturnsCachedRates()
        {
            // Arrange
            var baseCurrency = "USD";
            var symbols = new List<string> { "EUR", "GBP" };
            
            var cachedRates = new ExchangeRate
            {
                Base = baseCurrency,
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };
            
            object cachedValue = cachedRates;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(true);

            // Act
            var result = await _service.GetLatestExchangeRatesAsync(baseCurrency, symbols);

            // Assert
            Assert.Equal(cachedRates.Base, result.Base);
            Assert.Equal(cachedRates.Date, result.Date);
            Assert.Equal(cachedRates.Rates.Count, result.Rates.Count);
            
            _mockProvider.Verify(x => x.GetLatestExchangeRatesAsync(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public async Task GetHistoricalExchangeRatesAsync_ReturnsHistoricalRatesFromProvider()
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
            
            object cachedValue = null;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);
            
            _mockProvider
                .Setup(x => x.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, symbols))
                .ReturnsAsync(historicalRates);

            // Act
            var result = await _service.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, symbols);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize); // Default
            Assert.Equal(2, result.TotalCount);
            
            _mockProvider.Verify(x => x.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, symbols), Times.Once);
        }

        [Fact]
        public async Task GetHistoricalExchangeRatesAsync_WithCustomPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(-10);
            var endDate = DateTime.Today;
            var baseCurrency = "USD";
            var symbols = new List<string> { "EUR", "GBP" };
            var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 5 };
            
            var ratesDictionary = new Dictionary<string, Dictionary<string, decimal>>();
            
            // Create 10 days of rates
            for (int i = 0; i < 10; i++)
            {
                var date = startDate.AddDays(i);
                ratesDictionary.Add(
                    date.ToString("yyyy-MM-dd"),
                    new Dictionary<string, decimal> 
                    { 
                        { "EUR", 0.85m + (0.01m * i) },
                        { "GBP", 0.75m + (0.01m * i) }
                    }
                );
            }
            
            var historicalRates = new HistoricalExchangeRate
            {
                Base = baseCurrency,
                StartDate = startDate,
                EndDate = endDate,
                Rates = ratesDictionary
            };
            
            object cachedValue = null;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);
            
            _mockProvider
                .Setup(x => x.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, symbols))
                .ReturnsAsync(historicalRates);

            // Act
            var result = await _service.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, symbols, paginationParams);

            // Assert
            Assert.Equal(5, result.Items.Count); // Page size of 5
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(10, result.TotalCount);
            
            _mockProvider.Verify(x => x.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, symbols), Times.Once);
        }

        [Fact]
        public async Task GetHistoricalExchangeRatesAsync_FiltersRestrictedCurrencies()
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(-5);
            var endDate = DateTime.Today;
            var baseCurrency = "USD";
            
            var ratesDictionary = new Dictionary<string, Dictionary<string, decimal>>
            {
                { 
                    startDate.ToString("yyyy-MM-dd"), 
                    new Dictionary<string, decimal> 
                    { 
                        { "EUR", 0.85m }, 
                        { "TRY", 8.5m },  // Restricted currency
                        { "GBP", 0.75m } 
                    }
                }
            };
            
            var historicalRates = new HistoricalExchangeRate
            {
                Base = baseCurrency,
                StartDate = startDate,
                EndDate = endDate,
                Rates = ratesDictionary
            };
            
            object cachedValue = null;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);
            
            _mockProvider
                .Setup(x => x.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, null))
                .ReturnsAsync(historicalRates);

            // Act
            var result = await _service.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency);

            // Assert
            Assert.Single(result.Items);
            var rates = result.Items[0].Value;
            Assert.Equal(2, rates.Count); // EUR and GBP, but not TRY
            Assert.True(rates.ContainsKey("EUR"));
            Assert.True(rates.ContainsKey("GBP"));
            Assert.False(rates.ContainsKey("TRY")); // Restricted currency should be filtered out
        }


    }
}
