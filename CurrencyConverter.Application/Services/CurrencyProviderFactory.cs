using System;
using System.Collections.Generic;
using CurrencyConverter.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Application.Services
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly Dictionary<string, Func<ICurrencyProvider>> _providers;
        private readonly ILogger<CurrencyProviderFactory> _logger;
        private readonly string _defaultProvider;

        public CurrencyProviderFactory(
            IEnumerable<ICurrencyProvider> providers,
            IConfiguration configuration,
            ILogger<CurrencyProviderFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _providers = new Dictionary<string, Func<ICurrencyProvider>>(StringComparer.OrdinalIgnoreCase);
            _defaultProvider = configuration["DefaultCurrencyProvider"] ?? "Frankfurter";

            // Register providers by type name
            foreach (var provider in providers)
            {
                var providerType = provider.GetType();
                var providerName = providerType.Name.Replace("CurrencyProvider", "", StringComparison.OrdinalIgnoreCase);

                _providers[providerName] = () => provider;
                _logger.LogInformation("Registered currency provider: {ProviderName}", providerName);
            }
        }

        public ICurrencyProvider GetProvider(string providerName = null)
        {
            // If no provider name is specified, use the default provider from configuration
            providerName ??= _defaultProvider;

            if (_providers.TryGetValue(providerName, out var providerFactory))
            {
                _logger.LogInformation("Using currency provider: {ProviderName}", providerName);
                return providerFactory();
            }

            _logger.LogWarning("Currency provider '{ProviderName}' not found, using default provider '{DefaultProvider}'",
                providerName, _defaultProvider);

            // If the default provider is not found, throw an exception
            if (!_providers.TryGetValue(_defaultProvider, out var defaultProviderFactory))
            {
                throw new InvalidOperationException($"Default currency provider '{_defaultProvider}' is not registered.");
            }

            return defaultProviderFactory();
        }
    }
}
