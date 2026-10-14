using HidroSense.Api.Services;
using HidroSense.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HidroSense.Api.Controllers;

[ApiController]
[Route("api/v1/consumption")]
[Authorize]
public class ConsumptionController : ControllerBase
{
    private readonly IConsumptionService _service;
    public ConsumptionController(IConsumptionService service) => _service = service;

    [HttpPost("readings")]
    [ProducesResponseType(typeof(ConsumptionReadingResultViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsumptionReadingResultViewModel>> Ingest(
        [FromBody] CreateConsumptionReadingViewModel input, CancellationToken ct)
    {
        var result = await _service.IngestAsync(input, ct);
        return CreatedAtAction(nameof(List), new { id = result.ReadingId }, result);
    }

    [HttpGet("readings")]
    [ProducesResponseType(typeof(PagedResult<ConsumptionReadingViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ConsumptionReadingViewModel>>> List(
        [FromQuery] ConsumptionReadingFilter filter, CancellationToken ct)
        => Ok(await _service.ListAsync(filter, ct));
}
