using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Infrastructure.Http;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Infrastructure.Services
{
    public class FrankfurterCurrencyProvider : ICurrencyProvider
    {
        private readonly IFrankfurterApiClient _apiClient;
        private readonly ILogger<FrankfurterCurrencyProvider> _logger;

        public FrankfurterCurrencyProvider(
            IFrankfurterApiClient apiClient,
            ILogger<FrankfurterCurrencyProvider> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ExchangeRate> GetLatestExchangeRatesAsync(string baseCurrency, List<string> symbols = null)
        {
            _logger.LogInformation("Fetching latest exchange rates for {BaseCurrency} from API", baseCurrency);
            return await _apiClient.GetLatestRatesAsync(baseCurrency, symbols);
        }

        public async Task<ExchangeRate> GetExchangeRatesByDateAsync(DateTime date, string baseCurrency, List<string> symbols = null)
        {
            _logger.LogInformation("Fetching historical exchange rates for {BaseCurrency} on {Date} from API", baseCurrency, date);
            // Pass parameters in correct order to match API client method signature
            return await _apiClient.GetRatesByDateAsync(date, baseCurrency, symbols);
        }

        public async Task<HistoricalExchangeRate> GetHistoricalExchangeRatesAsync(DateTime startDate, DateTime endDate, string baseCurrency, List<string> symbols = null)
        {
            _logger.LogInformation("Fetching historical exchange rates for {BaseCurrency} from {StartDate} to {EndDate}", 
                baseCurrency, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
                
            // Making sure parameter order matches the expected signature in FrankfurterApiClient
            return await _apiClient.GetHistoricalRatesAsync(startDate, endDate, baseCurrency, symbols);
        }


    }
}
