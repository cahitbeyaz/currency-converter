using AspNetCoreRateLimit;
using CurrencyConverter.API.Extensions;
using CurrencyConverter.API.Middleware;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Infrastructure.Http;
using CurrencyConverter.Infrastructure.Services;
using CurrencyConverter.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Lower minimum level to see more logs
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // Show more Microsoft logs
    .MinimumLevel.Override("System", LogEventLevel.Information) // Show more System logs
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        restrictedToMinimumLevel: LogEventLevel.Debug, // Ensure debug logs go to console
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/currency-converter-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Configure JWT Authentication
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? "defaultsecretkey123456789012345678901234")),
        ClockSkew = TimeSpan.Zero
    };
});

// Add rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Currency Converter API",
        Version = "v1",
        Description = "A robust, scalable, and maintainable currency conversion API using ASP.NET Core"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure HTTP Clients with Polly for resilience
builder.Services.AddHttpClient<FrankfurterApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.app/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "CurrencyConverter-API");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("CurrencyConverter.API")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("CurrencyConverter.API"))
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();
    });

// Register Infrastructure services
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddMemoryCache();

// Register Application services
builder.Services.AddScoped<ICurrencyProviderFactory, CurrencyConverter.Application.Services.CurrencyProviderFactory>();
builder.Services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();

// Register Infrastructure services
builder.Services.AddScoped<ICurrencyProvider, FrankfurterCurrencyProvider>();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Converter API v1"));
}

app.UseHttpsRedirection();

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Use rate limiting
app.UseIpRateLimiting();

// Add custom request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure default route
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

// Helper methods for HTTP resilience policies
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                context.GetLogger()?.LogWarning("Delaying for {RetryTime}ms, then making retry {RetryCount}",
                    timespan.TotalMilliseconds, retryAttempt);
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1),
            onBreak: (outcome, timespan, context) =>
            {
                context.GetLogger()?.LogWarning("Circuit tripped! Circuit is now open and requests won't be made for {BreakTime}ms",
                    timespan.TotalMilliseconds);
            },
            onReset: context =>
            {
                context.GetLogger()?.LogInformation("Circuit reset! Requests are now allowed again");
            });
}
