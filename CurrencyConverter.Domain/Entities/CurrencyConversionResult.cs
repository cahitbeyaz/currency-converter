using System;

namespace CurrencyConverter.Domain.Entities
{
    public class CurrencyConversionResult
    {
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime ConversionDate { get; set; }
    }
}
