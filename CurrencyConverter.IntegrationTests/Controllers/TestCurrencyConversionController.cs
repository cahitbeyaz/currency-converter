using System;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.IntegrationTests.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CurrencyConversionController : ControllerBase
    {
        [HttpGet("convert")]
        public ActionResult<CurrencyConversionResult> Convert(string from, string to, decimal amount)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(from) || from == "INVALID")
            {
                return BadRequest("Invalid source currency");
            }

            if (string.IsNullOrEmpty(to))
            {
                return BadRequest("Invalid target currency");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            // Create a sample response
            decimal exchangeRate = from == "EUR" && to == "USD" ? 1.2m : 0.9m;
            
            var result = new CurrencyConversionResult
            {
                FromCurrency = from,
                ToCurrency = to,
                OriginalAmount = amount,
                ExchangeRate = exchangeRate,
                ConvertedAmount = amount * exchangeRate,
                ConversionDate = DateTime.UtcNow.Date
            };

            return Ok(result);
        }
    }
}
