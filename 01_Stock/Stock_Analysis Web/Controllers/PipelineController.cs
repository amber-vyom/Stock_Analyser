using Microsoft.AspNetCore.Mvc;
using Stock_Analysis.Engine.NSE;
using Stock_Analysis.Engine.Services;

namespace Stock_Analysis.Web.Controllers;

[ApiController]
[Route("api/pipeline")]
public class PipelineController : ControllerBase
{
    private static readonly SemaphoreSlim RunLock = new(1, 1);

    private readonly NsePipelineService pipeline;
    private readonly ILogger<PipelineController> logger;

    public PipelineController(NseDownloader downloader, ILogger<PipelineController> logger)
    {
        pipeline = new NsePipelineService(downloader);
        this.logger = logger;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run()
    {
        if (!await RunLock.WaitAsync(0))
        {
            return Conflict(new { error = "A data update is already running." });
        }

        try
        {
            var summary = await pipeline.RunAllAsync();

            if (summary.CsvFilesUsed == 0)
            {
                return StatusCode(502, new
                {
                    error = "No NSE reports could be downloaded.",
                    downloads = summary.Downloads
                });
            }

            return Ok(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pipeline run failed");
            return StatusCode(500, new { error = ex.Message });
        }
        finally
        {
            RunLock.Release();
        }
    }
}
