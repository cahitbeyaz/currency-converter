using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Entities;

namespace CurrencyConverter.Domain.Interfaces
{
    public interface ICurrencyProvider
    {
        Task<ExchangeRate> GetLatestExchangeRatesAsync(string baseCurrency, List<string> symbols = null);
        Task<ExchangeRate> GetExchangeRatesByDateAsync(DateTime date, string baseCurrency, List<string> symbols = null);
        Task<HistoricalExchangeRate> GetHistoricalExchangeRatesAsync(DateTime startDate, DateTime endDate, string baseCurrency, List<string> symbols = null);
        Task<Dictionary<string, string>> GetAvailableCurrenciesAsync();
    }
}
