# Data Validation Rules

## Overview
Comprehensive documentation of all data validation rules and business constraints identified in the Australian Tax Calculator system.

---

## Input Validation Rules

### Financial Data Validation

#### VR001: Taxable Income Non-Negative Rule
**Sources**: 
- `TaxCalculationService.CalculateTaxAsync()` (Line 29-30)
- `TaxController.CalculateTax()` (Line 31-32)

**Rule**: Taxable income must be greater than or equal to zero.

**Implementation**:
```csharp
if (request.TaxableIncome < 0)
    throw new ArgumentException("Taxable income cannot be negative");
```

**Business Justification**: Negative income is not valid for tax calculations in the Australian tax system.

**Error Response**: HTTP 400 Bad Request with descriptive message.

---

#### VR002: Income Range Validation for API Endpoints
**Source**: `TaxController.CompareTax()`, `TaxController.GetTaxHistory()`

**Rule**: Income parameters in API endpoints must be non-negative.

**Implementation**:
```csharp
if (income < 0)
    return BadRequest("Income cannot be negative");
```

**Applied To**:
- Tax comparison endpoint
- Tax history endpoint
- Direct calculation requests

---

### Temporal Data Validation

#### VR003: Financial Year Required Rule
**Source**: `TaxController.CalculateTax()` (Line 34-35)

**Rule**: Financial year must be provided and cannot be null or empty.

**Implementation**:
```csharp
if (string.IsNullOrEmpty(request.FinancialYear))
    return BadRequest("Financial year is required");
```

**Business Justification**: Tax calculations are year-specific due to changing tax brackets, rates, and rules.

**Expected Format**: "YYYY-YY" (e.g., "2023-24")

---

#### VR004: Year Parameter Validation
**Source**: `TaxController.GetTaxBrackets()` (Line 58-59)

**Rule**: Year parameter for tax bracket retrieval must be provided.

**Implementation**:
```csharp
if (string.IsNullOrEmpty(year))
    return BadRequest("Year is required");
```

**Business Impact**: Ensures correct tax bracket data is retrieved for calculations.

---

#### VR005: Tax History Years Range Validation
**Source**: `TaxController.GetTaxHistory()` (Line 102-103)

**Rule**: Tax history requests must specify between 1 and 20 years.

**Implementation**:
```csharp
if (years <= 0 || years > 20)
    return BadRequest("Years must be between 1 and 20");
```

**Business Constraints**:
- Minimum: 1 year
- Maximum: 20 years
- Default: 10 years

**Justification**: Performance and data availability constraints.

---

### Request Structure Validation

#### VR006: Request Object Null Validation
**Source**: `TaxController.CalculateTax()` (Line 28-29)

**Rule**: Tax calculation requests cannot be null.

**Implementation**:
```csharp
if (request == null)
    return BadRequest("Request cannot be null");
```

**Business Impact**: Prevents null reference exceptions and ensures data integrity.

---

#### VR007: Years Array Validation
**Source**: `TaxController.CompareTax()` (Line 80-81)

**Rule**: Multi-year tax comparison requires at least one year.

**Implementation**:
```csharp
if (years == null || years.Length == 0)
    return BadRequest("Years are required");
```

**Business Logic**: Comparison requires baseline data for meaningful results.

---

## Business Data Completeness Rules

### Monthly Income Data Validation

#### VR008: Complete Monthly Data Requirement
**Source**: `UserTaxService.CalculateAnnualTaxAsync()` (Line 34-37)

**Rule**: Annual tax calculation requires complete 12 months of income data.

**Implementation**:
```csharp
if (monthlyIncomes.Count != 12)
{
    throw new InvalidOperationException(
        $"Incomplete monthly income data for user {userId} in {financialYear}. " +
        $"Found {monthlyIncomes.Count} months, expected 12."
    );
}
```

**Business Justification**: 
- Accurate annual tax assessment requires complete year data
- Partial year calculations would be misleading
- ATO reporting requires full financial year data

**Error Type**: `InvalidOperationException` (system error, not user input error)

---

### Data Type Validation

#### VR009: Decimal Precision Validation
**Applied To**: All financial amounts (income, tax, levies, offsets)

**Rule**: Financial calculations must use decimal type for precision.

**Implementation**: All monetary values use `decimal` type throughout the system.

**Business Justification**: Floating-point precision errors are unacceptable for financial calculations.

---

#### VR010: GUID Validation for User IDs
**Source**: Various user-related methods

**Rule**: User identifiers must be valid GUIDs.

**Implementation**: Strong typing with `Guid` type enforces validation.

**Business Impact**: Ensures consistent user identification across the system.

---

## Configuration Data Validation

### Tax Bracket Validation

#### VR011: Tax Bracket Order Validation
**Source**: `TaxCalculationService.CalculateProgressiveIncomeTax()`

**Rule**: Tax brackets must be processed in ascending order.

**Implementation**:
```csharp
foreach (var bracket in brackets.OrderBy(b => b.BracketOrder))
```

**Business Rule**: Progressive tax system requires ordered bracket processing.

**Data Constraint**: BracketOrder must be unique and sequential.

---

#### VR012: Tax Bracket Range Validation
**Implicit Rule**: Tax bracket ranges must be contiguous and non-overlapping.

**Business Logic**: 
- MinIncome of next bracket = MaxIncome of previous bracket + 1
- No gaps or overlaps in income ranges
- Highest bracket has MaxIncome = null (unlimited)

---

### Levy Configuration Validation

#### VR013: Medicare Levy Threshold Validation
**Source**: `TaxCalculationService.CalculateMedicareLevyAsync()`

**Rule**: Medicare levy applies only above threshold income.

**Implementation**:
```csharp
if (medicareLevy == null || income <= medicareLevy.ThresholdIncome)
{
    return new LevyCalculation { Amount = 0 };
}
```

**Data Requirements**:
- ThresholdIncome must be > 0
- LevyRate must be between 0 and 1
- Configuration must exist for each financial year

---

#### VR014: Budget Repair Levy Threshold Validation
**Source**: `TaxCalculationService.CalculateOtherLeviesAsync()`

**Rule**: Budget repair levy applies only above high-income threshold.

**Implementation**:
```csharp
if (budgetRepairLevy != null && income > budgetRepairLevy.ThresholdIncome)
{
    budgetRepairAmount = income * budgetRepairLevy.LevyRate;
}
```

**Data Constraints**:
- ThresholdIncome typically > $180,000
- LevyRate typically 2% (0.02)
- May not exist for all financial years (temporary levy)

---

### Tax Offset Validation

#### VR015: LITO Configuration Validation
**Source**: `TaxCalculationService.CalculateTaxOffsetsAsync()`

**Rule**: Low Income Tax Offset must have valid configuration parameters.

**Data Requirements**:
- MaxOffset > 0
- MaxIncome > 0 (or null for no limit)
- PhaseOutStart <= MaxIncome
- PhaseOutRate between 0 and 1

**Business Logic Validation**:
```csharp
if (lito != null && (lito.MaxIncome == null || income <= lito.MaxIncome))
{
    // Offset applies
}
```

---

## Runtime Validation Rules

### Calculation Result Validation

#### VR016: Non-Negative Tax Payable Rule
**Source**: `TaxCalculationService.CalculateTaxAsync()` (Line 70)

**Rule**: Net tax payable cannot be negative after applying offsets.

**Implementation**:
```csharp
result.NetTaxPayable = Math.Max(0, grossTax - result.TaxOffsets);
```

**Business Justification**: Tax system cannot generate negative tax (refunds beyond liability).

---

#### VR017: Effective Rate Calculation Validation
**Source**: `TaxCalculationService.CalculateTaxAsync()` (Line 72)

**Rule**: Effective tax rate calculation must handle zero income.

**Implementation**:
```csharp
result.EffectiveRate = result.TaxableIncome > 0 
    ? result.NetTaxPayable / result.TaxableIncome 
    : 0;
```

**Business Logic**: Division by zero prevention for zero income scenarios.

---

### Phase-Out Calculation Validation

#### VR018: LITO Phase-Out Minimum Floor
**Source**: `TaxCalculationService.CalculateTaxOffsetsAsync()`

**Rule**: Tax offset cannot be reduced below zero during phase-out.

**Implementation**:
```csharp
totalOffsets = Math.Max(0, lito.MaxOffset - phaseOutAmount);
```

**Business Impact**: Prevents negative offsets which would increase tax liability incorrectly.

---

## Cross-Field Validation Rules

### Income Relationship Validation

#### VR019: Taxable Income Calculation Consistency
**Source**: Monthly income data processing

**Rule**: TaxableIncome = GrossIncome - DeductionsAmount

**Validation**: System should verify this relationship when processing monthly data.

**Implementation**: Implicit in `UserMonthlyIncome` entity relationships.

---

#### VR020: Annual Aggregation Validation
**Source**: `UserTaxService.CalculateAnnualTaxAsync()`

**Rule**: Annual totals must equal sum of monthly values.

**Implementation**:
```csharp
var totalGrossIncome = monthlyIncomes.Sum(m => m.GrossIncome);
var totalDeductions = monthlyIncomes.Sum(m => m.DeductionsAmount);
var totalTaxableIncome = monthlyIncomes.Sum(m => m.TaxableIncome);
```

**Validation**: Cross-check relationships between fields.

---

## Error Handling Validation

### Exception Type Mapping

#### VR021: Business Rule Violation Handling
**Rule**: Business rule violations should throw `ArgumentException`.

**Sources**: Input validation throughout the system

**Error Mapping**:
- `ArgumentException` → HTTP 400 Bad Request
- `InvalidOperationException` → HTTP 500 Internal Server Error
- General `Exception` → HTTP 500 Internal Server Error

---

#### VR022: Validation Error Message Standards
**Rule**: Error messages must be descriptive and actionable.

**Examples**:
- "Taxable income cannot be negative"
- "Financial year is required"
- "Years must be between 1 and 20"

**Standards**:
- Clear description of the violation
- No technical jargon
- Actionable guidance for correction

---

## Data Integrity Rules

### Database Constraints

#### VR023: Financial Year Format Consistency
**Rule**: Financial year strings must follow consistent format.

**Expected Format**: "YYYY-YY" (e.g., "2023-24", "2024-25")

**Validation**: Regular expression pattern matching in data layer.

---

#### VR024: Tax Bracket Data Integrity
**Rule**: Tax bracket data must be complete for each financial year.

**Requirements**:
- At least one bracket with MinIncome = 0
- Contiguous income ranges
- Valid tax rates (0 ≤ rate ≤ 1)
- Ordered by BracketOrder

---

#### VR025: User Data Completeness
**Rule**: User income data must be complete for accurate calculations.

**Constraints**:
- UserID must be valid GUID
- Month must be 1-12
- GrossIncome ≥ 0
- DeductionsAmount ≥ 0
- TaxableIncome = GrossIncome - DeductionsAmount

---

## Security Validation Rules

### Input Sanitization

#### VR026: SQL Injection Prevention
**Rule**: All database inputs must be parameterized.

**Implementation**: Repository pattern with parameterized queries.

**Business Impact**: Prevents malicious SQL injection attacks.

---

#### VR027: Cross-Site Scripting Prevention
**Rule**: API responses must not contain executable content.

**Implementation**: JSON serialization with proper encoding.

**Business Impact**: Prevents XSS attacks on API consumers.

---

## Performance Validation Rules

### Resource Constraints

#### VR028: Request Size Limitations
**Rule**: API requests must not exceed reasonable size limits.

**Implementation**: Standard HTTP request size limits.

**Business Justification**: Prevents resource exhaustion attacks.

---

#### VR029: Response Time Validation
**Rule**: Tax calculations should complete within acceptable timeframes.

**Target**: < 2 seconds for standard calculations

**Implementation**: Performance monitoring and caching strategies.

---

## Validation Rule Summary

| Category | Rule Count | Priority |
|----------|------------|----------|
| **Input Validation** | 7 | High |
| **Business Data** | 2 | High |
| **Configuration** | 4 | Medium |
| **Runtime Validation** | 4 | High |
| **Cross-Field** | 2 | Medium |
| **Error Handling** | 2 | Medium |
| **Data Integrity** | 3 | High |
| **Security** | 2 | High |
| **Performance** | 2 | Low |

**Total Validation Rules**: 29

---

## Testing Implications

### Validation Test Categories

1. **Positive Tests**: Valid data scenarios
2. **Negative Tests**: Invalid data scenarios
3. **Boundary Tests**: Edge cases and limits
4. **Cross-Field Tests**: Relationship validation
5. **Security Tests**: Injection and XSS prevention

### Test Data Requirements

1. **Valid Scenarios**: Complete, correct data
2. **Invalid Scenarios**: Each validation rule violation
3. **Edge Cases**: Boundary values and nulls
4. **Performance Tests**: Large data sets and concurrent requests

---

## Migration Considerations

### Framework Validation Changes

1. **Model Validation**: Consider using Data Annotations
2. **Custom Validators**: Implement IValidatableObject
3. **API Validation**: Use Action Filters for consistent validation
4. **Error Handling**: Implement global exception handlers

### Backward Compatibility

1. **Preserve Validation Logic**: Maintain existing business rules
2. **Error Message Consistency**: Keep existing error messages
3. **HTTP Status Codes**: Maintain existing API contracts
4. **Validation Behavior**: Ensure identical validation outcomes
