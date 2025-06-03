#!/usr/bin/env pwsh

# Output directory setup
$outputDir = "./TestResults"
$coverageReportDir = "$outputDir/CoverageReport"

# Clean previous results
Write-Host "Cleaning previous results..." -ForegroundColor Cyan
Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -Path $outputDir -ItemType Directory -Force | Out-Null

# Step 1: Run the tests with coverlet
Write-Host "Running tests with coverage collection..." -ForegroundColor Cyan
coverlet ".\CurrencyConverter.Tests\bin\Debug\net9.0\CurrencyConverter.Tests.dll" `
  --target "dotnet" `
  --targetargs "test .\CurrencyConverter.Tests\CurrencyConverter.Tests.csproj --no-build" `
  --format "cobertura" `
  --output "$outputDir/coverage.cobertura.xml" `
  --include "[CurrencyConverter.API]*" `
  --include "[CurrencyConverter.Application]*" `
  --include "[CurrencyConverter.Domain]*" `
  --include "[CurrencyConverter.Infrastructure]*" `
  --exclude "[*]*.Program" `
  --exclude "[*]*.Startup" `
  --exclude "[CurrencyConverter.Tests]*"

# Check if tests passed
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Generate the HTML report
Write-Host "Generating HTML report..." -ForegroundColor Cyan
reportgenerator `
  "-reports:$outputDir/coverage.cobertura.xml" `
  "-targetdir:$coverageReportDir" `
  "-reporttypes:Html;HtmlSummary;Cobertura"

# Show the report location
Write-Host "Coverage report generated at: $(Resolve-Path $coverageReportDir/index.html)" -ForegroundColor Green
Write-Host "Open the report in your browser to view detailed coverage information." -ForegroundColor Green
