using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ExchangeRatesController : ControllerBase
    {
        private readonly ICurrencyConverterService _currencyService;
        private readonly ILogger<ExchangeRatesController> _logger;

        public ExchangeRatesController(
            ICurrencyConverterService currencyService,
            ILogger<ExchangeRatesController> logger)
        {
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the latest exchange rates for a specified base currency
        /// </summary>
        /// <param name="baseCurrency">Base currency (default: EUR)</param>
        /// <param name="symbols">Comma-separated list of target currencies</param>
        /// <returns>Latest exchange rates</returns>
        [HttpGet("latest")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<ExchangeRate>> GetLatestRates(
            [FromQuery] string baseCurrency = "EUR",
            [FromQuery] string symbols = null)
        {
            try
            {
                var targetSymbols = string.IsNullOrEmpty(symbols) 
                    ? null 
                    : symbols.Split(',').Select(s => s.Trim()).ToList();
                
                var result = await _currencyService.GetLatestExchangeRatesAsync(baseCurrency, targetSymbols);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in GetLatestRates");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLatestRates");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets historical exchange rates for a given period with pagination
        /// </summary>
        /// <param name="startDate">Start date (yyyy-MM-dd)</param>
        /// <param name="endDate">End date (yyyy-MM-dd)</param>
        /// <param name="baseCurrency">Base currency (default: EUR)</param>
        /// <param name="symbols">Comma-separated list of target currencies</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <returns>Historical exchange rates</returns>
        [HttpGet("historical")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<PaginatedResult<KeyValuePair<string, Dictionary<string, decimal>>>>> GetHistoricalRates(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string baseCurrency = "EUR",
            [FromQuery] string symbols = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var actualEndDate = endDate ?? DateTime.UtcNow;
                var targetSymbols = string.IsNullOrEmpty(symbols) 
                    ? null 
                    : symbols.Split(',').Select(s => s.Trim()).ToList();
                
                var paginationParams = new PaginationParams 
                { 
                    PageNumber = pageNumber, 
                    PageSize = pageSize 
                };
                
                var result = await _currencyService.GetHistoricalExchangeRatesAsync(
                    startDate, actualEndDate, baseCurrency, targetSymbols, paginationParams);
                
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in GetHistoricalRates");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetHistoricalRates");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }


    }
}
