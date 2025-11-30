#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs all tests for the Massage Booking Bot solution.

.DESCRIPTION
    This script runs both backend (.NET) and frontend (Angular) tests,
    generates coverage reports, and provides a summary of results.

.PARAMETER SkipBackend
    Skip backend tests

.PARAMETER SkipFrontend
    Skip frontend tests

.PARAMETER Coverage
    Generate code coverage reports

.PARAMETER CI
    Run in CI mode (non-interactive, fail fast)

.EXAMPLE
    .\run-tests.ps1
    Runs all tests

.EXAMPLE
    .\run-tests.ps1 -Coverage
    Runs all tests with coverage reports

.EXAMPLE
    .\run-tests.ps1 -SkipFrontend
    Runs only backend tests
#>

param(
    [switch]$SkipBackend,
    [switch]$SkipFrontend,
    [switch]$Coverage,
    [switch]$CI
)

$ErrorActionPreference = "Stop"
$startTime = Get-Date

Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host "  Massage Booking Bot - Test Runner" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

$backendSuccess = $false
$frontendSuccess = $false

# Backend Tests
if (-not $SkipBackend) {
    Write-Host "`n[1/2] Running Backend Tests..." -ForegroundColor Yellow
    Write-Host "================================================" -ForegroundColor Gray
    
    try {
        if ($Coverage) {
            Write-Host "Running tests with code coverage..." -ForegroundColor Gray
            dotnet test --configuration Release `
                --collect:"XPlat Code Coverage" `
                --results-directory:./TestResults `
                --logger:"console;verbosity=normal"
        } else {
            dotnet test --configuration Release `
                --logger:"console;verbosity=normal"
        }
        
        if ($LASTEXITCODE -eq 0) {
            $backendSuccess = $true
            Write-Host "`n✓ Backend tests PASSED" -ForegroundColor Green
            
            if ($Coverage) {
                Write-Host "`nGenerating coverage report..." -ForegroundColor Gray
                
                # Check if reportgenerator is installed
                if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
                    Write-Host "Installing ReportGenerator..." -ForegroundColor Gray
                    dotnet tool install --global dotnet-reportgenerator-globaltool
                }
                
                $coverageFiles = Get-ChildItem -Path ./TestResults -Recurse -Filter "coverage.cobertura.xml"
                if ($coverageFiles.Count -gt 0) {
                    reportgenerator `
                        -reports:"**/coverage.cobertura.xml" `
                        -targetdir:"./TestResults/CoverageReport" `
                        -reporttypes:"Html;TextSummary"
                    
                    Write-Host "Coverage report generated at: ./TestResults/CoverageReport/index.html" -ForegroundColor Cyan
                    
                    if (-not $CI) {
                        $openReport = Read-Host "`nOpen coverage report? (y/n)"
                        if ($openReport -eq 'y') {
                            Start-Process "./TestResults/CoverageReport/index.html"
                        }
                    }
                }
            }
        } else {
            throw "Backend tests failed with exit code $LASTEXITCODE"
        }
    }
    catch {
        Write-Host "`n✗ Backend tests FAILED" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        
        if ($CI) {
            exit 1
        }
    }
}

# Frontend Tests
if (-not $SkipFrontend) {
    Write-Host "`n[2/2] Running Frontend Tests..." -ForegroundColor Yellow
    Write-Host "================================================" -ForegroundColor Gray
    
    try {
        Push-Location admin-panel
        
        # Check if node_modules exists
        if (-not (Test-Path "node_modules")) {
            Write-Host "Installing npm dependencies..." -ForegroundColor Gray
            npm install
        }
        
        if ($Coverage) {
            Write-Host "Running tests with code coverage..." -ForegroundColor Gray
            npm run test:coverage
        } else {
            npm test -- --run
        }
        
        if ($LASTEXITCODE -eq 0) {
            $frontendSuccess = $true
            Write-Host "`n✓ Frontend tests PASSED" -ForegroundColor Green
            
            if ($Coverage -and (Test-Path "coverage/index.html")) {
                Write-Host "Coverage report generated at: admin-panel/coverage/index.html" -ForegroundColor Cyan
                
                if (-not $CI) {
                    $openReport = Read-Host "`nOpen coverage report? (y/n)"
                    if ($openReport -eq 'y') {
                        Start-Process "./coverage/index.html"
                    }
                }
            }
        } else {
            throw "Frontend tests failed with exit code $LASTEXITCODE"
        }
    }
    catch {
        Write-Host "`n✗ Frontend tests FAILED" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        
        if ($CI) {
            Pop-Location
            exit 1
        }
    }
    finally {
        Pop-Location
    }
}

# Summary
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

if (-not $SkipBackend) {
    $status = if ($backendSuccess) { "✓ PASSED" } else { "✗ FAILED" }
    $color = if ($backendSuccess) { "Green" } else { "Red" }
    Write-Host "Backend Tests:  " -NoNewline
    Write-Host $status -ForegroundColor $color
}

if (-not $SkipFrontend) {
    $status = if ($frontendSuccess) { "✓ PASSED" } else { "✗ FAILED" }
    $color = if ($frontendSuccess) { "Green" } else { "Red" }
    Write-Host "Frontend Tests: " -NoNewline
    Write-Host $status -ForegroundColor $color
}

Write-Host "`nTotal Duration: $($duration.ToString('mm\:ss'))" -ForegroundColor Gray
Write-Host "==================================================" -ForegroundColor Cyan

# Exit with appropriate code
$allSuccess = ($SkipBackend -or $backendSuccess) -and ($SkipFrontend -or $frontendSuccess)

if ($allSuccess) {
    Write-Host "`n🎉 All tests passed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ Some tests failed. Please review the output above." -ForegroundColor Red
    exit 1
}
