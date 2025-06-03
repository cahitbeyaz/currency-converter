# Currency Converter API

A robust, scalable, and maintainable currency conversion API built with .NET 9, implementing industry best practices for performance, security, and resilience.

## Features

### API Endpoints

- **Exchange Rates**: Get the latest exchange rates for any base currency
- **Currency Conversion**: Convert amounts between different currencies with validation
- **Historical Data**: Access historical exchange rates with advanced pagination
- **Currency Information**: Retrieve available currencies and metadata

### Architecture

- **Clean Architecture** with distinct layers:
  - `CurrencyConverter.API`: Controllers, middleware, and API configuration
  - `CurrencyConverter.Application`: Business logic and services
  - `CurrencyConverter.Domain`: Core entities and interfaces
  - `CurrencyConverter.Infrastructure`: External services integration and data access
  - `CurrencyConverter.Tests`: Comprehensive test suite

- **Design Patterns**:
  - Factory pattern for dynamic currency provider selection
  - Repository pattern for data access abstraction
  - Decorator pattern for cross-cutting concerns

### Resilience & Performance

- **Caching Strategy**: In-memory caching to minimize external API calls
- **Retry Policies**: Exponential backoff for handling transient failures
- **Circuit Breaker**: Graceful degradation during downstream service outages
- **Rate Limiting**: Prevents API abuse and ensures fair usage

### Security

- **JWT Authentication**: Secure token-based authentication
- **Role-Based Access Control**: Fine-grained permission management
- **Request Validation**: Input sanitization and validation
- **API Throttling**: Protection against brute force and DoS attacks

### Observability

- **Structured Logging**: Detailed request/response logs with correlation IDs
- **Request Tracing**: Complete visibility into API call chains
- **Performance Metrics**: Monitoring of response times and system health

## Project Structure

```
CurrencyConverter2/
├── CurrencyConverter.API/             # API layer
│   ├── Controllers/                   # API endpoints
│   ├── Extensions/                    # Service registration and middleware
│   ├── Middleware/                    # Custom middleware components
│   └── Program.cs                     # Application entry point and configuration
├── CurrencyConverter.Application/     # Application layer
│   ├── Interfaces/                    # Service contracts
│   └── Services/                      # Service implementations
├── CurrencyConverter.Domain/          # Domain layer
│   └── Models/                        # Domain entities
├── CurrencyConverter.Infrastructure/  # Infrastructure layer
│   ├── Http/                          # External API clients
│   └── Services/                      # Infrastructure service implementations
├── CurrencyConverter.Tests/           # Test projects
│   ├── Controllers/                   # Controller tests
│   ├── Extensions/                    # Extension method tests
│   ├── Infrastructure/                # Infrastructure tests
│   └── Services/                      # Service tests
└── TestResults/                       # Generated test results and coverage reports
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or compatible IDE (optional)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/CurrencyConverter.git
   cd CurrencyConverter
   ```

2. Restore dependencies and build:
   ```bash
   dotnet restore
   dotnet build
   ```

3. Configure the application settings in `appsettings.json`:
   ```json
   {
     "JwtSettings": {
       "Secret": "your-strong-secret-key-at-least-32-chars",
       "Issuer": "CurrencyConverterAPI",
       "Audience": "ApiClients",
       "ExpirationInMinutes": 60
     },
     "ApiRateLimits": {
       "PerSecond": 10,
       "PerDay": 10000
     },
     "CurrencyProviders": {
       "Default": "frankfurter",
       "Providers": [
         {
           "Name": "frankfurter",
           "BaseUrl": "https://api.frankfurter.app"
         }
       ]
     }
   }
   ```

4. Run the application:
   ```bash
   dotnet run --project CurrencyConverter.API
   ```

5. The API will be available at:
   ```
   https://localhost:5001
   ```

## API Usage

### Authentication

To access protected endpoints, first obtain a JWT token:

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password123"
}
```

Use the returned token in subsequent requests:

```http
GET /api/currency/latest?base=USD
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Available Endpoints

#### Get Latest Exchange Rates

```http
GET /api/exchangerates/latest?base=EUR
```

#### Convert Currency

```http
GET /api/currency/convert?from=USD&to=EUR&amount=100
```

#### Get Historical Exchange Rates

```http
GET /api/exchangerates/history?base=EUR&start=2023-01-01&end=2023-01-31&page=1&pageSize=10
```

## Testing

### Running Tests

To run the unit tests:

```bash
dotnet test
```

### Code Coverage

This project uses Coverlet and ReportGenerator for code coverage analysis:

1. Ensure you have the required tools:

```bash
dotnet tool restore
```

2. Generate a coverage report:

```bash
# Windows PowerShell
.\run-coverage.ps1

# Linux/macOS
./run-coverage.ps1
```

3. View the HTML report at `TestResults/CoverageReport/index.html`

## Deployment

### Environment Configuration

The application supports multiple deployment environments:

- **Development**: Local development with debugging
- **Staging**: Pre-production testing environment
- **Production**: Live environment with optimized settings

Environment-specific settings can be configured using environment variables or environment-specific appsettings files.

### Containerization

A Dockerfile is provided for containerized deployments:

```bash
# Build the container
docker build -t currency-converter-api -f .\CurrencyConverter.API\Dockerfile .

# Run the container
docker run -p 80:8080 currency-converter-api
```

## Assumptions and Design Decisions

- The API uses Frankfurter as the default currency data provider, but the architecture supports adding additional providers.
- Certain currencies (TRY, PLN, THB, MXN) are restricted as per business requirements.
- JWT authentication is implemented with role-based permissions to secure endpoints.
- In-memory caching is used to optimize performance and reduce external API calls.
- Structured logging captures client IP, client ID, request details, and response metrics.

## Future Enhancements

- Integration with additional currency data providers
- Real-time currency updates using WebSockets
- Enhanced analytics dashboard for monitoring API usage
- AI-powered currency trend predictions
