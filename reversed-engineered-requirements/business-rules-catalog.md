# Business Rules Catalog

## Overview
This document catalogs all business rules identified in the Australian Tax Calculator system.

---

## Tax Calculation Rules

### BR001: Progressive Income Tax Calculation
- **Source**: `TaxCalculationService.CalculateProgressiveIncomeTax()` 
- **Rule**: Income tax is calculated using progressive tax brackets with cumulative fixed amounts and rates
- **Implementation**: Lines 78-122 in TaxCalculationService.cs
- **Business Impact**: Core revenue calculation for Australian tax system
- **Compliance**: Australian Taxation Office regulations

### BR002: Tax Bracket Order Processing
- **Source**: `TaxCalculationService.CalculateProgressiveIncomeTax()`
- **Rule**: Tax brackets must be processed in order from lowest to highest income thresholds
- **Implementation**: `foreach (var bracket in brackets.OrderBy(b => b.BracketOrder))`
- **Business Impact**: Ensures correct progressive tax calculation
- **Compliance**: Required for accurate tax computation

### BR003: Marginal Tax Rate Determination
- **Source**: `TaxCalculationService.GetMarginalTaxRate()`
- **Rule**: Marginal rate is the tax rate of the highest bracket that applies to the income
- **Implementation**: Lines 196-204 in TaxCalculationService.cs
- **Business Impact**: Provides taxpayer transparency on their tax bracket

### BR004: Effective Tax Rate Calculation
- **Source**: `TaxCalculationService.CalculateTaxAsync()`
- **Rule**: Effective rate = Net Tax Payable ÷ Taxable Income
- **Implementation**: `result.EffectiveRate = result.TaxableIncome > 0 ? result.NetTaxPayable / result.TaxableIncome : 0`
- **Business Impact**: Key metric for taxpayer understanding

---

## Levy Calculation Rules

### BR005: Medicare Levy Threshold
- **Source**: `TaxCalculationService.CalculateMedicareLevyAsync()`
- **Rule**: Medicare levy only applies to income above the threshold amount
- **Implementation**: `if (medicareLevy == null || income <= medicareLevy.ThresholdIncome)`
- **Business Impact**: Healthcare funding mechanism
- **Compliance**: Medicare Levy Act requirements

### BR006: Medicare Levy Rate Application
- **Source**: `TaxCalculationService.CalculateMedicareLevyAsync()`
- **Rule**: Medicare levy is calculated as income × levy rate for income above threshold
- **Implementation**: `var amount = income * medicareLevy.LevyRate`
- **Business Impact**: Proportional healthcare contribution

### BR007: Budget Repair Levy High Income
- **Source**: `TaxCalculationService.CalculateOtherLeviesAsync()`
- **Rule**: Budget repair levy applies only to high earners above threshold
- **Implementation**: `if (budgetRepairLevy != null && income > budgetRepairLevy.ThresholdIncome)`
- **Business Impact**: Additional revenue from high earners
- **Compliance**: Temporary levy for budget repair

---

## Tax Offset Rules

### BR008: LITO Eligibility Check
- **Source**: `TaxCalculationService.CalculateTaxOffsetsAsync()`
- **Rule**: Low Income Tax Offset (LITO) applies only below maximum income threshold
- **Implementation**: `if (lito != null && (lito.MaxIncome == null || income <= lito.MaxIncome))`
- **Business Impact**: Reduces tax burden for low-income earners

### BR009: LITO Phase-Out Calculation
- **Source**: `TaxCalculationService.CalculateTaxOffsetsAsync()`
- **Rule**: LITO phases out gradually above phase-out threshold
- **Implementation**: `var phaseOutAmount = (income - lito.PhaseOutStart.Value) * (lito.PhaseOutRate ?? 0)`
- **Business Impact**: Smooth transition to prevent cliff effects

### BR010: Minimum Tax Offset Application
- **Source**: `TaxCalculationService.CalculateTaxOffsetsAsync()`
- **Rule**: Tax offset cannot reduce payable tax below zero
- **Implementation**: `totalOffsets = Math.Max(0, lito.MaxOffset - phaseOutAmount)`
- **Business Impact**: Prevents negative tax scenarios

---

## Data Validation Rules

### BR011: Income Non-Negative Validation
- **Source**: `TaxCalculationService.CalculateTaxAsync()`, `TaxController.CalculateTax()`
- **Rule**: Taxable income cannot be negative
- **Implementation**: `if (request.TaxableIncome < 0) throw new ArgumentException("Taxable income cannot be negative")`
- **Business Impact**: Data integrity for tax calculations
- **Compliance**: Logical business constraint

### BR012: Financial Year Required
- **Source**: `TaxController.CalculateTax()`
- **Rule**: Financial year must be provided for tax calculations
- **Implementation**: `if (string.IsNullOrEmpty(request.FinancialYear)) return BadRequest("Financial year is required")`
- **Business Impact**: Ensures correct tax brackets are applied

### BR013: Monthly Income Completeness
- **Source**: `UserTaxService.CalculateAnnualTaxAsync()`
- **Rule**: Annual tax calculation requires complete 12 months of income data
- **Implementation**: `if (monthlyIncomes.Count != 12) throw new InvalidOperationException`
- **Business Impact**: Accurate annual tax assessment

---

## Calculation Aggregation Rules

### BR014: Net Tax Payable Floor
- **Source**: `TaxCalculationService.CalculateTaxAsync()`
- **Rule**: Net tax payable cannot be less than zero (after offsets)
- **Implementation**: `result.NetTaxPayable = Math.Max(0, grossTax - result.TaxOffsets)`
- **Business Impact**: Prevents tax refunds beyond liability

### BR015: Total Levies Calculation
- **Source**: `TaxCalculationService.CalculateTaxAsync()`
- **Rule**: Total levies = Medicare Levy + Budget Repair Levy + Other Levies
- **Implementation**: `TotalLevies = medicareLevyResult.Amount + otherLeviesResult.BudgetRepairLevy`
- **Business Impact**: Complete tax liability calculation

---

## User Service Rules

### BR016: Annual Summary Generation
- **Source**: `UserTaxService.CalculateAnnualTaxAsync()`
- **Rule**: Annual tax summary includes all income types and tax components
- **Implementation**: Lines 57-81 in UserTaxService.cs
- **Business Impact**: Comprehensive tax statement for users

### BR017: Tax History Limitations
- **Source**: `TaxController.GetTaxHistory()`
- **Rule**: Tax history requests limited to maximum 20 years
- **Implementation**: `if (years <= 0 || years > 20) return BadRequest("Years must be between 1 and 20")`
- **Business Impact**: Performance and data management

---

## API Business Rules

### BR018: Request Null Validation
- **Source**: `TaxController.CalculateTax()`
- **Rule**: Tax calculation requests cannot be null
- **Implementation**: `if (request == null) return BadRequest("Request cannot be null")`
- **Business Impact**: API data integrity

### BR019: Tax Bracket Year Validation
- **Source**: `TaxController.GetTaxBrackets()`
- **Rule**: Year parameter required for tax bracket retrieval
- **Implementation**: `if (string.IsNullOrEmpty(year)) return BadRequest("Year is required")`
- **Business Impact**: Correct tax bracket application

### BR020: Income Comparison Validation
- **Source**: `TaxController.CompareTax()`
- **Rule**: Multi-year tax comparison requires at least one year
- **Implementation**: `if (years == null || years.Length == 0) return BadRequest("Years are required")`
- **Business Impact**: Meaningful comparison data

---

## Caching Rules

### BR021: Tax Bracket Cache Duration
- **Source**: `TaxCalculationService.CalculateTaxAsync()`
- **Rule**: Tax brackets cached for 24 hours to improve performance
- **Implementation**: `await _cacheService.SetAsync(cacheKey, brackets, TimeSpan.FromHours(24))`
- **Business Impact**: Performance optimization while maintaining data freshness

---

## Business Rule Summary

| Category | Rule Count | Criticality |
|----------|------------|-------------|
| Tax Calculation | 4 | High |
| Levy Calculation | 3 | High |
| Tax Offsets | 3 | Medium |
| Data Validation | 3 | High |
| Aggregation | 2 | High |
| User Services | 2 | Medium |
| API Validation | 3 | Medium |
| Performance | 1 | Low |

**Total Business Rules Identified**: 21

## Compliance Requirements

1. **Australian Taxation Office (ATO) Regulations**
2. **Medicare Levy Act**
3. **Income Tax Assessment Act**
4. **Tax Administration Act**

## Risk Assessment

- **High Risk**: Tax calculation errors (BR001-BR004)
- **Medium Risk**: Validation failures (BR011-BR013)
- **Low Risk**: Performance degradation (BR021)
