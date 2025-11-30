# Testing Guide for Massage Booking Bot

## Overview

This document provides comprehensive instructions for running all tests in the Massage Booking Bot solution. The project includes:

- **Backend Tests** (.NET xUnit)
  - Domain Layer Tests
  - Application Layer Tests
  - Infrastructure Layer Tests
  - API Integration Tests
- **Frontend Tests** (Angular with Vitest)
  - Component Tests
  - Service Tests

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Backend Testing (.NET)](#backend-testing-net)
3. [Frontend Testing (Angular)](#frontend-testing-angular)
4. [Running All Tests](#running-all-tests)
5. [Code Coverage](#code-coverage)
6. [CI/CD Integration](#cicd-integration)
7. [Writing New Tests](#writing-new-tests)
8. [Troubleshooting](#troubleshooting)

## Prerequisites

### For Backend Tests

- .NET 10.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension

### For Frontend Tests

- Node.js 18+ and npm 10+
- Angular CLI (optional)

### Test Framework Dependencies

All test dependencies are already configured in the project files:

**Backend:**

- xUnit - Test framework
- Moq - Mocking library
- FluentAssertions - Assertion library
- Microsoft.EntityFrameworkCore.InMemory - In-memory database for testing
- Microsoft.AspNetCore.Mvc.Testing - Integration testing for API

**Frontend:**

- Vitest - Fast unit test framework
- @angular/core/testing - Angular testing utilities

## Backend Testing (.NET)

### Test Project Structure

```
src/
├── MassageBookingBot.Domain.Tests/
│   └── Entities/
│       ├── BookingTests.cs
│       └── ServiceTests.cs
├── MassageBookingBot.Application.Tests/
│   ├── Commands/
│   │   └── CreateBookingCommandHandlerTests.cs
│   └── Validators/
│       └── CreateBookingDtoValidatorTests.cs
├── MassageBookingBot.Infrastructure.Tests/
│   └── Services/
│       └── GoogleCalendarServiceTests.cs
└── MassageBookingBot.Api.Tests/
    └── Controllers/
        ├── BookingsControllerIntegrationTests.cs
        └── ServicesControllerIntegrationTests.cs
```

### Running Backend Tests

#### Run All Backend Tests

```powershell
# From solution root
dotnet test

# With detailed output
dotnet test --verbosity normal

# With logger output
dotnet test --logger "console;verbosity=detailed"
```

#### Run Tests for Specific Project

```powershell
# Domain tests only
dotnet test src/MassageBookingBot.Domain.Tests/MassageBookingBot.Domain.Tests.csproj

# Application tests only
dotnet test src/MassageBookingBot.Application.Tests/MassageBookingBot.Application.Tests.csproj

# Infrastructure tests only
dotnet test src/MassageBookingBot.Infrastructure.Tests/MassageBookingBot.Infrastructure.Tests.csproj

# API integration tests only
dotnet test src/MassageBookingBot.Api.Tests/MassageBookingBot.Api.Tests.csproj
```

#### Run Specific Test Class

```powershell
# Run specific test class
dotnet test --filter "FullyQualifiedName~BookingTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~BookingTests.Booking_ShouldBeCreatedWithValidProperties"
```

#### Filter Tests by Category

```powershell
# Run only unit tests (exclude integration tests)
dotnet test --filter "Category!=Integration"

# Run only integration tests
dotnet test --filter "Category=Integration"
```

### Backend Test Examples

#### Unit Test Example (Domain Layer)

```csharp
[Fact]
public void Booking_ShouldBeCreatedWithValidProperties()
{
    // Arrange & Act
    var booking = new Booking
    {
        UserId = 1,
        ServiceId = 2,
        BookingDateTime = DateTime.UtcNow.AddDays(1),
        Status = BookingStatus.Pending
    };

    // Assert
    booking.UserId.Should().Be(1);
    booking.Status.Should().Be(BookingStatus.Pending);
}
```

#### Integration Test Example (API Layer)

```csharp
[Fact]
public async Task GetBookings_ShouldReturnOk()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/bookings");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Frontend Testing (Angular)

### Test File Structure

```
admin-panel/src/app/
├── components/
│   └── bookings/
│       ├── bookings.ts
│       └── bookings.spec.ts
└── services/
    ├── api.service.ts
    └── api.service.spec.ts
```

### Running Frontend Tests

#### Run All Frontend Tests

```powershell
# Navigate to admin-panel directory
cd admin-panel

# Run tests with Vitest
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage
```

#### Run Specific Test File

```powershell
# Run specific test file
npx vitest run src/app/components/bookings/bookings.spec.ts

# Run tests matching a pattern
npx vitest run bookings
```

### Frontend Test Configuration

The Vitest configuration is in `admin-panel/vite.config.ts`:

```typescript
import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["src/test-setup.ts"],
    coverage: {
      provider: "v8",
      reporter: ["text", "html", "lcov"],
    },
  },
});
```

### Frontend Test Example

```typescript
describe("Bookings Component", () => {
  it("should load bookings on init", () => {
    const mockBookings = [{ id: 1, userName: "John", serviceName: "Massage" }];

    mockApiService.getBookings.mockReturnValue(of(mockBookings));
    component.ngOnInit();

    expect(mockApiService.getBookings).toHaveBeenCalled();
  });
});
```

## Running All Tests

### Run Complete Test Suite

```powershell
# Run all backend tests
dotnet test

# Run all frontend tests
cd admin-panel
npm test
```

### Automated Test Script

Create a PowerShell script `run-all-tests.ps1`:

```powershell
#!/usr/bin/env pwsh

Write-Host "Running Backend Tests..." -ForegroundColor Cyan
dotnet test --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Backend tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nBackend tests passed!" -ForegroundColor Green

Write-Host "`nRunning Frontend Tests..." -ForegroundColor Cyan
Push-Location admin-panel
npm test -- --run

if ($LASTEXITCODE -ne 0) {
    Pop-Location
    Write-Host "Frontend tests failed!" -ForegroundColor Red
    exit 1
}

Pop-Location
Write-Host "`nAll tests passed!" -ForegroundColor Green
```

Run the script:

```powershell
.\run-all-tests.ps1
```

## Code Coverage

### Backend Code Coverage

```powershell
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Generate detailed HTML report (requires ReportGenerator)
dotnet tool install --global dotnet-reportgenerator-globaltool

reportgenerator `
  -reports:"**/coverage.cobertura.xml" `
  -targetdir:"coverage-report" `
  -reporttypes:Html

# Open the report
Start-Process .\coverage-report\index.html
```

### Add Coverage Package to Test Projects

```powershell
# Add to each test project
dotnet add package coverlet.collector
```

### Frontend Code Coverage

```powershell
cd admin-panel

# Run tests with coverage
npm run test:coverage

# Coverage report is generated in admin-panel/coverage/
Start-Process .\coverage\index.html
```

### Coverage Configuration in package.json

```json
{
  "scripts": {
    "test": "vitest",
    "test:coverage": "vitest run --coverage"
  }
}
```

## CI/CD Integration

### GitHub Actions Example

Create `.github/workflows/test.yml`:

```yaml
name: Run Tests

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  backend-tests:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: "10.0.x"

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Run tests
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: "**/coverage.cobertura.xml"

  frontend-tests:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: "18"

      - name: Install dependencies
        working-directory: ./admin-panel
        run: npm ci

      - name: Run tests
        working-directory: ./admin-panel
        run: npm test -- --run

      - name: Run coverage
        working-directory: ./admin-panel
        run: npm run test:coverage
```

### Azure DevOps Pipeline Example

Create `azure-pipelines.yml`:

```yaml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: "windows-latest"

stages:
  - stage: Test
    jobs:
      - job: BackendTests
        steps:
          - task: UseDotNet@2
            inputs:
              version: "10.0.x"

          - task: DotNetCoreCLI@2
            displayName: "Restore packages"
            inputs:
              command: "restore"

          - task: DotNetCoreCLI@2
            displayName: "Run tests"
            inputs:
              command: "test"
              arguments: '--collect:"XPlat Code Coverage"'
              publishTestResults: true

      - job: FrontendTests
        steps:
          - task: NodeTool@0
            inputs:
              versionSpec: "18.x"

          - script: |
              cd admin-panel
              npm ci
              npm test -- --run
            displayName: "Run frontend tests"
```

## Writing New Tests

### Backend Test Template

```csharp
using FluentAssertions;
using Xunit;

namespace MassageBookingBot.Domain.Tests.Entities;

public class YourEntityTests
{
    [Fact]
    public void YourTest_Scenario_ExpectedBehavior()
    {
        // Arrange
        var entity = new YourEntity();

        // Act
        var result = entity.SomeMethod();

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(1, "expected1")]
    [InlineData(2, "expected2")]
    public void ParameterizedTest(int input, string expected)
    {
        // Arrange & Act
        var result = SomeMethod(input);

        // Assert
        result.Should().Be(expected);
    }
}
```

### Frontend Test Template

```typescript
import { describe, it, expect, beforeEach } from "vitest";
import { TestBed } from "@angular/core/testing";

describe("YourComponent", () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [YourComponent],
    }).compileComponents();
  });

  it("should do something", () => {
    // Arrange
    const component = TestBed.createComponent(YourComponent);

    // Act
    component.componentInstance.someMethod();

    // Assert
    expect(component.componentInstance.someProperty).toBe(expectedValue);
  });
});
```

## Test Best Practices

### Backend Testing

1. **Follow AAA Pattern**: Arrange, Act, Assert
2. **Use Descriptive Test Names**: `MethodName_Scenario_ExpectedBehavior`
3. **One Assert Per Test**: Focus each test on a single behavior
4. **Use FluentAssertions**: Makes assertions more readable
5. **Mock External Dependencies**: Use Moq for interfaces
6. **Isolate Tests**: Each test should be independent

### Frontend Testing

1. **Mock External Services**: Use vi.fn() for mocking
2. **Test Component Behavior**: Not implementation details
3. **Test User Interactions**: Simulate clicks, inputs, etc.
4. **Verify API Calls**: Check that services are called correctly
5. **Test Error Handling**: Verify error scenarios

## Troubleshooting

### Common Backend Issues

#### Issue: Tests can't find dependencies

```powershell
# Solution: Restore packages
dotnet restore
dotnet build
```

#### Issue: Database-related test failures

```csharp
// Solution: Use InMemory database with unique name per test
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

#### Issue: Async test hangs

```csharp
// Solution: Always use await and async Task
[Fact]
public async Task MyTest()
{
    await _service.MyMethodAsync(); // Don't forget await!
}
```

### Common Frontend Issues

#### Issue: Module not found errors

```powershell
# Solution: Reinstall dependencies
cd admin-panel
Remove-Item -Recurse -Force node_modules
npm install
```

#### Issue: Test timeout

```typescript
// Solution: Increase timeout in vitest.config.ts
export default defineConfig({
  test: {
    testTimeout: 10000, // 10 seconds
  },
});
```

#### Issue: Component not rendering

```typescript
// Solution: Call detectChanges()
fixture.detectChanges();
await fixture.whenStable();
```

## Test Metrics and Goals

### Target Coverage Goals

- **Domain Layer**: 90%+ coverage
- **Application Layer**: 85%+ coverage
- **Infrastructure Layer**: 75%+ coverage
- **API Controllers**: 80%+ coverage
- **Frontend Components**: 70%+ coverage

### Running Coverage Analysis

```powershell
# Backend
dotnet test --collect:"XPlat Code Coverage" --results-directory:./TestResults

# Frontend
cd admin-panel
npm run test:coverage
```

## Continuous Improvement

1. **Add Tests for New Features**: All new code should include tests
2. **Fix Broken Tests Immediately**: Don't let broken tests accumulate
3. **Review Test Coverage Regularly**: Identify gaps in test coverage
4. **Refactor Tests**: Keep tests maintainable and readable
5. **Update Documentation**: Keep this guide current with changes

## Additional Resources

- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [Vitest Documentation](https://vitest.dev/)
- [Angular Testing Guide](https://angular.io/guide/testing)

## Support

If you encounter issues with tests:

1. Check this documentation
2. Review test output for error messages
3. Verify all dependencies are installed
4. Check for environment-specific issues
5. Consult with the development team

---

**Last Updated**: November 30, 2025  
**Maintainer**: Development Team
