using System;
using System.Collections.Generic;

namespace CurrencyConverter.Domain.Entities
{
    public class HistoricalExchangeRate
    {
        public string Base { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = new Dictionary<string, Dictionary<string, decimal>>();
    }
}
