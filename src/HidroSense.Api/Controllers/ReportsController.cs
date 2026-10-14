using HidroSense.Api.Services;
using HidroSense.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HidroSense.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = "Admin,Operator")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    public ReportsController(IReportService service) => _service = service;

    [HttpGet("water-impact")]
    [ProducesResponseType(typeof(WaterImpactReportViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<WaterImpactReportViewModel>> WaterImpact(
        [FromQuery] WaterImpactReportQuery query, CancellationToken ct)
        => Ok(await _service.BuildWaterImpactAsync(query, ct));
}
