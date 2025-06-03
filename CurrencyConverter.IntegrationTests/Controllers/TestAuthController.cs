using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CurrencyConverter.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyConverter.IntegrationTests.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public ActionResult<TokenResponse> Login(LoginRequest request)
        {
            // For testing, we'll accept a specific test user
            if (request.Username == "testuser" && request.Password == "password123")
            {
                // Valid credentials, generate token
                var token = GenerateJwtToken(request.Username);
                return Ok(new TokenResponse 
                { 
                    Token = token,
                    ExpiresIn = 3600
                });
            }

            return Unauthorized();
        }

        private string GenerateJwtToken(string username)
        {
            // For testing, use a simple hardcoded key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestSecretKeyForIntegrationTestingOnly"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User")
            };

            var token = new JwtSecurityToken(
                issuer: "TestIssuer",
                audience: "TestAudience",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
