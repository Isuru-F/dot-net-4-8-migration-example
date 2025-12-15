# Australian Tax Calculator - Complete Method List

## API Controllers

### TaxController
- `CalculateTax([FromBody] TaxCalculationRequest request)` - POST /api/tax/calculate
- `CompareTax(decimal income, [FromUri] string[] years)` - GET /api/tax/compare
- `GetTaxBrackets(string year)` - GET /api/tax/brackets/{year}
- `GetTaxHistory(decimal income, int years = 10)` - GET /api/tax/history/{income}

### HealthController
- `GetHealth()` - GET /api/health

## Core Service Interfaces

### ITaxCalculationService
- `CalculateTaxAsync(TaxCalculationRequest request)`
- `CompareTaxAcrossYearsAsync(decimal income, List<string> years)`
- `GetTaxBracketsAsync(string financialYear)`
- `GetTaxHistoryAsync(decimal income, int years = 10)`

### IUserTaxService
- `CalculateAnnualTaxAsync(Guid userId, string financialYear)`
- `GetAnnualSummaryAsync(Guid userId, string financialYear)`
- `GetMonthlyIncomeAsync(Guid userId, string financialYear)`
- `GetTaxHistoryAsync(Guid userId, int years = 5)`
- `SaveMonthlyIncomeAsync(UserMonthlyIncome income)`

### ICacheService
- `GetAsync<T>(string key) where T : class`
- `RemoveAsync(string key)`
- `SetAsync<T>(string key, T value, TimeSpan expiration) where T : class`

### ILogger
- `LogDebug(string message)`
- `LogError(string message, Exception exception = null)`
- `LogInformation(string message)`
- `LogWarning(string message)`

## Data Repository Interfaces

### ITaxBracketRepository
- `GetAvailableFinancialYearsAsync()`
- `GetTaxBracketsAsync(string financialYear)`
- `GetTaxLeviesAsync(string financialYear)`
- `GetTaxOffsetsAsync(string financialYear)`

### IUserIncomeRepository
- `GetAnnualSummaryAsync(Guid userId, string financialYear)`
- `GetMonthlyIncomeAsync(Guid userId, string financialYear)`
- `GetTaxHistoryAsync(Guid userId, int years = 5)`
- `SaveAnnualSummaryAsync(UserAnnualTaxSummary summary)`
- `SaveMonthlyIncomeAsync(UserMonthlyIncome income)`
- `UpdateMonthlyIncomeAsync(UserMonthlyIncome income)`

### IUserRepository
- `CreateUserAsync(User user)`
- `GetAllActiveUsersAsync()`
- `GetUserAsync(Guid userId)`
- `UpdateUserAsync(User user)`
- `UserExistsAsync(string email)`

### IConnectionFactory
- `CreateConnection()`
- `CreateConnection(string connectionString)`

## Implementation Classes

### TaxCalculationService
- `CalculateTaxAsync(TaxCalculationRequest request)`
- `CompareTaxAcrossYearsAsync(decimal income, List<string> years)`
- `GetTaxBracketsAsync(string financialYear)`
- `GetTaxHistoryAsync(decimal income, int years = 10)`

### UserTaxService
- `CalculateAnnualTaxAsync(Guid userId, string financialYear)`
- `GetAnnualSummaryAsync(Guid userId, string financialYear)`
- `GetMonthlyIncomeAsync(Guid userId, string financialYear)`
- `GetTaxHistoryAsync(Guid userId, int years = 5)`
- `SaveMonthlyIncomeAsync(UserMonthlyIncome income)`

### CacheService
- `GetAsync<T>(string key) where T : class`
- `RemoveAsync(string key)`
- `SetAsync<T>(string key, T value, TimeSpan expiration) where T : class`

### SimpleLogger
- `LogDebug(string message)`
- `LogError(string message, Exception exception = null)`
- `LogInformation(string message)`
- `LogWarning(string message)`

### TaxBracketRepository
- `GetAvailableFinancialYearsAsync()`
- `GetTaxBracketsAsync(string financialYear)`
- `GetTaxLeviesAsync(string financialYear)`
- `GetTaxOffsetsAsync(string financialYear)`

### UserIncomeRepository
- `GetAnnualSummaryAsync(Guid userId, string financialYear)`
- `GetMonthlyIncomeAsync(Guid userId, string financialYear)`
- `GetTaxHistoryAsync(Guid userId, int years = 5)`
- `SaveAnnualSummaryAsync(UserAnnualTaxSummary summary)`
- `SaveMonthlyIncomeAsync(UserMonthlyIncome income)`
- `UpdateMonthlyIncomeAsync(UserMonthlyIncome income)`

### UserRepository
- `CreateUserAsync(User user)`
- `GetAllActiveUsersAsync()`
- `GetUserAsync(Guid userId)`
- `UpdateUserAsync(User user)`
- `UserExistsAsync(string email)`

### SqlConnectionFactory
- `CreateConnection()`
- `CreateConnection(string connectionString)`

## Configuration Classes

### AutofacConfig
- `Configure()`

### WebApiConfig
- `Register(HttpConfiguration config)`

### WebApiApplication
- `Application_Start()`

## Unit Test Methods

### CacheServiceTests
- `CacheService_ConcurrentAccess_HandlesCorrectly()`
- `GetAsync_AfterPartialExpiration_OnlyReturnsValidItems()`
- `GetAsync_ExpiredItem_ReturnsNull()`
- `GetAsync_NonExistentKey_ReturnsNull()`
- `GetAsync_ValidItem_ReturnsCorrectValue()`
- `GetAsync_WrongType_ReturnsNull()`
- `RemoveAsync_ExistingKey_RemovesItem()`
- `RemoveAsync_NonExistentKey_DoesNotThrow()`
- `SetAsync_ComplexObject_StoresAndRetrievesCorrectly()`
- `SetAsync_LongExpiration_ItemPersists()`
- `SetAsync_NullValue_StoresNull()`
- `SetAsync_OverwriteExistingKey_UpdatesValue()`
- `SetAsync_ValidKeyValue_StoresSuccessfully()`
- `SetAsync_ZeroExpiration_ItemExpiresImmediately()`
- `Setup()`

### TaxCalculationServiceTests
- `CalculateTaxAsync_2024_25_Income85000_ReturnsCorrectTax()`
- `CalculateTaxAsync_2024_25_VariousIncomes_ReturnsExpectedTax(decimal income, decimal expectedTax)`
- `CalculateTaxAsync_NegativeIncome_ThrowsArgumentException()`
- `CalculateTaxAsync_WithLITO_AppliesOffsetCorrectly()`
- `CalculateTaxAsync_WithMedicareLevy_CalculatesCorrectly()`
- `GetTaxBracketsAsync_ValidYear_ReturnsBrackets()`
- `Setup()`

### UserTaxServiceTests
- `CalculateAnnualTaxAsync_IncompleteMonthlyData_ThrowsInvalidOperationException()`
- `CalculateAnnualTaxAsync_LogsInformation()`
- `CalculateAnnualTaxAsync_ValidInput_ReturnsCorrectSummary()`
- `CalculateAnnualTaxAsync_ZeroIncome_HandlesCorrectly()`
- `GetAnnualSummaryAsync_ValidInput_ReturnsRepositoryResult()`
- `GetMonthlyIncomeAsync_ValidInput_ReturnsRepositoryResult()`
- `GetTaxHistoryAsync_WithCustomYears_ReturnsRepositoryResult()`
- `GetTaxHistoryAsync_WithDefaultYears_ReturnsRepositoryResult()`
- `SaveMonthlyIncomeAsync_ValidInput_CallsRepository()`
- `Setup()`

## Total Method Count
- **API Endpoints:** 5 methods
- **Service Layer:** 22 methods
- **Data Layer:** 19 methods
- **Infrastructure:** 7 methods
- **Unit Tests:** 27 methods
- **Total:** 80 public methods
