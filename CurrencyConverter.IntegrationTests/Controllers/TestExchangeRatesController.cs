using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.IntegrationTests.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExchangeRatesController : ControllerBase
    {
        [HttpGet("latest")]
        public ActionResult<ExchangeRate> GetLatestRates(string baseCurrency = "EUR", string symbols = null)
        {
            if (string.IsNullOrEmpty(baseCurrency) || baseCurrency == "INVALID")
            {
                return BadRequest("Invalid base currency");
            }

            // Create a sample response that mimics the real API
            var rates = new Dictionary<string, decimal>();
            
            if (symbols != null)
            {
                var symbolList = symbols.Split(',');
                foreach (var symbol in symbolList)
                {
                    rates.Add(symbol.Trim(), 1.2m); // Dummy rate
                }
            }
            else
            {
                rates.Add("USD", 1.2m);
                rates.Add("GBP", 0.9m);
            }

            var result = new ExchangeRate
            {
                Base = baseCurrency,
                Date = DateTime.UtcNow.Date,
                Rates = rates
            };

            return Ok(result);
        }

        [Authorize]
        [HttpGet("historical")]
        public ActionResult<ExchangeRate> GetHistoricalRates(string baseCurrency, DateTime date, string symbols = null)
        {
            // Similar implementation to GetLatestRates but with the specified date
            if (string.IsNullOrEmpty(baseCurrency))
            {
                return BadRequest("Base currency is required");
            }

            var rates = new Dictionary<string, decimal>();
            
            if (symbols != null)
            {
                var symbolList = symbols.Split(',');
                foreach (var symbol in symbolList)
                {
                    rates.Add(symbol.Trim(), 1.1m); // Historical dummy rate
                }
            }
            else
            {
                rates.Add("USD", 1.1m);
                rates.Add("GBP", 0.85m);
            }

            var result = new ExchangeRate
            {
                Base = baseCurrency,
                Date = date.Date,
                Rates = rates
            };

            return Ok(result);
        }
    }
}
