using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Entities;

namespace CurrencyConverter.Application.Interfaces
{
    public interface ICurrencyConverterService
    {
        Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request);
        Task<ExchangeRate> GetLatestExchangeRatesAsync(string baseCurrency, List<string> symbols = null);
        Task<PaginatedResult<KeyValuePair<string, Dictionary<string, decimal>>>> GetHistoricalExchangeRatesAsync(
            DateTime startDate, DateTime endDate, string baseCurrency, List<string> symbols = null, PaginationParams paginationParams = null);
        Task<Dictionary<string, string>> GetAvailableCurrenciesAsync();
    }
}
