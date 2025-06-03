using System.Threading.Tasks;
using CurrencyConverter.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        public Task<AuthResult> AuthenticateAsync(string username, string password)
        {
            // In a real application, you would validate the user credentials against a database
            // For this demo, we'll use hardcoded values
            if (username == "user" && password == "password")
            {
                var userId = "user123";
                var roles = new[] { "User" };
                var token = _tokenService.GenerateToken(userId, username, roles);

                _logger.LogInformation("User {Username} authenticated successfully", username);

                return Task.FromResult(new AuthResult 
                { 
                    Success = true,
                    Token = token,
                    UserId = userId,
                    Roles = roles
                });
            }
            else if (username == "admin" && password == "adminpassword")
            {
                var userId = "admin456";
                var roles = new[] { "User", "Admin" };
                var token = _tokenService.GenerateToken(userId, username, roles);

                _logger.LogInformation("Admin {Username} authenticated successfully", username);

                return Task.FromResult(new AuthResult 
                { 
                    Success = true,
                    Token = token,
                    UserId = userId,
                    Roles = roles
                });
            }

            _logger.LogWarning("Failed login attempt for username: {Username}", username);
            return Task.FromResult(new AuthResult 
            { 
                Success = false,
                ErrorMessage = "Invalid username or password"
            });
        }
    }
}
