using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYears.Commands;
using Drmc.Procurement.Application.Features.PlanningYears.DTO;
using Drmc.Procurement.Application.Features.PlanningYears.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningYearsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PlanningYearDto>>> GetAll()
    {
        var planningYears = await dispatcher.Send(new GetAllPlanningYearsQuery(), HttpContext.RequestAborted);
        return Ok(planningYears);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlanningYearDto>> GetById(int id)
    {
        var planningYear = await dispatcher.Send(new GetPlanningYearByIdQuery(id), HttpContext.RequestAborted);
        if (planningYear is null) return NotFound(new { message = "Planning year not found." });
        return Ok(planningYear);
    }

    [HttpPost]
    public async Task<ActionResult<PlanningYearDto>> Create([FromBody] CreatePlanningYearRequest request)
    {
        try
        {
            var planningYear = await dispatcher.Send(new CreatePlanningYearCommand(request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = planningYear.PlanningYearId }, planningYear);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PlanningYearDto>> Update(int id, [FromBody] CreatePlanningYearRequest request)
    {
        try
        {
            var planningYear = await dispatcher.Send(new UpdatePlanningYearCommand(id, request), HttpContext.RequestAborted);
            if (planningYear is null) return NotFound(new { message = "Planning year not found." });
            return Ok(planningYear);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        try
        {
            var updated = await dispatcher.Send(new UpdatePlanningYearStatusCommand(id, status), HttpContext.RequestAborted);
            if (!updated) return NotFound(new { message = "Planning year not found." });
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
