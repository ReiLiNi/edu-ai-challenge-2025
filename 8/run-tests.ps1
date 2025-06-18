#!/usr/bin/env pwsh

param(
    [switch]$Coverage,
    [switch]$Benchmarks,
    [switch]$Html,
    [string]$Filter = "*"
)

Write-Host "🧪 Validation Framework Test Runner" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Restore packages
Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Run unit tests
Write-Host "🔬 Running unit tests..." -ForegroundColor Yellow

if ($Coverage) {
    Write-Host "📊 Running tests with coverage analysis..." -ForegroundColor Green
    
    # Run tests with coverage collection
    dotnet test --collect:"XPlat Code Coverage" --filter $Filter --logger "console;verbosity=detailed"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tests failed" -ForegroundColor Red
        exit 1
    }
    
    # Find the coverage file
    $coverageFile = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1
    
    if ($coverageFile) {
        Write-Host "📈 Generating coverage report..." -ForegroundColor Green
        
        # Generate HTML report
        dotnet tool install --global dotnet-reportgenerator-globaltool --ignore-failed-sources 2>$null
        reportgenerator -reports:$coverageFile.FullName -targetdir:CoverageReport -reporttypes:"Html;HtmlSummary;Badges;TextSummary"
        
        # Display coverage summary
        if (Test-Path "CoverageReport/Summary.txt") {
            Write-Host "📋 Coverage Summary:" -ForegroundColor Cyan
            Get-Content "CoverageReport/Summary.txt"
        }
        
        if ($Html) {
            Write-Host "🌐 Opening coverage report in browser..." -ForegroundColor Green
            Start-Process "CoverageReport/index.html"
        }
        
        Write-Host "✅ Coverage report generated: CoverageReport/index.html" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Coverage file not found" -ForegroundColor Yellow
    }
} else {
    # Run tests without coverage
    dotnet test --filter $Filter --logger "console;verbosity=detailed"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tests failed" -ForegroundColor Red
        exit 1
    }
}

# Run benchmarks if requested
if ($Benchmarks) {
    Write-Host "⚡ Running performance benchmarks..." -ForegroundColor Yellow
    dotnet run --configuration Release --framework net8.0 -- benchmarks
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️  Benchmarks completed with warnings" -ForegroundColor Yellow
    } else {
        Write-Host "✅ Benchmarks completed successfully" -ForegroundColor Green
    }
}

Write-Host "🎉 Test execution completed!" -ForegroundColor Green

# Display summary
Write-Host "`n📊 Test Summary:" -ForegroundColor Cyan
Write-Host "===============" -ForegroundColor Cyan

if ($Coverage) {
    Write-Host "✅ Unit tests with coverage analysis completed" -ForegroundColor Green
    Write-Host "📁 Coverage report: CoverageReport/index.html" -ForegroundColor White
} else {
    Write-Host "✅ Unit tests completed" -ForegroundColor Green
}

if ($Benchmarks) {
    Write-Host "⚡ Performance benchmarks completed" -ForegroundColor Green
    Write-Host "📁 Benchmark results: BenchmarkDotNet.Artifacts/" -ForegroundColor White
}

Write-Host "`n💡 Usage examples:" -ForegroundColor Cyan
Write-Host "  ./run-tests.ps1                    # Run all tests" -ForegroundColor White
Write-Host "  ./run-tests.ps1 -Coverage          # Run tests with coverage" -ForegroundColor White
Write-Host "  ./run-tests.ps1 -Coverage -Html    # Run tests with coverage and open report" -ForegroundColor White
Write-Host "  ./run-tests.ps1 -Benchmarks        # Run performance benchmarks" -ForegroundColor White
Write-Host "  ./run-tests.ps1 -Filter '*String*' # Run only string-related tests" -ForegroundColor White 