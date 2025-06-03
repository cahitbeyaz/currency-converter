using System;
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
    public class CurrencyConversionControllerTests
    {
        private readonly Mock<ICurrencyConverterService> _mockCurrencyService;
        private readonly Mock<ILogger<CurrencyConversionController>> _mockLogger;
        private readonly CurrencyConversionController _controller;

        public CurrencyConversionControllerTests()
        {
            _mockCurrencyService = new Mock<ICurrencyConverterService>();
            _mockLogger = new Mock<ILogger<CurrencyConversionController>>();
            _controller = new CurrencyConversionController(_mockCurrencyService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ConvertCurrency_WithValidRequest_ReturnsOkWithConversionResult()
        {
            // Arrange
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100m
            };
            
            var expectedResponse = new CurrencyConversionResponse
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100m,
                ConvertedAmount = 85m,
                ExchangeRate = 0.85m,
                Date = DateTime.Today
            };
            
            _mockCurrencyService
                .Setup(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ConvertCurrency(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CurrencyConversionResponse>(okResult.Value);
            
            Assert.Equal(expectedResponse.FromCurrency, returnValue.FromCurrency);
            Assert.Equal(expectedResponse.ToCurrency, returnValue.ToCurrency);
            Assert.Equal(expectedResponse.Amount, returnValue.Amount);
            Assert.Equal(expectedResponse.ConvertedAmount, returnValue.ConvertedAmount);
            Assert.Equal(expectedResponse.ExchangeRate, returnValue.ExchangeRate);
            
            _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(
                It.Is<CurrencyConversionRequest>(r => 
                    r.FromCurrency == request.FromCurrency && 
                    r.ToCurrency == request.ToCurrency && 
                    r.Amount == request.Amount)), 
                Times.Once);
        }

        [Fact]
        public async Task ConvertCurrency_WithNullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ConvertCurrency(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Request body cannot be null", badRequestResult.Value.ToString());
            
            _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()), Times.Never);
        }

        [Fact]
        public async Task ConvertCurrency_WhenServiceThrowsInvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "INVALID",
                ToCurrency = "EUR",
                Amount = 100m
            };
            
            var errorMessage = "Invalid currency code";
            
            _mockCurrencyService
                .Setup(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()))
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            // Act
            var result = await _controller.ConvertCurrency(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(errorMessage, badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task ConvertCurrency_WhenServiceThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = -100m // Invalid amount
            };
            
            var errorMessage = "Amount must be positive";
            
            _mockCurrencyService
                .Setup(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()))
                .ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            var result = await _controller.ConvertCurrency(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(errorMessage, badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task ConvertCurrency_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            var request = new CurrencyConversionRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100m
            };
            
            _mockCurrencyService
                .Setup(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ConvertCurrency(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ConvertCurrencyGet_WithValidParameters_ReturnsOkWithConversionResult()
        {
            // Arrange
            var from = "USD";
            var to = "EUR";
            var amount = 100m;
            
            var expectedResponse = new CurrencyConversionResponse
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100m,
                ConvertedAmount = 85m,
                ExchangeRate = 0.85m,
                Date = DateTime.Today
            };
            
            _mockCurrencyService
                .Setup(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ConvertCurrencyGet(from, to, amount);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CurrencyConversionResponse>(okResult.Value);
            
            Assert.Equal(expectedResponse.FromCurrency, returnValue.FromCurrency);
            Assert.Equal(expectedResponse.ToCurrency, returnValue.ToCurrency);
            Assert.Equal(expectedResponse.Amount, returnValue.Amount);
            Assert.Equal(expectedResponse.ConvertedAmount, returnValue.ConvertedAmount);
            
            _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(
                It.Is<CurrencyConversionRequest>(r => 
                    r.FromCurrency == from && 
                    r.ToCurrency == to && 
                    r.Amount == amount && 
                    r.Date == null)), 
                Times.Once);
        }

        [Fact]
        public async Task ConvertCurrencyGet_WithSpecificDate_ReturnsOkWithConversionResult()
        {
            // Arrange
            var from = "USD";
            var to = "EUR";
            var amount = 100m;
            var date = DateTime.Today.AddDays(-5);
            
            var expectedResponse = new CurrencyConversionResponse
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100m,
                ConvertedAmount = 85m,
                ExchangeRate = 0.85m,
                Date = date
            };
            
            _mockCurrencyService
                .Setup(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ConvertCurrencyGet(from, to, amount, date);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            
            _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(
                It.Is<CurrencyConversionRequest>(r => 
                    r.FromCurrency == from && 
                    r.ToCurrency == to && 
                    r.Amount == amount && 
                    r.Date == date)), 
                Times.Once);
        }

        [Fact]
        public async Task ConvertCurrencyGet_WithEmptyFromCurrency_ReturnsBadRequest()
        {
            // Arrange
            var from = "";
            var to = "EUR";
            var amount = 100m;

            // Act
            var result = await _controller.ConvertCurrencyGet(from, to, amount);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Source and target currencies must be specified", badRequestResult.Value.ToString());
            
            _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()), Times.Never);
        }

        [Fact]
        public async Task ConvertCurrencyGet_WithEmptyToCurrency_ReturnsBadRequest()
        {
            // Arrange
            var from = "USD";
            var to = "";
            var amount = 100m;

            // Act
            var result = await _controller.ConvertCurrencyGet(from, to, amount);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Source and target currencies must be specified", badRequestResult.Value.ToString());
            
            _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()), Times.Never);
        }

        [Fact]
        public async Task ConvertCurrencyGet_WithZeroAmount_ReturnsBadRequest()
        {
            // Arrange
            var from = "USD";
            var to = "EUR";
            var amount = 0m;

            // Act
            var result = await _controller.ConvertCurrencyGet(from, to, amount);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Amount must be greater than zero", badRequestResult.Value.ToString());
            
            _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()), Times.Never);
        }

        [Fact]
        public async Task ConvertCurrencyGet_WithNegativeAmount_ReturnsBadRequest()
        {
            // Arrange
            var from = "USD";
            var to = "EUR";
            var amount = -100m;

            // Act
            var result = await _controller.ConvertCurrencyGet(from, to, amount);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Amount must be greater than zero", badRequestResult.Value.ToString());
            
            _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()), Times.Never);
        }

        [Fact]
        public async Task ConvertCurrencyGet_WhenServiceThrowsInvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var from = "USD";
            var to = "INVALID";
            var amount = 100m;
            
            var errorMessage = "Invalid currency code";
            
            _mockCurrencyService
                .Setup(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()))
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            // Act
            var result = await _controller.ConvertCurrencyGet(from, to, amount);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(errorMessage, badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task ConvertCurrencyGet_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            var from = "USD";
            var to = "EUR";
            var amount = 100m;
            
            _mockCurrencyService
                .Setup(x => x.ConvertCurrencyAsync(It.IsAny<CurrencyConversionRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ConvertCurrencyGet(from, to, amount);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
