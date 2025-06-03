using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CurrencyConverter.IntegrationTests
{
    public class AuthApiTests : IClassFixture<CurrencyConverterApiFactory>
    {
        private readonly HttpClient _client;

        public AuthApiTests(CurrencyConverterApiFactory factory)
        {
            _client = factory.Client;
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest), 
                Encoding.UTF8, 
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            
            tokenResponse.Should().NotBeNull();
            tokenResponse!.Token.Should().NotBeNullOrEmpty();
            tokenResponse.ExpiresIn.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "wronguser",
                Password = "wrongpassword"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest), 
                Encoding.UTF8, 
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithValidToken_ReturnsSuccess()
        {
            // Arrange - Get a valid token first
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest), 
                Encoding.UTF8, 
                "application/json");

            var loginResponse = await _client.PostAsync("/api/v1/auth/login", content);
            var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

            // Add the token to request headers
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", tokenResponse?.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/exchangerates/historical?baseCurrency=EUR&date=2023-01-01&symbols=USD,GBP");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
