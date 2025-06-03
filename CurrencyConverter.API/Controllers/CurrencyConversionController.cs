using System;
using System.Threading.Tasks;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CurrencyConversionController : ControllerBase
    {
        private readonly ICurrencyConverterService _currencyService;
        private readonly ILogger<CurrencyConversionController> _logger;

        public CurrencyConversionController(
            ICurrencyConverterService currencyService,
            ILogger<CurrencyConversionController> logger)
        {
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Converts an amount from one currency to another
        /// </summary>
        /// <param name="request">Currency conversion request</param>
        /// <returns>Conversion result</returns>
        [HttpPost("convert")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<CurrencyConversionResponse>> ConvertCurrency([FromBody] CurrencyConversionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request body cannot be null" });
                }

                var result = await _currencyService.ConvertCurrencyAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in ConvertCurrency");
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in ConvertCurrency");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConvertCurrency");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Simple conversion with query parameters
        /// </summary>
        /// <param name="from">Source currency</param>
        /// <param name="to">Target currency</param>
        /// <param name="amount">Amount to convert</param>
        /// <param name="date">Optional date for historical conversion (yyyy-MM-dd)</param>
        /// <returns>Conversion result</returns>
        [HttpGet("convert")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<CurrencyConversionResponse>> ConvertCurrencyGet(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] decimal amount,
            [FromQuery] DateTime? date = null)
        {
            try
            {
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                {
                    return BadRequest(new { error = "Source and target currencies must be specified" });
                }

                if (amount <= 0)
                {
                    return BadRequest(new { error = "Amount must be greater than zero" });
                }

                var request = new CurrencyConversionRequest
                {
                    FromCurrency = from,
                    ToCurrency = to,
                    Amount = amount,
                    Date = date
                };

                var result = await _currencyService.ConvertCurrencyAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in ConvertCurrencyGet");
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in ConvertCurrencyGet");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConvertCurrencyGet");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }
    }
}
