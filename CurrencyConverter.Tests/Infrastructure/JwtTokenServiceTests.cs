using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Infrastructure
{
    public class JwtTokenServiceTests
    {
        private readonly JwtSettings _jwtSettings;
        private readonly JwtTokenService _tokenService;

        public JwtTokenServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                Secret = "ThisIsAVerySecureTestKeyWith32Chars!",
                Issuer = "test-issuer",
                Audience = "test-audience",
                ExpirationInMinutes = 60
            };
            
            var options = new Mock<IOptions<JwtSettings>>();
            options.Setup(x => x.Value).Returns(_jwtSettings);
            
            _tokenService = new JwtTokenService(options.Object);
        }

        [Fact]
        public void GenerateToken_WithValidInputs_ReturnsValidJwtToken()
        {
            // Arrange
            var userId = "user123";
            var email = "test@example.com";
            var roles = new List<string> { "User" };

            // Act
            var token = _tokenService.GenerateToken(userId, email, roles);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            // Validate token structure
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            
            Assert.NotNull(jsonToken);
            Assert.Equal(_jwtSettings.Issuer, jsonToken.Issuer);
            
            // Validate claims
            Assert.Contains(jsonToken.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
            Assert.Contains(jsonToken.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
            Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
            Assert.Contains(jsonToken.Claims, c => c.Type == "ClientId" && c.Value == userId);
            Assert.Contains(jsonToken.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
            
            // Validate audience (token property, not a claim)
            Assert.Equal(_jwtSettings.Audience, jsonToken.Audiences.FirstOrDefault());
            
            // Validate expiration
            var expectedExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
            Assert.True(jsonToken.ValidTo <= expectedExpiration.AddMinutes(1)); // Allow for 1 minute of processing time
            Assert.True(jsonToken.ValidTo >= expectedExpiration.AddMinutes(-1));
        }

        [Fact]
        public void GenerateToken_WithMultipleRoles_IncludesAllRolesInToken()
        {
            // Arrange
            var userId = "admin456";
            var email = "admin@example.com";
            var roles = new List<string> { "User", "Admin", "Manager" };

            // Act
            var token = _tokenService.GenerateToken(userId, email, roles);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            
            var roleClaims = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.Equal(3, roleClaims.Count);
            Assert.Contains("User", roleClaims);
            Assert.Contains("Admin", roleClaims);
            Assert.Contains("Manager", roleClaims);
        }

        [Fact]
        public void GenerateToken_WithNoRoles_CreatesTokenWithoutRoleClaims()
        {
            // Arrange
            var userId = "guest789";
            var email = "guest@example.com";
            var roles = new List<string>();

            // Act
            var token = _tokenService.GenerateToken(userId, email, roles);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            
            var roleClaims = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Empty(roleClaims);
        }

    }
}
