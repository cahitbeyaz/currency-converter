using System;

namespace CurrencyConverter.Domain.Entities
{
    public class CurrencyConversionResponse
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime Date { get; set; }
    }
}
