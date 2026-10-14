using HidroSense.Api.Services;
using HidroSense.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HidroSense.Api.Controllers;

[ApiController]
[Route("api/v1/consumer-units")]
[Authorize]
public class ConsumerUnitsController : ControllerBase
{
    private readonly IConsumerUnitService _service;
    public ConsumerUnitsController(IConsumerUnitService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ConsumerUnitViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ConsumerUnitViewModel>>> List(
        [FromQuery] ConsumerUnitFilter filter, CancellationToken ct)
        => Ok(await _service.ListAsync(filter, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ConsumerUnitViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConsumerUnitViewModel>> Create(
        [FromBody] CreateConsumerUnitViewModel input, CancellationToken ct)
    {
        var created = await _service.CreateAsync(input, ct);
        return CreatedAtAction(nameof(List), new { id = created.Id }, created);
    }
}
