# Functional Requirements Documentation

## System Overview
Australian Tax Calculator - A comprehensive system for calculating income tax, levies, and offsets according to Australian taxation rules.

---

## Core Functional Requirements

### REQ001: Progressive Income Tax Calculation
**Priority**: Critical  
**Source**: TaxCalculationService.CalculateProgressiveIncomeTax()

**Description**: The system shall calculate income tax using Australian progressive tax brackets.

**Acceptance Criteria**:
- System must support multiple tax brackets with different rates
- Tax calculation must process brackets in ascending order
- Each bracket must have minimum income, maximum income (optional), tax rate, and fixed amount
- System must calculate cumulative tax across all applicable brackets
- Taxable income in each bracket = MIN(income - bracket_min + 1, bracket_max - bracket_min + 1)
- Tax in bracket = fixed_amount + (taxable_in_bracket × tax_rate)

**Business Rules**: BR001, BR002  
**Compliance**: Australian Taxation Office progressive tax structure

---

### REQ002: Medicare Levy Calculation
**Priority**: Critical  
**Source**: TaxCalculationService.CalculateMedicareLevyAsync()

**Description**: The system shall calculate Medicare levy for eligible taxpayers.

**Acceptance Criteria**:
- System must check income against Medicare levy threshold
- If income ≤ threshold, Medicare levy = 0
- If income > threshold, Medicare levy = income × levy_rate
- System must retrieve levy rates and thresholds by financial year
- Medicare levy must be included in total tax liability

**Business Rules**: BR005, BR006  
**Compliance**: Medicare Levy Act

---

### REQ003: Tax Offset Application
**Priority**: High  
**Source**: TaxCalculationService.CalculateTaxOffsetsAsync()

**Description**: The system shall apply Low Income Tax Offset (LITO) to reduce tax liability.

**Acceptance Criteria**:
- System must validate income eligibility for LITO
- If income ≤ max_income, offset applies
- If income > phase_out_start, offset = max_offset - ((income - phase_out_start) × phase_out_rate)
- Offset amount cannot be negative
- Final tax payable = MAX(0, gross_tax - total_offsets)

**Business Rules**: BR008, BR009, BR010  
**Compliance**: Income Tax Assessment Act

---

### REQ004: Budget Repair Levy
**Priority**: Medium  
**Source**: TaxCalculationService.CalculateOtherLeviesAsync()

**Description**: The system shall calculate Budget Repair Levy for high-income earners.

**Acceptance Criteria**:
- System must check income against Budget Repair Levy threshold
- If income > threshold, levy = income × levy_rate
- Levy must be included in total levies calculation
- System must retrieve levy parameters by financial year

**Business Rules**: BR007  
**Compliance**: Temporary budget repair legislation

---

### REQ005: Annual Tax Summary Generation
**Priority**: High  
**Source**: UserTaxService.CalculateAnnualTaxAsync()

**Description**: The system shall generate comprehensive annual tax summaries for users.

**Acceptance Criteria**:
- System must validate 12 months of income data exists
- System must calculate total gross income, deductions, and taxable income
- System must generate tax calculation using aggregated annual figures
- Summary must include monthly breakdown of income and deductions
- System must save generated summary for future retrieval
- Summary must include effective and marginal tax rates

**Business Rules**: BR013, BR016  
**Dependencies**: REQ001, REQ002, REQ003

---

### REQ006: Tax Bracket Management
**Priority**: High  
**Source**: TaxCalculationService.GetTaxBracketsAsync()

**Description**: The system shall manage tax brackets by financial year.

**Acceptance Criteria**:
- System must store tax brackets with financial year association
- Each bracket must have: order, min_income, max_income, tax_rate, fixed_amount
- System must retrieve brackets for specified financial year
- Brackets must be cached for performance (24-hour expiry)
- System must support unlimited brackets per year

**Business Rules**: BR021  
**Data Requirements**: Tax bracket data for each financial year

---

### REQ007: Input Validation
**Priority**: Critical  
**Source**: TaxController, TaxCalculationService

**Description**: The system shall validate all input data for business rule compliance.

**Acceptance Criteria**:
- Taxable income must be ≥ 0
- Financial year must be provided and non-empty
- Tax calculation requests must not be null
- Year parameters for tax bracket retrieval must be provided
- Multi-year comparison must include at least one year
- Tax history requests must be between 1-20 years

**Business Rules**: BR011, BR012, BR018, BR019, BR020, BR017  
**Error Handling**: Return appropriate error messages for validation failures

---

### REQ008: Marginal and Effective Tax Rate Calculation
**Priority**: Medium  
**Source**: TaxCalculationService.GetMarginalTaxRate()

**Description**: The system shall calculate and provide marginal and effective tax rates.

**Acceptance Criteria**:
- Marginal rate = tax rate of highest applicable bracket
- Effective rate = net_tax_payable ÷ taxable_income
- Effective rate = 0 when taxable_income = 0
- Both rates must be included in tax calculation results

**Business Rules**: BR003, BR004  
**Purpose**: Taxpayer transparency and financial planning

---

### REQ009: Multi-Year Tax Comparison
**Priority**: Low  
**Source**: TaxController.CompareTax()

**Description**: The system shall support tax comparison across multiple financial years.

**Acceptance Criteria**:
- System must accept income amount and array of financial years
- System must calculate tax for each specified year
- Results must show comparative analysis
- Implementation marked as future enhancement (NotImplementedException)

**Status**: Planned for future implementation  
**Dependencies**: REQ001, REQ002, REQ003

---

### REQ010: Tax History Reporting
**Priority**: Low  
**Source**: TaxController.GetTaxHistory()

**Description**: The system shall provide historical tax calculations for given income.

**Acceptance Criteria**:
- System must support up to 20 years of tax history
- Default history period is 10 years
- System must calculate tax for each historical year
- Results must show year-over-year comparison
- Implementation marked as future enhancement (NotImplementedException)

**Status**: Planned for future implementation  
**Dependencies**: REQ001, REQ002, REQ003

---

## API Requirements

### REQ011: Tax Calculation API
**Priority**: Critical  
**Source**: TaxController.CalculateTax()

**Endpoint**: POST /api/tax/calculate  
**Description**: Calculate tax for given income and financial year.

**Request Format**:
```json
{
  "TaxableIncome": 75000,
  "FinancialYear": "2023-24",
  "ResidencyStatus": "Resident",
  "IncludeMedicareLevy": true,
  "IncludeOffsets": true
}
```

**Response Format**:
```json
{
  "TaxableIncome": 75000,
  "IncomeTax": 16467,
  "MedicareLevy": 1500,
  "TotalLevies": 1500,
  "TaxOffsets": 700,
  "NetTaxPayable": 17267,
  "EffectiveRate": 0.2302,
  "MarginalRate": 0.325,
  "BracketBreakdown": [...]
}
```

---

### REQ012: Tax Brackets API
**Priority**: High  
**Source**: TaxController.GetTaxBrackets()

**Endpoint**: GET /api/tax/brackets/{year}  
**Description**: Retrieve tax brackets for specified financial year.

**Response Format**:
```json
[
  {
    "BracketOrder": 1,
    "MinIncome": 0,
    "MaxIncome": 18200,
    "TaxRate": 0,
    "FixedAmount": 0
  },
  {
    "BracketOrder": 2,
    "MinIncome": 18201,
    "MaxIncome": 45000,
    "TaxRate": 0.19,
    "FixedAmount": 0
  }
]
```

---

## Data Requirements

### Data Entities

1. **TaxBracket**
   - BracketOrder (int)
   - MinIncome (decimal)
   - MaxIncome (decimal?)
   - TaxRate (decimal)
   - FixedAmount (decimal)
   - FinancialYear (string)

2. **TaxLevy**
   - LevyType (string)
   - ThresholdIncome (decimal)
   - LevyRate (decimal)
   - FinancialYear (string)

3. **TaxOffset**
   - OffsetType (string)
   - MaxOffset (decimal)
   - MaxIncome (decimal?)
   - PhaseOutStart (decimal?)
   - PhaseOutRate (decimal?)
   - FinancialYear (string)

4. **UserMonthlyIncome**
   - UserId (Guid)
   - FinancialYear (string)
   - Month (int)
   - GrossIncome (decimal)
   - DeductionsAmount (decimal)
   - TaxableIncome (decimal)
   - IncomeType (string)

---

## Performance Requirements

### REQ013: Response Time
- Tax calculation API responses must complete within 2 seconds
- Tax bracket retrieval must complete within 500ms
- Database queries must be optimized for performance

### REQ014: Caching Strategy
- Tax brackets must be cached for 24 hours
- Cache keys must include financial year for isolation
- Cache invalidation must be supported for data updates

---

## Security Requirements

### REQ015: Input Sanitization
- All user inputs must be validated and sanitized
- SQL injection prevention through parameterized queries
- Cross-site scripting (XSS) prevention in API responses

### REQ016: Error Handling
- Sensitive information must not be exposed in error messages
- All errors must be logged for audit purposes
- Generic error messages for external API consumers

---

## Integration Requirements

### REQ017: Database Integration
- Support for SQL Server database
- Repository pattern for data access abstraction
- Connection factory for database connection management

### REQ018: Logging Integration
- Comprehensive logging for all business operations
- Structured logging with correlation IDs
- Log levels: Information, Warning, Error

### REQ019: Dependency Injection
- Support for Autofac dependency injection container
- Service lifetime management
- Configuration-based dependency registration

---

## Compliance and Regulatory Requirements

### REQ020: Australian Taxation Office Compliance
- All tax calculations must comply with current ATO regulations
- Tax brackets must be updated annually
- System must support historical tax rules for previous years

### REQ021: Data Retention
- Tax calculation history must be retained for audit purposes
- User income data must be securely stored
- Compliance with privacy legislation

---

## Non-Functional Requirements

### REQ022: Scalability
- System must support concurrent users
- Database must handle large volumes of tax calculations
- API must support load balancing

### REQ023: Maintainability
- Clear separation between business logic and infrastructure
- Comprehensive unit test coverage
- Documentation for all business rules

### REQ024: Reliability
- System must have 99.5% uptime
- Graceful error handling and recovery
- Data consistency across all operations

---

## Future Enhancements

1. **Multi-year Tax Comparison** (REQ009)
2. **Tax History Reporting** (REQ010)
3. **Real-time Tax Updates**
4. **Mobile Application Support**
5. **Integration with Accounting Systems**
6. **Advanced Tax Optimization Features**

---

## Requirements Traceability Matrix

| Requirement | Business Rules | Source Files | Test Coverage |
|-------------|----------------|--------------|---------------|
| REQ001 | BR001, BR002 | TaxCalculationService.cs | Required |
| REQ002 | BR005, BR006 | TaxCalculationService.cs | Required |
| REQ003 | BR008, BR009, BR010 | TaxCalculationService.cs | Required |
| REQ004 | BR007 | TaxCalculationService.cs | Required |
| REQ005 | BR013, BR016 | UserTaxService.cs | Required |
| REQ007 | BR011, BR012, BR018-BR020 | TaxController.cs | Required |

---

## Change Management

All requirements changes must:
1. Be approved by business stakeholders
2. Update corresponding business rules
3. Include impact analysis
4. Update test cases
5. Maintain backward compatibility where possible
