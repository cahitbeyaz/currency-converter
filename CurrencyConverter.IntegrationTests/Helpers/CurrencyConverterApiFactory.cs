using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CurrencyConverter.IntegrationTests.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CurrencyConverter.IntegrationTests.Helpers
{
    public class CurrencyConverterApiFactory : IDisposable
    {
        private readonly TestServer _testServer;
        public HttpClient Client { get; }

        public CurrencyConverterApiFactory()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.ConfigureAppConfiguration((context, config) =>
                    {
                        // Add test-specific configurations
                        config.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            // Set default currency provider to Frankfurter for tests
                            { "DefaultCurrencyProvider", "Frankfurter" }
                        });
                    });

                    webHost.ConfigureServices(services =>
                    {
                        // Add essential services for testing
                        services.AddControllers()
                            .AddApplicationPart(typeof(ExchangeRatesController).Assembly);
                        services.AddEndpointsApiExplorer();
                        
                        // Configure authentication for testing
                        services.AddAuthentication("Test")
                            .AddScheme<TestAuthHandlerOptions, TestAuthHandler>("Test", options => { });
                            
                        // Add required services
                        services.AddLogging(builder => builder.AddConsole());
                        
                        // Configure rate limiting for testing
                        services.AddMemoryCache();
                    });

                    webHost.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints => endpoints.MapControllers());
                    });
                });

            var host = hostBuilder.Start();
            _testServer = host.GetTestServer();
            Client = _testServer.CreateClient();
        }

        public void Dispose()
        {
            Client.Dispose();
            _testServer.Dispose();
        }
    }
}
