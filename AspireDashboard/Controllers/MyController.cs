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
        logger.LogInformation("MANUAL: Request started at {Date}", DateTime.UtcNow.Date);
        logger.LogUselessInfo(DateTime.UtcNow.Date);
        
        Diagnostics.Counter.Add(1,
            new KeyValuePair<string, object?>("type", "MyType"),
            new KeyValuePair<string, object?>("id", 123),
            new KeyValuePair<string, object?>("date", DateTime.UtcNow.ToShortDateString()));
        
        return Ok("privet");
    }

    [HttpGet]
    [Route("Test")]
    public async Task<IActionResult> Test()
    {

        return Ok(new
        {
            Value = 123,
            NullValue = (string?)null
        });
    }
}

public static partial class Log
{
    [LoggerMessage(LogLevel.Information, "CACHED: Request started at {date}")]
    public static partial void LogUselessInfo(this ILogger logger, DateTime date);
}
