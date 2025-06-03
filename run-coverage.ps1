#!/usr/bin/env pwsh

# Output directory setup
$outputDir = "./TestResults"
$coverageReportDir = "$outputDir/CoverageReport"

# Clean previous results
Write-Host "Cleaning previous results..." -ForegroundColor Cyan
Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -Path $outputDir -ItemType Directory -Force | Out-Null

# Step 1: Run the tests with coverage collection
Write-Host "Running tests with coverage collection..." -ForegroundColor Cyan
dotnet test .\CurrencyConverter.Tests\CurrencyConverter.Tests.csproj --collect:"XPlat Code Coverage"

# Find the generated coverage file (it will be in a GUID-named subdirectory)
$coverageFile = Get-ChildItem -Path "CurrencyConverter.Tests\TestResults" -Filter "*.cobertura.xml" -Recurse | Select-Object -First 1

# Copy the coverage file to our output directory
Write-Host "Copying coverage file to $outputDir" -ForegroundColor Cyan
Copy-Item $coverageFile.FullName -Destination "$outputDir/coverage.cobertura.xml"

# Step 2: Generate the HTML report
Write-Host "Generating HTML report..." -ForegroundColor Cyan
dotnet reportgenerator "-reports:$outputDir/coverage.cobertura.xml" "-targetdir:$coverageReportDir" "-reporttypes:Html;HtmlSummary"

# Show the report location
Write-Host "Coverage report generated at: $coverageReportDir\index.html" -ForegroundColor Green
Write-Host "Open the report in your browser to view detailed coverage information." -ForegroundColor Green

# Show summary of coverage
Write-Host "\nCoverage Summary:" -ForegroundColor Yellow
$summary = Get-Content -Path "$coverageReportDir\summary.html" -Raw
$match = $summary -match '<div class="coverage-info">.*?<div>(.*?)</div>'
if ($matches) {
    Write-Host $matches[1].Trim() -ForegroundColor Green
}
