using Microsoft.AspNetCore.Mvc;

namespace TaxCalculator.AspNetCore.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "OK", timestamp = DateTime.UtcNow });
    }
}
