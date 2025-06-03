using Microsoft.Extensions.Logging;
using Polly;

namespace CurrencyConverter.API.Extensions
{
    /// <summary>
    /// Extension methods for logging in resilience policies
    /// </summary>
    public static class LoggerExtensions
    {
        private const string LoggerKey = "ILogger";


        /// <summary>
        /// Gets the ILogger from the Polly context
        /// </summary>
        /// <param name="context">The Polly context</param>
        /// <returns>The logger if found, otherwise null</returns>
        public static ILogger GetLogger(this Context context)
        {
            if (context.TryGetValue(LoggerKey, out var logger))
            {
                return logger as ILogger;
            }

            return null;
        }
    }
}
