# Currency Converter API

A robust, scalable, and maintainable currency conversion API using C# and ASP.NET Core, ensuring high performance, security, and resilience.

## Features

- **API Endpoints**
  - Retrieve latest exchange rates for a specific base currency
  - Convert amounts between different currencies
  - Retrieve historical exchange rates with pagination
  - Get available currencies

- **Architecture & Design**
  - Clean architecture with separation of concerns
  - Factory pattern for currency provider selection
  - Dependency injection for all services
  - Extensible design for adding new currency providers

- **Resilience & Performance**
  - In-memory caching to minimize external API calls
  - Retry policies with exponential backoff
  - Circuit breaker for handling API outages
  - Rate limiting to prevent abuse

- **Security**
  - JWT authentication for API endpoints
  - Role-based access control (RBAC)
  - API throttling

- **Logging & Monitoring**
  - Structured logging with Serilog
  - Request/response logging with correlation
  - OpenTelemetry for distributed tracing

## Project Structure

- **CurrencyConverter.API**: API controllers, middleware, authentication
- **CurrencyConverter.Core**: Domain models, interfaces, business logic
- **CurrencyConverter.Infrastructure**: External services, data access, caching

## Setup Instructions

### Prerequisites

- .NET 7.0 or later
- Visual Studio 2022 or other compatible IDE
- Git

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/CurrencyConverter.git
   ```

2. Navigate to the project directory:
   ```
   cd CurrencyConverter
   ```

3. Restore dependencies:
   ```
   dotnet restore
   ```

4. Build the solution:
   ```
   dotnet build
   ```

5. Update JWT Secret in appsettings.json:
   ```json
   "JwtSettings": {
     "Secret": "YourSuperSecretKeyWithAtLeast32Characters",
     "Issuer": "CurrencyConverterAPI",
     "Audience": "CurrencyConverterAPIClients",
     "ExpirationInMinutes": 60
   }
   ```

6. Run the application:
   ```
   dotnet run --project CurrencyConverter.API
   ```

7. Access the Swagger UI:
   ```
   https://localhost:5001/swagger
   ```

## API Usage

### Authentication

1. Use the `/api/v1/auth/login` endpoint to get a JWT token:
   ```
   POST /api/v1/auth/login
   {
     "username": "user",
     "password": "password"
   }
   ```

2. Include the token in the Authorization header:
   ```
   Authorization: Bearer <your-token>
   ```

### Example Requests

1. Get latest exchange rates:
   ```
   GET /api/v1/exchangerates/latest?baseCurrency=USD&symbols=EUR,GBP,JPY
   ```

2. Convert currency:
   ```
   POST /api/v1/currencyconversion/convert
   {
     "fromCurrency": "USD",
     "toCurrency": "EUR",
     "amount": 100
   }
   ```

3. Get historical rates with pagination:
   ```
   GET /api/v1/exchangerates/historical?startDate=2023-01-01&endDate=2023-01-31&baseCurrency=USD&symbols=EUR,GBP&pageNumber=1&pageSize=10
   ```

## Key Design Decisions

1. **Currency Provider Factory**: Enables dynamic selection of currency providers, allowing for future integration with multiple exchange rate providers beyond Frankfurter.

2. **Caching Strategy**: Implemented in-memory caching to reduce calls to external APIs, with configurable expiration times.

3. **Resilience Patterns**: Used Polly for retry and circuit breaker patterns to handle transient failures and prevent cascading failures.

4. **RBAC Implementation**: Different endpoints require different roles, with some endpoints being more restricted than others.

5. **Restricted Currencies**: Implementation of business rules to exclude specific currencies (TRY, PLN, THB, MXN).

## Assumptions

1. The Frankfurter API is the primary data source for exchange rates.
2. Rate limits are applied per IP address.
3. JWT tokens are short-lived (60 minutes) for security.
4. For demonstration purposes, user credentials are hardcoded.

## Future Enhancements

1. **Additional Providers**: Add more currency data providers (e.g., OpenExchangeRates, Fixer.io).
2. **Database Integration**: Store historical exchange rates in a database for better performance.
3. **User Management**: Add proper user registration and management system.
4. **Rate Limit by User**: Implement user-based rate limiting instead of just IP-based.
5. **Caching Provider**: Replace in-memory cache with Redis for distributed caching.
6. **GraphQL API**: Add GraphQL endpoint for more flexible data querying.
7. **Containerization**: Add Docker support for easier deployment.
8. **CI/CD Pipeline**: Implement automated testing and deployment pipeline.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
