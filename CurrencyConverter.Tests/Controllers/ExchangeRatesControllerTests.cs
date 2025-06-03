using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConverter.API.Controllers;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Controllers
{
    public class ExchangeRatesControllerTests
    {
        private readonly Mock<ICurrencyConverterService> _mockCurrencyService;
        private readonly Mock<ILogger<ExchangeRatesController>> _mockLogger;
        private readonly ExchangeRatesController _controller;

        public ExchangeRatesControllerTests()
        {
            _mockCurrencyService = new Mock<ICurrencyConverterService>();
            _mockLogger = new Mock<ILogger<ExchangeRatesController>>();
            _controller = new ExchangeRatesController(_mockCurrencyService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLatestRates_WithValidParameters_ReturnsOkWithRates()
        {
            // Arrange
            var baseCurrency = "USD";
            var symbolsParam = "EUR,JPY,GBP";
            var symbols = new List<string> { "EUR", "JPY", "GBP" };
            
            var expectedRates = new ExchangeRate
            {
                Base = baseCurrency,
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "JPY", 110.5m },
                    { "GBP", 0.75m }
                }
            };
            
            _mockCurrencyService
                .Setup(x => x.GetLatestExchangeRatesAsync(baseCurrency, It.IsAny<List<string>>()))
                .ReturnsAsync(expectedRates);

            // Act
            var result = await _controller.GetLatestRates(baseCurrency, symbolsParam);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ExchangeRate>(okResult.Value);
            
            Assert.Equal(expectedRates.Base, returnValue.Base);
            Assert.Equal(expectedRates.Date, returnValue.Date);
            Assert.Equal(expectedRates.Rates.Count, returnValue.Rates.Count);
            Assert.Equal(expectedRates.Rates["EUR"], returnValue.Rates["EUR"]);
            
            _mockCurrencyService.Verify(x => x.GetLatestExchangeRatesAsync(
                baseCurrency, 
                It.Is<List<string>>(list => list.Count == symbols.Count && 
                                         list.All(item => symbols.Contains(item)))), 
                Times.Once);
        }

        [Fact]
        public async Task GetLatestRates_WithNullSymbols_ReturnsOkWithRates()
        {
            // Arrange
            var baseCurrency = "USD";
            
            var expectedRates = new ExchangeRate
            {
                Base = baseCurrency,
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "JPY", 110.5m },
                    { "GBP", 0.75m }
                }
            };
            
            _mockCurrencyService
                .Setup(x => x.GetLatestExchangeRatesAsync(baseCurrency, null))
                .ReturnsAsync(expectedRates);

            // Act
            var result = await _controller.GetLatestRates(baseCurrency, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ExchangeRate>(okResult.Value);
            
            Assert.Equal(expectedRates.Base, returnValue.Base);
            
            _mockCurrencyService.Verify(x => x.GetLatestExchangeRatesAsync(baseCurrency, null), Times.Once);
        }

        [Fact]
        public async Task GetLatestRates_WithDefaultParameters_ReturnsOkWithRates()
        {
            // Arrange
            var expectedRates = new ExchangeRate
            {
                Base = "EUR",
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal>
                {
                    { "USD", 1.18m },
                    { "JPY", 130.5m },
                    { "GBP", 0.85m }
                }
            };
            
            _mockCurrencyService
                .Setup(x => x.GetLatestExchangeRatesAsync("EUR", null))
                .ReturnsAsync(expectedRates);

            // Act
            var result = await _controller.GetLatestRates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ExchangeRate>(okResult.Value);
            
            Assert.Equal(expectedRates.Base, returnValue.Base);
            
            _mockCurrencyService.Verify(x => x.GetLatestExchangeRatesAsync("EUR", null), Times.Once);
        }

        [Fact]
        public async Task GetLatestRates_WhenServiceThrowsInvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var baseCurrency = "INVALID";
            var errorMessage = "Invalid base currency";
            
            _mockCurrencyService
                .Setup(x => x.GetLatestExchangeRatesAsync(baseCurrency, It.IsAny<List<string>>()))
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            // Act
            var result = await _controller.GetLatestRates(baseCurrency);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(errorMessage, badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetLatestRates_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            _mockCurrencyService
                .Setup(x => x.GetLatestExchangeRatesAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetLatestRates();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetHistoricalRates_WithValidParameters_ReturnsOkWithRates()
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(-10);
            var endDate = DateTime.Today;
            var baseCurrency = "USD";
            var symbolsParam = "EUR,JPY";
            var symbols = new List<string> { "EUR", "JPY" };
            var pageNumber = 1;
            var pageSize = 5;
            
            var expectedRates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { 
                    startDate.ToString("yyyy-MM-dd"), 
                    new Dictionary<string, decimal> { { "EUR", 0.85m }, { "JPY", 110.5m } }
                },
                { 
                    startDate.AddDays(1).ToString("yyyy-MM-dd"), 
                    new Dictionary<string, decimal> { { "EUR", 0.86m }, { "JPY", 111.0m } }
                }
            };
            
            var paginatedResult = new PaginatedResult<KeyValuePair<string, Dictionary<string, decimal>>>
            {
                Items = expectedRates.Select(kvp => new KeyValuePair<string, Dictionary<string, decimal>>(kvp.Key, kvp.Value)).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = expectedRates.Count
            };
            
            _mockCurrencyService
                .Setup(x => x.GetHistoricalExchangeRatesAsync(
                    startDate, 
                    endDate, 
                    baseCurrency, 
                    It.IsAny<List<string>>(), 
                    It.IsAny<PaginationParams>()))
                .ReturnsAsync(paginatedResult);

            // Act
            var result = await _controller.GetHistoricalRates(startDate, endDate, baseCurrency, symbolsParam, pageNumber, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<PaginatedResult<KeyValuePair<string, Dictionary<string, decimal>>>>(okResult.Value);
            
            Assert.Equal(paginatedResult.Items.Count, returnValue.Items.Count);
            Assert.Equal(paginatedResult.PageNumber, returnValue.PageNumber);
            Assert.Equal(paginatedResult.PageSize, returnValue.PageSize);
            Assert.Equal(paginatedResult.TotalCount, returnValue.TotalCount);
            
            _mockCurrencyService.Verify(x => x.GetHistoricalExchangeRatesAsync(
                startDate, 
                endDate, 
                baseCurrency, 
                It.Is<List<string>>(list => list.Count == symbols.Count && 
                                          list.All(item => symbols.Contains(item))),
                It.Is<PaginationParams>(p => p.PageNumber == pageNumber && p.PageSize == pageSize)), 
                Times.Once);
        }

        [Fact]
        public async Task GetHistoricalRates_WithNullEndDate_UsesToday()
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(-10);
            var baseCurrency = "USD";
            
            var paginatedResult = new PaginatedResult<KeyValuePair<string, Dictionary<string, decimal>>>
            {
                Items = new List<KeyValuePair<string, Dictionary<string, decimal>>>(),
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 0
            };
            
            _mockCurrencyService
                .Setup(x => x.GetHistoricalExchangeRatesAsync(
                    startDate, 
                    It.IsAny<DateTime>(), 
                    baseCurrency, 
                    null, 
                    It.IsAny<PaginationParams>()))
                .ReturnsAsync(paginatedResult);

            // Act
            var result = await _controller.GetHistoricalRates(startDate, null, baseCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            
            _mockCurrencyService.Verify(x => x.GetHistoricalExchangeRatesAsync(
                startDate, 
                It.Is<DateTime>(d => d.Date == DateTime.UtcNow.Date), 
                baseCurrency, 
                null,
                It.IsAny<PaginationParams>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetHistoricalRates_WhenServiceThrowsInvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(-10);
            var endDate = DateTime.Today;
            var errorMessage = "Invalid date range";
            
            _mockCurrencyService
                .Setup(x => x.GetHistoricalExchangeRatesAsync(
                    startDate, 
                    endDate, 
                    It.IsAny<string>(), 
                    It.IsAny<List<string>>(), 
                    It.IsAny<PaginationParams>()))
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            // Act
            var result = await _controller.GetHistoricalRates(startDate, endDate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(errorMessage, badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetHistoricalRates_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(-10);
            
            _mockCurrencyService
                .Setup(x => x.GetHistoricalExchangeRatesAsync(
                    It.IsAny<DateTime>(), 
                    It.IsAny<DateTime>(), 
                    It.IsAny<string>(), 
                    It.IsAny<List<string>>(), 
                    It.IsAny<PaginationParams>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetHistoricalRates(startDate);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }


    }
}
