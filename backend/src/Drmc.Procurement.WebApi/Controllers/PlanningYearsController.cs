using Drmc.Procurement.Application.Features.PlanningYears.DTO;
using Drmc.Procurement.Application.Features.PlanningYears.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningYearsController : ControllerBase
{
    private readonly IPlanningYearService _planningYearService;

    public PlanningYearsController(
        IPlanningYearService planningYearService)
    {
        _planningYearService = planningYearService;
    }

    // GET: api/planningyears
    [HttpGet]
    public async Task<ActionResult<List<PlanningYearDto>>> GetAll()
    {
        var planningYears = await _planningYearService.GetAllAsync();

        return Ok(planningYears);
    }

    // GET: api/planningyears/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlanningYearDto>> GetById(int id)
    {
        var planningYear = await _planningYearService.GetByIdAsync(id);

        if (planningYear is null)
        {
            return NotFound(new
            {
                message = "Planning year not found."
            });
        }

        return Ok(planningYear);
    }

    // POST: api/planningyears
    [HttpPost]
    public async Task<ActionResult<PlanningYearDto>> Create(
        [FromBody] CreatePlanningYearRequest request)
    {
        try
        {
            var planningYear =
                await _planningYearService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = planningYear.PlanningYearId },
                planningYear);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT: api/planningyears/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PlanningYearDto>> Update(
        int id,
        [FromBody] CreatePlanningYearRequest request)
    {
        try
        {
            var planningYear =
                await _planningYearService.UpdateAsync(id, request);

            if (planningYear is null)
            {
                return NotFound(new
                {
                    message = "Planning year not found."
                });
            }

            return Ok(planningYear);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PATCH: api/planningyears/1/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] string status)
    {
        try
        {
            var updated = await _planningYearService.UpdateStatusAsync(
                id,
                status);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Planning year not found."
                });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}