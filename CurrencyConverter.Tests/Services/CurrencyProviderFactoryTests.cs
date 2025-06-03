using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Services
{
    // Test implementations of ICurrencyProvider to avoid mocking GetType()
    public class FrankfurterCurrencyProvider : ICurrencyProvider
    {
        public Task<ExchangeRate> GetLatestExchangeRatesAsync(string baseCurrency, List<string> symbols) => Task.FromResult(new ExchangeRate());
        public Task<ExchangeRate> GetExchangeRatesByDateAsync(DateTime date, string baseCurrency, List<string> symbols) => Task.FromResult(new ExchangeRate());
        public Task<HistoricalExchangeRate> GetHistoricalExchangeRatesAsync(DateTime startDate, DateTime endDate, string baseCurrency, List<string> symbols) => Task.FromResult(new HistoricalExchangeRate());
        public Task<Dictionary<string, string>> GetAvailableCurrenciesAsync() => Task.FromResult(new Dictionary<string, string>());
    }

    public class CustomCurrencyProvider : ICurrencyProvider
    {
        public Task<ExchangeRate> GetLatestExchangeRatesAsync(string baseCurrency, List<string> symbols) => Task.FromResult(new ExchangeRate());
        public Task<ExchangeRate> GetExchangeRatesByDateAsync(DateTime date, string baseCurrency, List<string> symbols) => Task.FromResult(new ExchangeRate());
        public Task<HistoricalExchangeRate> GetHistoricalExchangeRatesAsync(DateTime startDate, DateTime endDate, string baseCurrency, List<string> symbols) => Task.FromResult(new HistoricalExchangeRate());
        public Task<Dictionary<string, string>> GetAvailableCurrenciesAsync() => Task.FromResult(new Dictionary<string, string>());
    }

    public class Provider1CurrencyProvider : ICurrencyProvider
    {
        public Task<ExchangeRate> GetLatestExchangeRatesAsync(string baseCurrency, List<string> symbols) => Task.FromResult(new ExchangeRate());
        public Task<ExchangeRate> GetExchangeRatesByDateAsync(DateTime date, string baseCurrency, List<string> symbols) => Task.FromResult(new ExchangeRate());
        public Task<HistoricalExchangeRate> GetHistoricalExchangeRatesAsync(DateTime startDate, DateTime endDate, string baseCurrency, List<string> symbols) => Task.FromResult(new HistoricalExchangeRate());
        public Task<Dictionary<string, string>> GetAvailableCurrenciesAsync() => Task.FromResult(new Dictionary<string, string>());
    }

    public class Provider2CurrencyProvider : ICurrencyProvider
    {
        public Task<ExchangeRate> GetLatestExchangeRatesAsync(string baseCurrency, List<string> symbols) => Task.FromResult(new ExchangeRate());
        public Task<ExchangeRate> GetExchangeRatesByDateAsync(DateTime date, string baseCurrency, List<string> symbols) => Task.FromResult(new ExchangeRate());
        public Task<HistoricalExchangeRate> GetHistoricalExchangeRatesAsync(DateTime startDate, DateTime endDate, string baseCurrency, List<string> symbols) => Task.FromResult(new HistoricalExchangeRate());
        public Task<Dictionary<string, string>> GetAvailableCurrenciesAsync() => Task.FromResult(new Dictionary<string, string>());
    }

    public class CurrencyProviderFactoryTests
    {
        private readonly ICurrencyProvider _frankfurterProvider;
        private readonly ICurrencyProvider _customProvider;
        private readonly Mock<ILogger<CurrencyProviderFactory>> _mockLogger;
        private CurrencyProviderFactory _factory;

        public CurrencyProviderFactoryTests()
        {
            _frankfurterProvider = new FrankfurterCurrencyProvider();
            _customProvider = new CustomCurrencyProvider();
            _mockLogger = new Mock<ILogger<CurrencyProviderFactory>>();
            
            var providers = new List<ICurrencyProvider>
            {
                _frankfurterProvider,
                _customProvider
            };
            
            _factory = new CurrencyProviderFactory(providers, _mockLogger.Object);
        }

        [Fact]
        public void GetProvider_WithNoProviderName_ReturnsDefaultProvider()
        {
            // Act
            var provider = _factory.GetProvider();
            
            // Assert
            Assert.Equal(_frankfurterProvider, provider);
        }

        [Fact]
        public void GetProvider_WithExistingProviderName_ReturnsCorrectProvider()
        {
            // Act
            var provider = _factory.GetProvider("custom");
            
            // Assert
            Assert.Equal(_customProvider, provider);
        }

        [Fact]
        public void GetProvider_WithNonExistentProviderName_ReturnsDefaultProvider()
        {
            // Act
            var provider = _factory.GetProvider("nonexistent");
            
            // Assert
            Assert.Equal(_frankfurterProvider, provider);
        }

        [Fact]
        public void GetProvider_WithCaseInsensitiveProviderName_ReturnsCorrectProvider()
        {
            // Act
            var provider = _factory.GetProvider("CUSTOM");
            
            // Assert
            Assert.Equal(_customProvider, provider);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var providers = new List<ICurrencyProvider>
            {
                _frankfurterProvider
            };
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CurrencyProviderFactory(providers, null));
        }

        [Fact]
        public void Constructor_RegistersProvidersCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyProviderFactory>>();
            var provider1 = new Provider1CurrencyProvider();
            var provider2 = new Provider2CurrencyProvider();
            
            var providers = new List<ICurrencyProvider>
            {
                provider1,
                provider2
            };
            
            // Act
            var factory = new CurrencyProviderFactory(providers, mockLogger.Object);
            
            // Assert
            var resolvedProvider1 = factory.GetProvider("provider1");
            var resolvedProvider2 = factory.GetProvider("provider2");
            
            Assert.Equal(provider1, resolvedProvider1);
            Assert.Equal(provider2, resolvedProvider2);
        }
    }
}
