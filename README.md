# Currency Converter API

A robust, scalable, and maintainable currency conversion API built with .NET 9, implementing industry best practices for performance, security, and resilience.

## Features

### API Endpoints

- **Exchange Rates**: Get the latest exchange rates for any base currency
- **Currency Conversion**: Convert amounts between different currencies with validation
  - Business validation to exclude specific currencies (TRY, PLN, THB, MXN) with appropriate error responses
- **Historical Data**: Access historical exchange rates with advanced pagination
- **Currency Information**: Retrieve available currencies and metadata
- **API Versioning**: All endpoints support API versioning for future-proofing

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

- **Caching Strategy**: In-memory caching to minimize direct calls to the Frankfurter API
- **Retry Policies**: Exponential backoff for handling transient failures (3 retries with increasing delays)
- **Circuit Breaker**: Graceful degradation during downstream service outages
- **Rate Limiting**: Configurable per-endpoint and global limits to prevent API abuse
- **Provider Factory Pattern**: Dynamic selection of currency providers based on configuration

### Security

- **JWT Authentication**: Secure token-based authentication with configurable settings
- **Role-Based Access Control**: Fine-grained permission management for protected endpoints
- **Request Validation**: Input sanitization and validation for all API inputs
- **API Throttling**: Protection against brute force and DoS attacks using IP-based rate limiting
- **Environment-Specific Secrets**: JWT secrets managed via environment variables for security

### Observability

- **Structured Logging**: Detailed request/response logs including:
  - Client IP address
  - ClientId from JWT token
  - HTTP method and target endpoint
  - Response code and response time
- **Correlation IDs**: Request correlation between client calls and external API calls
- **Request Tracing**: Complete visibility into API call chains using OpenTelemetry
- **Performance Metrics**: Monitoring of response times and system health
- **Environment-Specific Log Levels**: Configurable logging based on environment

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
- Docker and Docker Compose (for containerized deployment)

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

### Environment Configuration

The application supports multiple environments with environment-specific settings:

- **Development**: Enhanced debugging and detailed logs (appsettings.Development.json)
- **Staging**: Testing environment with moderate logging (appsettings.Staging.json)
- **Production**: Optimized for performance with minimal logging (appsettings.json)

Each environment has appropriate settings for:
- Logging levels
- JWT authentication configuration
- Rate limiting rules
- Cache expiration times

### Running Locally

1. Set the environment:
   ```bash
   # Windows
   $env:ASPNETCORE_ENVIRONMENT="Development"
   
   # Linux/macOS
   export ASPNETCORE_ENVIRONMENT=Development
   ```

2. Run the application:
   ```bash
   dotnet run --project CurrencyConverter.API
   ```

3. Access the API at `https://localhost:5001` or via Swagger at `https://localhost:5001/swagger`

### Docker Deployment

The application is containerized and supports horizontal scaling with NGINX load balancing:

1. Run with Docker Compose:
   ```bash
   # Using default JWT secret
   docker-compose up -d
   
   # Using custom JWT secret (recommended for production)
   JWT_SECRET=your_secure_secret docker-compose up -d
   ```

2. Scale the API horizontally:
   ```bash
   # Scale to 5 instances
   docker-compose up -d --scale api=5
   ```

3. Access the load-balanced API at `http://localhost:80`

### Autoscaling with Docker Swarm

For production deployments with autoscaling, the application can be deployed to Docker Swarm:

1. Initialize Docker Swarm:
   ```bash
   docker swarm init
   ```

2. Deploy the stack with autoscaling:
   ```bash
   # Deploy the stack
   docker stack deploy -c docker-compose.yml currencyconverter
   ```

3. Configure autoscaling rules (requires additional monitoring):
   ```bash
   # Example autoscaling command with external tools
   docker service update --replicas-max 5 --replicas-min 2 currencyconverter_api
   ```

> Note: For production-grade autoscaling, consider using Kubernetes with Horizontal Pod Autoscaler (HPA) which can scale based on CPU/memory metrics.

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
