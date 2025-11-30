# Quick Test Commands

This file contains quick copy-paste commands for running tests.

## Backend Tests

### Run all backend tests

```powershell
dotnet test
```

### Run with detailed output

```powershell
dotnet test --verbosity normal
```

### Run with coverage

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Run specific project tests

```powershell
# Domain tests
dotnet test src/MassageBookingBot.Domain.Tests/

# Application tests
dotnet test src/MassageBookingBot.Application.Tests/

# Infrastructure tests
dotnet test src/MassageBookingBot.Infrastructure.Tests/

# API tests
dotnet test src/MassageBookingBot.Api.Tests/
```

### Run specific test

```powershell
dotnet test --filter "FullyQualifiedName~BookingTests"
```

## Frontend Tests

### Run all frontend tests

```powershell
cd admin-panel
npm test
```

### Run with coverage

```powershell
cd admin-panel
npm run test:coverage
```

### Run in watch mode

```powershell
cd admin-panel
npm run test:watch
```

## Run All Tests

### Using the test script

```powershell
# Run all tests
.\run-tests.ps1

# Run with coverage
.\run-tests.ps1 -Coverage

# Run only backend
.\run-tests.ps1 -SkipFrontend

# Run only frontend
.\run-tests.ps1 -SkipBackend

# CI mode (fail fast)
.\run-tests.ps1 -CI
```

### Manual approach

```powershell
# Backend
dotnet test

# Frontend
cd admin-panel
npm test
```

## Coverage Reports

### Backend coverage

```powershell
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:Html
Start-Process ./TestResults/CoverageReport/index.html
```

### Frontend coverage

```powershell
cd admin-panel
npm run test:coverage
Start-Process ./coverage/index.html
```

## Debugging Tests

### Debug specific test in VS Code

1. Open test file
2. Set breakpoint
3. Click "Debug Test" above the test method

### Debug in Visual Studio

1. Right-click on test
2. Select "Debug Test"

### View test output

```powershell
dotnet test --logger:"console;verbosity=detailed"
```

## CI/CD

### GitHub Actions workflow is in

```
.github/workflows/test.yml
```

### Run tests like CI locally

```powershell
.\run-tests.ps1 -CI -Coverage
```
