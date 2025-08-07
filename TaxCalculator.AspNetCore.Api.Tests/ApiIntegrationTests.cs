using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using TaxCalculator.Core.Models;
using TaxCalculator.Data.Interfaces;
using TaxCalculator.Services.Interfaces;

namespace TaxCalculator.AspNetCore.Api.Tests;

public class ApiIntegrationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real repository and add a mock
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITaxBracketRepository));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add mock repository
                    var mockRepository = new Mock<ITaxBracketRepository>();
                    SetupMockTaxBrackets(mockRepository);
                    services.AddSingleton(mockRepository.Object);
                });
            });
        _client = _factory.CreateClient();
    }

    private void SetupMockTaxBrackets(Mock<ITaxBracketRepository> mockRepository)
    {
        var brackets2024_25 = new List<TaxBracket>
        {
            new TaxBracket { Id = 1, FinancialYear = "2024-25", MinIncome = 0, MaxIncome = 18200, TaxRate = 0m, FixedAmount = 0m, BracketOrder = 1, IsActive = true, CreatedDate = DateTime.Now },
            new TaxBracket { Id = 2, FinancialYear = "2024-25", MinIncome = 18201, MaxIncome = 45000, TaxRate = 0.16m, FixedAmount = 0m, BracketOrder = 2, IsActive = true, CreatedDate = DateTime.Now },
            new TaxBracket { Id = 3, FinancialYear = "2024-25", MinIncome = 45001, MaxIncome = 135000, TaxRate = 0.30m, FixedAmount = 4288m, BracketOrder = 3, IsActive = true, CreatedDate = DateTime.Now },
            new TaxBracket { Id = 4, FinancialYear = "2024-25", MinIncome = 135001, MaxIncome = 190000, TaxRate = 0.37m, FixedAmount = 31288m, BracketOrder = 4, IsActive = true, CreatedDate = DateTime.Now },
            new TaxBracket { Id = 5, FinancialYear = "2024-25", MinIncome = 190001, MaxIncome = null, TaxRate = 0.45m, FixedAmount = 51638m, BracketOrder = 5, IsActive = true, CreatedDate = DateTime.Now }
        };

        var brackets2023_24 = new List<TaxBracket>
        {
            new TaxBracket { Id = 6, FinancialYear = "2023-24", MinIncome = 0, MaxIncome = 18200, TaxRate = 0m, FixedAmount = 0m, BracketOrder = 1, IsActive = true, CreatedDate = DateTime.Now },
            new TaxBracket { Id = 7, FinancialYear = "2023-24", MinIncome = 18201, MaxIncome = 45000, TaxRate = 0.19m, FixedAmount = 0m, BracketOrder = 2, IsActive = true, CreatedDate = DateTime.Now },
            new TaxBracket { Id = 8, FinancialYear = "2023-24", MinIncome = 45001, MaxIncome = 120000, TaxRate = 0.325m, FixedAmount = 5092m, BracketOrder = 3, IsActive = true, CreatedDate = DateTime.Now },
            new TaxBracket { Id = 9, FinancialYear = "2023-24", MinIncome = 120001, MaxIncome = 180000, TaxRate = 0.37m, FixedAmount = 29467m, BracketOrder = 4, IsActive = true, CreatedDate = DateTime.Now },
            new TaxBracket { Id = 10, FinancialYear = "2023-24", MinIncome = 180001, MaxIncome = null, TaxRate = 0.45m, FixedAmount = 51667m, BracketOrder = 5, IsActive = true, CreatedDate = DateTime.Now }
        };

        mockRepository.Setup(x => x.GetTaxBracketsAsync("2024-25")).ReturnsAsync(brackets2024_25);
        mockRepository.Setup(x => x.GetTaxBracketsAsync("2023-24")).ReturnsAsync(brackets2023_24);
        
        // Setup for tax offsets (empty for now)
        mockRepository.Setup(x => x.GetTaxOffsetsAsync(It.IsAny<string>())).ReturnsAsync(new List<TaxOffset>());
        
        // Setup for tax levies (empty for now)
        mockRepository.Setup(x => x.GetTaxLeviesAsync(It.IsAny<string>())).ReturnsAsync(new List<TaxLevy>());
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var healthResponse = JsonConvert.DeserializeObject<dynamic>(content);
        Assert.That(healthResponse?.status?.ToString(), Is.EqualTo("OK"));
        Assert.That(healthResponse?.timestamp, Is.Not.Null);
    }

    [Test]
    public async Task TaxCalculate_WithValidRequest_ReturnsCalculation()
    {
        // Arrange
        var request = new TaxCalculationRequest
        {
            TaxableIncome = 50000m,
            FinancialYear = "2024-25"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/tax/calculate", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var result = JsonConvert.DeserializeObject<TaxCalculationResult>(responseContent);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.NetTaxPayable, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task TaxCalculate_WithNegativeIncome_ReturnsBadRequest()
    {
        // Arrange
        var request = new TaxCalculationRequest
        {
            TaxableIncome = -1000m,
            FinancialYear = "2024-25"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/tax/calculate", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task TaxCalculate_WithNullRequest_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("null", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/tax/calculate", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetTaxBrackets_WithValidYear_ReturnsBrackets()
    {
        // Act
        var response = await _client.GetAsync("/api/tax/brackets/2024-25");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var brackets = JsonConvert.DeserializeObject<List<TaxBracket>>(content);
        Assert.That(brackets, Is.Not.Null);
        Assert.That(brackets, Is.Not.Empty);
    }

    [Test]
    public async Task CompareTax_WithValidParameters_ReturnsComparison()
    {
        // Act
        var response = await _client.GetAsync("/api/tax/compare?income=75000&years=2023-24&years=2024-25");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var comparison = JsonConvert.DeserializeObject<List<object>>(content);
        Assert.That(comparison, Is.Not.Null);
    }

    [Test]
    public async Task GetTaxHistory_WithValidIncome_ReturnsHistory()
    {
        // Act
        var response = await _client.GetAsync("/api/tax/history/60000?years=5");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var history = JsonConvert.DeserializeObject<List<object>>(content);
        Assert.That(history, Is.Not.Null);
    }

    [Test]
    public async Task GetTaxHistory_WithNegativeIncome_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/tax/history/-1000");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetTaxHistory_WithInvalidYears_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/tax/history/60000?years=25");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
