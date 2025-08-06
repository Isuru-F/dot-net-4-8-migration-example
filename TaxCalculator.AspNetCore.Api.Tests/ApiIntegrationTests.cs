using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using TaxCalculator.Core.Models;

namespace TaxCalculator.AspNetCore.Api.Tests;

public class ApiIntegrationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
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
