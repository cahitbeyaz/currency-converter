using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Entities;

namespace CurrencyConverter.Infrastructure.Http
{
    public interface IFrankfurterApiClient
    {
        Task<ExchangeRate> GetLatestRatesAsync(string baseCurrency = "EUR", List<string> symbols = null);
        Task<ExchangeRate> GetRatesByDateAsync(DateTime date, string baseCurrency = "EUR", List<string> symbols = null);
        Task<HistoricalExchangeRate> GetHistoricalRatesAsync(DateTime startDate, DateTime endDate, string baseCurrency = "EUR", List<string> symbols = null);
        Task<Dictionary<string, string>> GetAvailableCurrenciesAsync();
    }
}
