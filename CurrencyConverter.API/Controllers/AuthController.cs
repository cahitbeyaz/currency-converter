using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="request">Login request</param>
        /// <returns>Authentication token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
        {
            // Validate request
            if (request == null)
            {
                return BadRequest(new { error = "Login request cannot be null" });
            }
            
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { error = "Username cannot be empty" });
            }
            
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Password cannot be empty" });
            }
            
            var result = await _authService.AuthenticateAsync(request.Username, request.Password);
            
            if (result.Success)
            {
                return Ok(new TokenResponse { Token = result.Token });
            }
            
            return Unauthorized(new { error = result.ErrorMessage });
        }
    }
}
