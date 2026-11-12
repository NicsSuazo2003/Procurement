using System.Security.Claims;
using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Commands;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[Authorize(Roles = "End-user")]
[ApiController]
[Route("api/[controller]")]
public class UserCategoryPreferencesController(IDispatcher dispatcher) : ControllerBase
{
    private int? GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    [HttpGet("{planningYearId:int}")]
    public async Task<IActionResult> GetByPlanningYear(int planningYearId)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Forbid();
        var preferences = await dispatcher.Send(new GetUserCategoryPreferencesQuery(userId.Value, planningYearId), HttpContext.RequestAborted);
        return Ok(preferences);
    }

    [HttpPut]
    public async Task<IActionResult> Save([FromBody] SaveUserCategoryPreferencesRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Forbid();
        if (request.PlanningYearId <= 0) return BadRequest(new { message = "A valid planning year is required." });
        if (request.PlanningYearCategoryIds == null) return BadRequest(new { message = "Category selections are required." });
        try
        {
            var result = await dispatcher.Send(new SaveUserCategoryPreferencesCommand(userId.Value, request), HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
