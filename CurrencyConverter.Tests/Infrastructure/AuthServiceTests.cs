using System.Linq;
using System.Threading.Tasks;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Infrastructure
{
    public class AuthServiceTests
    {
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_mockTokenService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidUserCredentials_ReturnsSuccessAndToken()
        {
            // Arrange
            var username = "user";
            var password = "password";
            var userId = "user123";
            var roles = new[] { "User" };
            var expectedToken = "valid-jwt-token";
            
            _mockTokenService
                .Setup(x => x.GenerateToken(userId, username, roles))
                .Returns(expectedToken);

            // Act
            var result = await _authService.AuthenticateAsync(username, password);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedToken, result.Token);
            Assert.Equal(userId, result.UserId);
            Assert.Single(result.Roles);
            Assert.Equal("User", result.Roles.First());
            
            _mockTokenService.Verify(x => x.GenerateToken(userId, username, roles), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidAdminCredentials_ReturnsSuccessAndTokenWithAdminRole()
        {
            // Arrange
            var username = "admin";
            var password = "adminpassword";
            var userId = "admin456";
            var roles = new[] { "User", "Admin" };
            var expectedToken = "valid-admin-jwt-token";
            
            _mockTokenService
                .Setup(x => x.GenerateToken(userId, username, roles))
                .Returns(expectedToken);

            // Act
            var result = await _authService.AuthenticateAsync(username, password);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedToken, result.Token);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(2, result.Roles.Length);
            Assert.Contains("User", result.Roles);
            Assert.Contains("Admin", result.Roles);
            
            _mockTokenService.Verify(x => x.GenerateToken(userId, username, roles), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidUsername_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            var username = "nonexistent";
            var password = "password";

            // Act
            var result = await _authService.AuthenticateAsync(username, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password", result.ErrorMessage);
            Assert.Null(result.Token);
            Assert.Null(result.UserId);
            Assert.Null(result.Roles);
            
            _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidPassword_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            var username = "user";
            var password = "wrongpassword";

            // Act
            var result = await _authService.AuthenticateAsync(username, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password", result.ErrorMessage);
            Assert.Null(result.Token);
            Assert.Null(result.UserId);
            Assert.Null(result.Roles);
            
            _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_WithCaseSensitiveUsername_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            var username = "User"; // Uppercase U
            var password = "password";

            // Act
            var result = await _authService.AuthenticateAsync(username, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password", result.ErrorMessage);
            
            _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_WithEmptyUsername_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            var username = "";
            var password = "password";

            // Act
            var result = await _authService.AuthenticateAsync(username, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password", result.ErrorMessage);
            
            _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_WithEmptyPassword_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            var username = "user";
            var password = "";

            // Act
            var result = await _authService.AuthenticateAsync(username, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password", result.ErrorMessage);
            
            _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }
    }
}
