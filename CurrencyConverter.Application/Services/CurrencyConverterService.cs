using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Application.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly ICurrencyProviderFactory _providerFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyConverterService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);
        private readonly HashSet<string> _restrictedCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TRY", "PLN", "THB", "MXN"
        };

        public CurrencyConverterService(
            ICurrencyProviderFactory providerFactory,
            IMemoryCache cache,
            ILogger<CurrencyConverterService> logger)
        {
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate currencies
            if (string.IsNullOrWhiteSpace(request.FromCurrency) || string.IsNullOrWhiteSpace(request.ToCurrency))
                throw new ArgumentException("From currency and To currency must be specified");

            // Check for restricted currencies
            if (_restrictedCurrencies.Contains(request.FromCurrency) || _restrictedCurrencies.Contains(request.ToCurrency))
                throw new InvalidOperationException($"Currency conversion involving restricted currencies ({string.Join(", ", _restrictedCurrencies)}) is not allowed");

            _logger.LogInformation("Converting {Amount} {FromCurrency} to {ToCurrency}", request.Amount, request.FromCurrency, request.ToCurrency);

            var provider = _providerFactory.GetProvider();
            var date = request.Date ?? DateTime.UtcNow.Date;
            
            ExchangeRate exchangeRate;
            if (request.Date.HasValue)
            {
                var cacheKey = $"historical_{date:yyyy-MM-dd}_{request.FromCurrency}_{request.ToCurrency}";
                
                if (_cache.TryGetValue(cacheKey, out ExchangeRate cachedRates))
                {
                    _logger.LogInformation("Retrieved historical exchange rates for {BaseCurrency} to {ToCurrency} on {Date} from cache", 
                        request.FromCurrency, request.ToCurrency, date);
                    exchangeRate = cachedRates;
                }
                else
                {
                    _logger.LogInformation("Fetching historical exchange rates for {BaseCurrency} to {ToCurrency} on {Date}", 
                        request.FromCurrency, request.ToCurrency, date);
                    exchangeRate = await provider.GetExchangeRatesByDateAsync(date, request.FromCurrency, new List<string> { request.ToCurrency });
                    _cache.Set(cacheKey, exchangeRate, _cacheDuration);
                }
            }
            else
            {
                var cacheKey = $"latest_{request.FromCurrency}_{request.ToCurrency}";
                
                if (_cache.TryGetValue(cacheKey, out ExchangeRate cachedRates))
                {
                    _logger.LogInformation("Retrieved latest exchange rates for {BaseCurrency} to {ToCurrency} from cache", 
                        request.FromCurrency, request.ToCurrency);
                    exchangeRate = cachedRates;
                }
                else
                {
                    _logger.LogInformation("Fetching latest exchange rates for {BaseCurrency} to {ToCurrency}", 
                        request.FromCurrency, request.ToCurrency);
                    exchangeRate = await provider.GetLatestExchangeRatesAsync(request.FromCurrency, new List<string> { request.ToCurrency });
                    _cache.Set(cacheKey, exchangeRate, _cacheDuration);
                }
            }

            if (!exchangeRate.Rates.TryGetValue(request.ToCurrency, out var rate))
                throw new InvalidOperationException($"Exchange rate from {request.FromCurrency} to {request.ToCurrency} not available");

            var convertedAmount = request.Amount * rate;

            _logger.LogInformation("Converted {Amount} {FromCurrency} to {ConvertedAmount} {ToCurrency} at rate {Rate}", 
                request.Amount, request.FromCurrency, convertedAmount, request.ToCurrency, rate);

            return new CurrencyConversionResponse
            {
                FromCurrency = request.FromCurrency,
                ToCurrency = request.ToCurrency,
                Amount = request.Amount,
                ConvertedAmount = convertedAmount,
                ExchangeRate = rate,
                Date = exchangeRate.Date
            };
        }

        // Method removed as it was referencing non-existent API methods

        public async Task<ExchangeRate> GetLatestExchangeRatesAsync(string baseCurrency, List<string> symbols = null)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency))
                throw new ArgumentException("Base currency must be specified", nameof(baseCurrency));

            var symbolsKey = symbols == null ? "all" : string.Join("_", symbols.OrderBy(s => s));
            var cacheKey = $"latest_{baseCurrency}_{symbolsKey}";

            if (_cache.TryGetValue(cacheKey, out ExchangeRate cachedRates))
            {
                _logger.LogInformation("Retrieved latest exchange rates for {BaseCurrency} from cache", baseCurrency);
                return cachedRates;
            }

            var provider = _providerFactory.GetProvider();
            _logger.LogInformation("Fetching latest exchange rates for {BaseCurrency} from provider", baseCurrency);
            var result = await provider.GetLatestExchangeRatesAsync(baseCurrency, symbols);
            
            _cache.Set(cacheKey, result, _cacheDuration);
            return result;
        }

        public async Task<PaginatedResult<KeyValuePair<string, Dictionary<string, decimal>>>> GetHistoricalExchangeRatesAsync(
            DateTime startDate, DateTime endDate, string baseCurrency, List<string> symbols = null, PaginationParams paginationParams = null)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency))
                throw new ArgumentException("Base currency must be specified", nameof(baseCurrency));

            if (startDate > endDate)
                throw new ArgumentException("Start date must be before or equal to end date");
            
            // Use default pagination parameters if none provided
            paginationParams ??= new PaginationParams { PageNumber = 1, PageSize = 10 };

            var symbolsKey = symbols == null ? "all" : string.Join("_", symbols.OrderBy(s => s));
            var cacheKey = $"historical_period_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{baseCurrency}_{symbolsKey}_page{paginationParams.PageNumber}_size{paginationParams.PageSize}";

            if (_cache.TryGetValue(cacheKey, out PaginatedResult<KeyValuePair<string, Dictionary<string, decimal>>> cachedResults))
            {
                _logger.LogInformation("Retrieved historical rates for period {StartDate} to {EndDate} for {BaseCurrency} from cache",
                    startDate, endDate, baseCurrency);
                return cachedResults;
            }

            _logger.LogInformation("Getting historical rates for base currency {BaseCurrency} from {StartDate} to {EndDate}",
                baseCurrency, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            var provider = _providerFactory.GetProvider();
            var result = await provider.GetHistoricalExchangeRatesAsync(startDate, endDate, baseCurrency, symbols);

            // Filter out restricted currencies in each date's rates
            if (result.Rates?.Count > 0)
            {
                foreach (var dateRates in result.Rates)
                {
                    foreach (var restrictedCurrency in _restrictedCurrencies)
                    {
                        dateRates.Value.Remove(restrictedCurrency);
                    }
                }
            }

            // Apply pagination
            var data = result.Rates.OrderByDescending(x => x.Key)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            _logger.LogInformation("Retrieved {Count} days of historical rates for base currency {BaseCurrency}", result.Rates?.Count ?? 0, baseCurrency);

            // Convert from Dictionary<string, Dictionary<string, decimal>> to List<KeyValuePair<DateTime, Dictionary<string, decimal>>>
            var items = data.Select(d => new KeyValuePair<string, Dictionary<string, decimal>>(d.Key, d.Value)).ToList();

            var paginatedResult = new PaginatedResult<KeyValuePair<string, Dictionary<string, decimal>>>
            {
                Items = items,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalCount = result.Rates.Count
            };
            
            _cache.Set(cacheKey, paginatedResult, _cacheDuration);
            return paginatedResult;
        }


    }
}
