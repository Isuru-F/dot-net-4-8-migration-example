using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TaxCalculator.Core.Models;
using TaxCalculator.Data.Interfaces;
using TaxCalculator.Services.Interfaces;
using TaxCalculator.Services.Services;

namespace TaxCalculator.Tests.Unit.Services
{
    [TestFixture]
    public class UserTaxServiceTests
    {
        private IUserTaxService _userTaxService;
        private Mock<IUserIncomeRepository> _mockUserIncomeRepository;
        private Mock<ITaxCalculationService> _mockTaxCalculationService;
        private Mock<ILogger> _mockLogger;

        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly string _testFinancialYear = "2024-25";

        [SetUp]
        public void Setup()
        {
            _mockUserIncomeRepository = new Mock<IUserIncomeRepository>();
            _mockTaxCalculationService = new Mock<ITaxCalculationService>();
            _mockLogger = new Mock<ILogger>();

            _userTaxService = new UserTaxService(
                _mockUserIncomeRepository.Object,
                _mockTaxCalculationService.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task CalculateAnnualTaxAsync_ValidInput_ReturnsCorrectSummary()
        {
            // Arrange
            var monthlyIncomes = CreateCompleteMonthlyIncomeData();
            var taxResult = CreateSampleTaxResult();

            _mockUserIncomeRepository.Setup(x => x.GetMonthlyIncomeAsync(_testUserId, _testFinancialYear))
                                   .ReturnsAsync(monthlyIncomes);

            _mockTaxCalculationService.Setup(x => x.CalculateTaxAsync(It.IsAny<TaxCalculationRequest>()))
                                     .ReturnsAsync(taxResult);

            _mockUserIncomeRepository.Setup(x => x.SaveAnnualSummaryAsync(It.IsAny<UserAnnualTaxSummary>()))
                                   .Returns(Task.CompletedTask);

            // Act
            var result = await _userTaxService.CalculateAnnualTaxAsync(_testUserId, _testFinancialYear);

            // Assert
            Assert.That(result.UserId, Is.EqualTo(_testUserId));
            Assert.That(result.FinancialYear, Is.EqualTo(_testFinancialYear));
            Assert.That(result.TotalGrossIncome, Is.EqualTo(120000m)); // 12 months * 10,000
            Assert.That(result.TotalDeductions, Is.EqualTo(12000m)); // 12 months * 1,000
            Assert.That(result.TotalTaxableIncome, Is.EqualTo(108000m)); // 12 months * 9,000
            Assert.That(result.IncomeTaxPayable, Is.EqualTo(taxResult.IncomeTax));
            Assert.That(result.MedicareLevyPayable, Is.EqualTo(taxResult.MedicareLevy));
            Assert.That(result.NetTaxPayable, Is.EqualTo(taxResult.NetTaxPayable));
            Assert.That(result.MonthlyBreakdown.Count, Is.EqualTo(12));
            Assert.That(result.LastCalculated, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));

            // Verify calls
            _mockUserIncomeRepository.Verify(x => x.GetMonthlyIncomeAsync(_testUserId, _testFinancialYear), Times.Once);
            _mockTaxCalculationService.Verify(x => x.CalculateTaxAsync(It.Is<TaxCalculationRequest>(r => 
                r.TaxableIncome == 108000m && 
                r.FinancialYear == _testFinancialYear &&
                r.ResidencyStatus == "Resident" &&
                r.IncludeMedicareLevy == true &&
                r.IncludeOffsets == true)), Times.Once);
            _mockUserIncomeRepository.Verify(x => x.SaveAnnualSummaryAsync(It.IsAny<UserAnnualTaxSummary>()), Times.Once);
        }

        [Test]
        public async Task CalculateAnnualTaxAsync_IncompleteMonthlyData_ThrowsInvalidOperationException()
        {
            // Arrange
            var incompleteMonthlyIncomes = CreateCompleteMonthlyIncomeData().Take(8).ToList(); // Only 8 months

            _mockUserIncomeRepository.Setup(x => x.GetMonthlyIncomeAsync(_testUserId, _testFinancialYear))
                                   .ReturnsAsync(incompleteMonthlyIncomes);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userTaxService.CalculateAnnualTaxAsync(_testUserId, _testFinancialYear));

            Assert.That(ex.Message, Contains.Substring("Incomplete monthly income data"));
            Assert.That(ex.Message, Contains.Substring("Found 8 months, expected 12"));

            // Verify tax calculation was not called
            _mockTaxCalculationService.Verify(x => x.CalculateTaxAsync(It.IsAny<TaxCalculationRequest>()), Times.Never);
        }

        [Test]
        public async Task CalculateAnnualTaxAsync_ZeroIncome_HandlesCorrectly()
        {
            // Arrange
            var monthlyIncomes = CreateMonthlyIncomeDataWithZeros();
            var taxResult = new TaxCalculationResult
            {
                TaxableIncome = 0m,
                IncomeTax = 0m,
                MedicareLevy = 0m,
                NetTaxPayable = 0m,
                EffectiveRate = 0m,
                MarginalRate = 0m
            };

            _mockUserIncomeRepository.Setup(x => x.GetMonthlyIncomeAsync(_testUserId, _testFinancialYear))
                                   .ReturnsAsync(monthlyIncomes);

            _mockTaxCalculationService.Setup(x => x.CalculateTaxAsync(It.IsAny<TaxCalculationRequest>()))
                                     .ReturnsAsync(taxResult);

            _mockUserIncomeRepository.Setup(x => x.SaveAnnualSummaryAsync(It.IsAny<UserAnnualTaxSummary>()))
                                   .Returns(Task.CompletedTask);

            // Act
            var result = await _userTaxService.CalculateAnnualTaxAsync(_testUserId, _testFinancialYear);

            // Assert
            Assert.That(result.TotalGrossIncome, Is.EqualTo(0m));
            Assert.That(result.TotalTaxableIncome, Is.EqualTo(0m));
            Assert.That(result.IncomeTaxPayable, Is.EqualTo(0m));
            Assert.That(result.NetTaxPayable, Is.EqualTo(0m));
        }

        [Test]
        public async Task GetMonthlyIncomeAsync_ValidInput_ReturnsRepositoryResult()
        {
            // Arrange
            var expectedIncomes = CreateCompleteMonthlyIncomeData();

            _mockUserIncomeRepository.Setup(x => x.GetMonthlyIncomeAsync(_testUserId, _testFinancialYear))
                                   .ReturnsAsync(expectedIncomes);

            // Act
            var result = await _userTaxService.GetMonthlyIncomeAsync(_testUserId, _testFinancialYear);

            // Assert
            Assert.That(result, Is.EqualTo(expectedIncomes));
            _mockUserIncomeRepository.Verify(x => x.GetMonthlyIncomeAsync(_testUserId, _testFinancialYear), Times.Once);
        }

        [Test]
        public async Task SaveMonthlyIncomeAsync_ValidInput_CallsRepository()
        {
            // Arrange
            var income = new UserMonthlyIncome
            {
                UserId = _testUserId,
                FinancialYear = _testFinancialYear,
                Month = 1,
                GrossIncome = 10000m,
                DeductionsAmount = 1000m,
                TaxableIncome = 9000m
            };

            _mockUserIncomeRepository.Setup(x => x.SaveMonthlyIncomeAsync(income))
                                   .Returns(Task.CompletedTask);

            // Act
            await _userTaxService.SaveMonthlyIncomeAsync(income);

            // Assert
            _mockUserIncomeRepository.Verify(x => x.SaveMonthlyIncomeAsync(income), Times.Once);
        }

        [Test]
        public async Task GetAnnualSummaryAsync_ValidInput_ReturnsRepositoryResult()
        {
            // Arrange
            var expectedSummary = CreateSampleAnnualSummary();

            _mockUserIncomeRepository.Setup(x => x.GetAnnualSummaryAsync(_testUserId, _testFinancialYear))
                                   .ReturnsAsync(expectedSummary);

            // Act
            var result = await _userTaxService.GetAnnualSummaryAsync(_testUserId, _testFinancialYear);

            // Assert
            Assert.That(result, Is.EqualTo(expectedSummary));
            _mockUserIncomeRepository.Verify(x => x.GetAnnualSummaryAsync(_testUserId, _testFinancialYear), Times.Once);
        }

        [Test]
        public async Task GetTaxHistoryAsync_WithDefaultYears_ReturnsRepositoryResult()
        {
            // Arrange
            var expectedHistory = new List<UserAnnualTaxSummary>
            {
                CreateSampleAnnualSummary(),
                CreateSampleAnnualSummary()
            };

            _mockUserIncomeRepository.Setup(x => x.GetTaxHistoryAsync(_testUserId, 5))
                                   .ReturnsAsync(expectedHistory);

            // Act
            var result = await _userTaxService.GetTaxHistoryAsync(_testUserId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedHistory));
            _mockUserIncomeRepository.Verify(x => x.GetTaxHistoryAsync(_testUserId, 5), Times.Once);
        }

        [Test]
        public async Task GetTaxHistoryAsync_WithCustomYears_ReturnsRepositoryResult()
        {
            // Arrange
            var customYears = 3;
            var expectedHistory = new List<UserAnnualTaxSummary>
            {
                CreateSampleAnnualSummary()
            };

            _mockUserIncomeRepository.Setup(x => x.GetTaxHistoryAsync(_testUserId, customYears))
                                   .ReturnsAsync(expectedHistory);

            // Act
            var result = await _userTaxService.GetTaxHistoryAsync(_testUserId, customYears);

            // Assert
            Assert.That(result, Is.EqualTo(expectedHistory));
            _mockUserIncomeRepository.Verify(x => x.GetTaxHistoryAsync(_testUserId, customYears), Times.Once);
        }

        [Test]
        public async Task CalculateAnnualTaxAsync_LogsInformation()
        {
            // Arrange
            var monthlyIncomes = CreateCompleteMonthlyIncomeData();
            var taxResult = CreateSampleTaxResult();

            _mockUserIncomeRepository.Setup(x => x.GetMonthlyIncomeAsync(_testUserId, _testFinancialYear))
                                   .ReturnsAsync(monthlyIncomes);
            _mockTaxCalculationService.Setup(x => x.CalculateTaxAsync(It.IsAny<TaxCalculationRequest>()))
                                     .ReturnsAsync(taxResult);
            _mockUserIncomeRepository.Setup(x => x.SaveAnnualSummaryAsync(It.IsAny<UserAnnualTaxSummary>()))
                                   .Returns(Task.CompletedTask);

            // Act
            await _userTaxService.CalculateAnnualTaxAsync(_testUserId, _testFinancialYear);

            // Assert
            _mockLogger.Verify(x => x.LogInformation(It.Is<string>(s => 
                s.Contains("Calculating annual tax") && 
                s.Contains(_testUserId.ToString()) && 
                s.Contains(_testFinancialYear))), Times.Once);
        }

        private List<UserMonthlyIncome> CreateCompleteMonthlyIncomeData()
        {
            var incomes = new List<UserMonthlyIncome>();
            for (int month = 1; month <= 12; month++)
            {
                incomes.Add(new UserMonthlyIncome
                {
                    UserId = _testUserId,
                    FinancialYear = _testFinancialYear,
                    Month = month,
                    GrossIncome = 10000m,
                    DeductionsAmount = 1000m,
                    TaxableIncome = 9000m,
                    IncomeType = "Salary"
                });
            }
            return incomes;
        }

        private List<UserMonthlyIncome> CreateMonthlyIncomeDataWithZeros()
        {
            var incomes = new List<UserMonthlyIncome>();
            for (int month = 1; month <= 12; month++)
            {
                incomes.Add(new UserMonthlyIncome
                {
                    UserId = _testUserId,
                    FinancialYear = _testFinancialYear,
                    Month = month,
                    GrossIncome = 0m,
                    DeductionsAmount = 0m,
                    TaxableIncome = 0m,
                    IncomeType = "Salary"
                });
            }
            return incomes;
        }

        private TaxCalculationResult CreateSampleTaxResult()
        {
            return new TaxCalculationResult
            {
                TaxableIncome = 108000m,
                IncomeTax = 22800m,
                MedicareLevy = 2160m,
                BudgetRepairLevy = 0m,
                TotalLevies = 2160m,
                TaxOffsets = 700m,
                NetTaxPayable = 24260m,
                EffectiveRate = 0.2246m,
                MarginalRate = 0.30m
            };
        }

        private UserAnnualTaxSummary CreateSampleAnnualSummary()
        {
            return new UserAnnualTaxSummary
            {
                UserId = _testUserId,
                FinancialYear = _testFinancialYear,
                TotalGrossIncome = 120000m,
                TotalDeductions = 12000m,
                TotalTaxableIncome = 108000m,
                IncomeTaxPayable = 22800m,
                MedicareLevyPayable = 2160m,
                NetTaxPayable = 24260m,
                EffectiveTaxRate = 0.2246m,
                LastCalculated = DateTime.UtcNow
            };
        }
    }
}
