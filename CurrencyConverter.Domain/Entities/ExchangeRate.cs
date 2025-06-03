using System;
using System.Collections.Generic;

namespace CurrencyConverter.Domain.Entities
{
    public class ExchangeRate
    {
        public string Base { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
    }
}
