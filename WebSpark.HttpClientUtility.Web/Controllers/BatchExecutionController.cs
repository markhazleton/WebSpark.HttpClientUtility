using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Web.Models;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Web.Controllers;

public sealed class BatchExecutionController : Controller
{
    private readonly BatchExecutionDemoService _demoService;

    public BatchExecutionController(BatchExecutionDemoService demoService)
    {
        _demoService = demoService;
    }

    [HttpGet("/BatchExecution")]
    public IActionResult Index()
    {
        return View(new BatchExecutionPageViewModel());
    }

    [HttpPost("/BatchExecution/runs")]
    public async Task<IActionResult> StartRun([FromBody] DemoStartRunRequest? request, CancellationToken ct)
    {
        var normalized = request ?? DemoStartRunRequest.CreateDefault();
        normalized.Environments ??= [];
        normalized.Users ??= [];
        normalized.Requests ??= [];

        var started = await _demoService.StartRunAsync(normalized, ct).ConfigureAwait(false);
        if (!started.Accepted)
        {
            return BadRequest(new { error = started.Error ?? "The demo request could not be started." });
        }

        return Accepted(started.Response);
    }

    [HttpGet("/BatchExecution/runs/{runId}")]
    public IActionResult GetRunStatus(string runId)
    {
        var status = _demoService.GetRunStatus(runId);
        if (status is null)
        {
            return NotFound(new { error = "Run not found or expired." });
        }

        return Ok(status);
    }
}
