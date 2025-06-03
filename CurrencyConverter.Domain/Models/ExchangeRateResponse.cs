using System;
using System.Collections.Generic;

namespace CurrencyConverter.Domain.Models
{
    public class ExchangeRateResponse
    {
        public string BaseCurrency { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
    }
}
