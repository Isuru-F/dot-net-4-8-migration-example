# AGENT.md - Australian Tax Calculator (.NET Framework 4.8)

## Build/Test Commands
- **Build solution:** `msbuild AustralianTaxCalculator.sln /p:Configuration=Debug`
- **Run all tests:** `nunit3-console TaxCalculator.Tests.Unit\bin\Debug\TaxCalculator.Tests.Unit.dll`
- **Run standalone API:** `msbuild TaxCalculator.StandaloneApi\TaxCalculator.StandaloneApi.csproj && TaxCalculator.StandaloneApi\bin\Debug\TaxCalculator.StandaloneApi.exe`
- **Test API client:** `msbuild ApiTestClient.csproj && bin\Debug\ApiTestClient.exe`
- **NuGet restore:** `nuget restore AustralianTaxCalculator.sln`

## Architecture & Structure
- **Layered architecture:** Presentation (Api/Console) → Business (Services) → Data (Data/Infrastructure) → Core (models)
- **Key projects:** TaxCalculator.Api (Web API 2), TaxCalculator.Services (business logic), TaxCalculator.Data (ADO.NET repositories)
- **Database:** SQL Server LocalDB with in-memory fallback, ADO.NET data access
- **DI Container:** Autofac 4.9.4 for dependency injection
- **Testing:** NUnit 3.13.3 with Moq for mocking

## Code Style Guidelines
- **Namespaces:** `TaxCalculator.{Layer}.{Feature}` pattern (e.g., TaxCalculator.Services.Interfaces)
- **Interfaces:** Prefix with 'I' (ITaxCalculationService, ILogger, ICacheService)
- **Methods:** PascalCase, descriptive names, async methods end with 'Async'
- **Error handling:** Try-catch with specific exceptions, logging at service layer
- **Dependencies:** Constructor injection, interface-based design
- **Legacy .NET 4.8:** Use ADO.NET, avoid newer C# features, target .NET Framework patterns
