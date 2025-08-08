# .NET 8 Australian Tax Calculator

A comprehensive Australian tax calculation application successfully migrated from .NET Framework 4.8 to .NET 8. This project demonstrates modern .NET 8 development practices and serves as an example of enterprise-grade application migration.

## Overview

This application calculates Australian income tax, Medicare levy, and historical levies (such as the Budget Repair Levy) for financial years 2015-16 through 2024-25. It demonstrates:

- **Historical tax calculations** with accurate progressive tax brackets
- **Modern ASP.NET Core Web API** endpoints for tax calculation, bracket retrieval, and health checks
- **Clean architecture patterns** with dependency injection and repository pattern
- **.NET 8 ecosystem** with modern data access and caching
- **Cross-platform compatibility** runs on Windows, Linux, and macOS

## Solution Architecture

```
AustralianTaxCalculator/
├── TaxCalculator.Core/                    # Domain models and entities (.NET 8)
├── TaxCalculator.Data/                    # Repositories and data access (.NET 8)
├── TaxCalculator.Services/                # Business logic and tax calculation engine (.NET 8)
├── TaxCalculator.Infrastructure/          # Infrastructure services (.NET 8)
├── TaxCalculator.AspNetCore.Api/          # ASP.NET Core 8 Web API (NEW)
├── TaxCalculator.AspNetCore.Api.Tests/    # Integration tests (.NET 8)
├── TaxCalculator.Tests.Unit/              # Unit tests with NUnit (.NET 8)
├── TaxCalculator.Console/                 # Database setup utility (Legacy)
├── TaxCalculator.Api/                     # Legacy Web API 2 (Preserved)
├── TaxCalculator.StandaloneApi/           # Legacy standalone API (Preserved)
├── Database/                              # SQL Server schema and seed scripts
└── publish/                               # Published .NET 8 application
```

## How to Run (.NET 8)

### Prerequisites

- **.NET 8 SDK** or later
- **SQL Server LocalDB** (optional, uses in-memory data by default)
- **Visual Studio 2022** or **VS Code** (recommended)

### Quick Start (Recommended)

1. **Run the published application:**
   ```cmd
   cd publish
   TaxCalculator.AspNetCore.Api.exe
   ```
   API starts on `https://localhost:5001` and `http://localhost:5000`

2. **Test the API:**
   ```cmd
   curl https://localhost:5001/health
   ```

### Development Build

1. **Build and run:**
   ```cmd
   dotnet build TaxCalculator.AspNetCore.Api
   dotnet run --project TaxCalculator.AspNetCore.Api
   ```

2. **Build entire solution:**
   ```cmd
   dotnet build AustralianTaxCalculator.sln
   ```

### Database Setup (Optional)

The application works with in-memory data by default. To use SQL Server:

1. **Update connection string** in `TaxCalculator.AspNetCore.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AustralianTaxDB;Trusted_Connection=true;"
     }
   }
   ```

2. **Create database:**
   ```cmd
   sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database\CreateDatabase.sql
   ```

3. **Seed data:**
   ```cmd
   sqlcmd -S "(localdb)\MSSQLLocalDB" -d AustralianTaxDB -i Database\SeedData.sql
   ```

## Technology Stack (.NET 8)

### Core Framework
- **.NET 8** - Modern cross-platform framework
- **C# 12** - Latest language features

### Web API
- **ASP.NET Core 8** - Modern web API framework
- **Minimal APIs** - Lightweight API endpoints
- **Built-in dependency injection** - No external IoC container needed

### Data Access
- **Microsoft.Data.SqlClient 5.1.x** - Modern SQL Server provider
- **ADO.NET** - Direct database connectivity (no ORM)
- **Repository pattern** - Data access abstraction

### Configuration
- **appsettings.json** - Modern configuration system
- **Environment-specific settings** - Development/Production configurations

### Caching
- **StackExchange.Redis 2.8.x** - Modern Redis client

### Testing
- **NUnit 4.x** - Latest unit testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

### JSON Serialization
- **Newtonsoft.Json** - Preserved for API contract compatibility

## API Endpoints

The .NET 8 API maintains full backward compatibility with the original API contracts.

### Health Check
```http
GET /health
```
Returns server health status and timestamp.

### Tax Calculation
```http
POST /tax/calculate
Content-Type: application/json

{
  "taxableIncome": 75000,
  "financialYear": "2024-25"
}
```

Returns detailed tax calculation including:
- Income tax
- Medicare levy
- Budget Repair Levy (historical years)
- Tax offsets
- Net tax payable
- Effective tax rate

### Tax Brackets
```http
GET /tax/brackets/{financialYear}
```
Example: `GET /tax/brackets/2024-25`

Returns progressive tax brackets for the specified financial year.

## Tax Calculation Features

### Supported Financial Years
- **2015-16** to **2024-25** (10 years of historical data)

### Tax Components
- **Progressive Income Tax** - Based on ATO tax brackets
- **Medicare Levy** - 2% for incomes above threshold ($23,365-$29,207 depending on year)
- **Budget Repair Levy** - 2% for incomes above $180,000 (2014-15 to 2016-17)
- **Low Income Tax Offset (LITO)** - Automatic offset for eligible incomes

### Examples

**Middle Income (2024-25):**
- Income: $75,000
- Net Tax: $14,788
- Effective Rate: 19.72%

**High Income with Budget Repair Levy (2015-16):**
- Income: $200,000  
- Net Tax: $71,547
- Effective Rate: 35.77%
- Includes Budget Repair Levy: $4,000

## Testing

### Unit Tests
```cmd
dotnet test TaxCalculator.Tests.Unit
```

### Integration Tests
```cmd
dotnet test TaxCalculator.AspNetCore.Api.Tests
```

### Run All Tests
```cmd
dotnet test
```

## CI/CD Pipeline

The project includes a GitHub Actions workflow (`.github/workflows/dotnet8-ci.yml`) that:
- Builds the .NET 8 solution
- Runs unit tests
- Runs integration tests
- Publishes the application

## Migration Highlights

This application was successfully migrated from .NET Framework 4.8 to .NET 8:

### ✅ **Completed Migration**
- **Core Libraries** - All migrated to .NET 8 with SDK-style project files
- **ASP.NET Web API 2** → **ASP.NET Core 8 Web API**
- **Autofac 4.x** → **Built-in ASP.NET Core DI Container**
- **System.Data.SqlClient** → **Microsoft.Data.SqlClient 5.1.x**
- **App.config** → **appsettings.json**
- **Legacy project files** → **Modern SDK-style projects**

### 🔄 **API Contract Preservation**
- **Zero breaking changes** - All existing API contracts preserved
- **Newtonsoft.Json** - Maintained for exact serialization compatibility
- **Same endpoints** - `/health`, `/tax/calculate`, `/tax/brackets/{year}`
- **Same request/response models** - Full backward compatibility

### 📈 **Modern Improvements**
- **Cross-platform** - Runs on Windows, Linux, macOS
- **Better performance** - .NET 8 runtime optimizations
- **Modern tooling** - dotnet CLI, modern project system
- **Improved security** - Latest security patches and features

## Legacy Compatibility

The original .NET Framework 4.8 projects are preserved for reference:
- `TaxCalculator.Api` - Original ASP.NET Web API 2
- `TaxCalculator.StandaloneApi` - Original self-hosted API

## Performance Benefits

.NET 8 provides significant performance improvements over .NET Framework 4.8:
- **Faster startup time** - ~40% improvement
- **Better memory efficiency** - Reduced allocations
- **Improved JSON serialization** - System.Text.Json available
- **Modern garbage collection** - Better throughput

## Development Experience

Modern development features in .NET 8:
- **Hot reload** - Code changes without restart
- **Better debugging** - Enhanced Visual Studio integration
- **NuGet improvements** - Central package management
- **Modern C#** - Latest language features

## License

This project is provided as an educational example for .NET Framework to .NET 8 migration patterns.

## Contributing

This project demonstrates successful migration patterns from legacy .NET Framework to modern .NET 8. For real-world tax calculations, please consult the Australian Taxation Office (ATO) for current rates and regulations.
