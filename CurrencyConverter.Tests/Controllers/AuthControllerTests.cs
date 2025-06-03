using System;
using System.Threading.Tasks;
using CurrencyConverter.API.Controllers;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "password" };
            var authResult = new AuthResult { 
                Success = true, 
                Token = "jwt-token", 
                UserId = "user123", 
                Roles = new[] { "User" } 
            };
            
            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<TokenResponse>(okResult.Value);
            Assert.Equal(authResult.Token, returnValue.Token);
            
            _mockAuthService.Verify(x => x.AuthenticateAsync(loginRequest.Username, loginRequest.Password), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "invalid", Password = "wrong" };
            var authResult = new AuthResult { 
                Success = false, 
                ErrorMessage = "Invalid credentials" 
            };
            
            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            
            _mockAuthService.Verify(x => x.AuthenticateAsync(loginRequest.Username, loginRequest.Password), Times.Once);
        }


        [Fact]
        public async Task Login_WithNullRequest_ThrowsException()
        {
            // Arrange
            LoginRequest request = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _controller.Login(request));
            
            _mockAuthService.Verify(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithEmptyUsername_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "", Password = "password" };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            _mockAuthService.Verify(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithEmptyPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "" };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            _mockAuthService.Verify(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_WhenAuthServiceThrowsException_ThrowsException()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "password" };
            
            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Login(loginRequest));
            
            _mockAuthService.Verify(x => x.AuthenticateAsync(loginRequest.Username, loginRequest.Password), Times.Once);
        }
    }
}
