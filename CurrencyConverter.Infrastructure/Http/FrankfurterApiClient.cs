using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CurrencyConverter.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Infrastructure.Http
{
    public class FrankfurterApiClient : IFrankfurterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrankfurterApiClient> _logger;

        public FrankfurterApiClient(HttpClient httpClient, ILogger<FrankfurterApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Ensure the base address is set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.frankfurter.app/");
            }
        }

        public async Task<ExchangeRate> GetLatestRatesAsync(string baseCurrency = "EUR", List<string> symbols = null)
        {
            try
            {
                string requestUri = "latest";
                requestUri = AddQueryParameters(requestUri, baseCurrency, symbols);
                
                _logger.LogInformation("Requesting latest exchange rates from Frankfurter API: {RequestUri}", requestUri);
                var response = await _httpClient.GetFromJsonAsync<ExchangeRate>(requestUri);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest exchange rates from Frankfurter API");
                throw;
            }
        }

        public async Task<ExchangeRate> GetRatesByDateAsync(DateTime date, string baseCurrency = "EUR", List<string> symbols = null)
        {
            try
            {
                string requestUri = date.ToString("yyyy-MM-dd");
                requestUri = AddQueryParameters(requestUri, baseCurrency, symbols);
                
                _logger.LogInformation("Requesting exchange rates for date {Date} from Frankfurter API: {RequestUri}", date, requestUri);
                var response = await _httpClient.GetFromJsonAsync<ExchangeRate>(requestUri);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rates for date {Date} from Frankfurter API", date);
                throw;
            }
        }

        public async Task<HistoricalExchangeRate> GetHistoricalRatesAsync(DateTime startDate, DateTime endDate, string baseCurrency = "EUR", List<string> symbols = null)
        {
            try
            {
                string requestUri = $"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}";
                requestUri = AddQueryParameters(requestUri, baseCurrency, symbols);
                
                _logger.LogInformation("Requesting historical exchange rates from {StartDate} to {EndDate} from Frankfurter API: {RequestUri}", 
                    startDate, endDate, requestUri);
                var response = await _httpClient.GetFromJsonAsync<HistoricalExchangeRate>(requestUri);
                
                // Ensure start and end dates are properly set, as they might not be deserialized from the API response
                response.StartDate = startDate;
                response.EndDate = endDate;
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching historical exchange rates from {StartDate} to {EndDate} from Frankfurter API", 
                    startDate, endDate);
                throw;
            }
        }



        private string AddQueryParameters(string requestUri, string baseCurrency, List<string> symbols)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(baseCurrency) && baseCurrency != "EUR")
            {
                queryParams.Add($"base={baseCurrency}");
            }
            
            if (symbols != null && symbols.Count > 0)
            {
                queryParams.Add($"symbols={string.Join(",", symbols)}");
            }
            
            if (queryParams.Count > 0)
            {
                requestUri += $"?{string.Join("&", queryParams)}";
            }
            
            return requestUri;
        }
    }
}
