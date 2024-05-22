using Microsoft.AspNetCore.Mvc;
using OpenTelemetryTest.Meters;

namespace OpenTelemetryTest.Controllers;

[ApiController]
[Route("service")]
public sealed class MyController : ControllerBase
{
    private readonly ILogger<MyController> logger;

    public MyController(ILogger<MyController> logger)
    {
        this.logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        logger.LogInformation("WOW");
        
        Diagnostics.Counter.Add(1,
            new KeyValuePair<string, object?>("type", "MyType"),
            new KeyValuePair<string, object?>("id", 123),
            new KeyValuePair<string, object?>("date", DateTime.UtcNow.ToShortDateString()));
        
        return Ok("privet");
    }
}