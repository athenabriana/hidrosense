using System.Security.Claims;
using HidroSense.Api.Services;
using HidroSense.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HidroSense.Api.Controllers;

[ApiController]
[Route("api/v1/leak-alerts")]
[Authorize]
public class LeakAlertsController : ControllerBase
{
    private readonly ILeakAlertService _service;
    public LeakAlertsController(ILeakAlertService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LeakAlertViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LeakAlertViewModel>>> List(
        [FromQuery] LeakAlertFilter filter, CancellationToken ct)
        => Ok(await _service.ListAsync(filter, ct));

    [HttpPatch("{id:long}/resolve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(LeakAlertViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeakAlertViewModel>> Resolve(
        long id, [FromBody] ResolveLeakAlertViewModel input, CancellationToken ct)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name)
            ?? User.Identity?.Name ?? "unknown";
        return Ok(await _service.ResolveAsync(id, actor, input, ct));
    }
}
