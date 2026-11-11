
using System.Security.Claims;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[Authorize(Roles = "End-user")]
[ApiController]
[Route("api/[controller]")]
public class UserCategoryPreferencesController : ControllerBase
{
    private readonly IUserCategoryPreferenceService _service;

    public UserCategoryPreferencesController(
        IUserCategoryPreferenceService service)
    {
        _service = service;
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var userId)
            ? userId
            : null;
    }

    // GET: api/UserCategoryPreferences/1
    [HttpGet("{planningYearId:int}")]
    public async Task<IActionResult> GetByPlanningYear(int planningYearId)
    {
        var userId = GetUserId();

        if (!userId.HasValue)
            return Forbid();

        var preferences = await _service
            .GetByUserAndPlanningYearAsync(
                userId.Value,
                planningYearId);

        return Ok(preferences);
    }

    // PUT: api/UserCategoryPreferences
    [HttpPut]
    public async Task<IActionResult> Save(
        [FromBody] SaveUserCategoryPreferencesRequest request)
    {
        var userId = GetUserId();

        if (!userId.HasValue)
            return Forbid();

        if (request.PlanningYearId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid planning year is required."
            });
        }

        if (request.PlanningYearCategoryIds == null)
        {
            return BadRequest(new
            {
                message = "Category selections are required."
            });
        }

        try
        {
            var result = await _service.SaveAsync(
                userId.Value,
                request);

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
