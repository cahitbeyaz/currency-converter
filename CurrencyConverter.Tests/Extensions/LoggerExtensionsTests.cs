using CurrencyConverter.API.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Xunit;

namespace CurrencyConverter.Tests.Extensions
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void GetLogger_WhenLoggerNotInContext_ReturnsNull()
        {
            // Arrange
            var context = new Context();
            
            // Act
            var result = context.GetLogger();
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public void GetLogger_WhenLoggerInContext_ReturnsLogger()
        {
            // Arrange
            var context = new Context();
            var mockLogger = new Mock<ILogger>().Object;
            context["ILogger"] = mockLogger;
            
            // Act
            var result = context.GetLogger();
            
            // Assert
            Assert.NotNull(result);
            Assert.Same(mockLogger, result);
        }
    }
}
