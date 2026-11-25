using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Commands;
using Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningYearCategoriesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await dispatcher.Send(new GetAllPlanningYearCategoriesQuery(), HttpContext.RequestAborted));
    }

    [HttpGet("planning-year/{planningYearId:int}")]
    public async Task<IActionResult> GetByPlanningYearId(int planningYearId)
    {
        return Ok(await dispatcher.Send(new GetPlanningYearCategoriesByPlanningYearQuery(planningYearId), HttpContext.RequestAborted));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await dispatcher.Send(new GetPlanningYearCategoryByIdQuery(id), HttpContext.RequestAborted);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] AssignPlanningYearCategoryRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new AssignPlanningYearCategoryCommand(request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = result.PlanningYearCategoryId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/active-status")]
    public async Task<IActionResult> SetActiveStatus(int id, [FromBody] bool isActive)
    {
        var updated = await dispatcher.Send(new SetPlanningYearCategoryActiveStatusCommand(id, isActive), HttpContext.RequestAborted);
        if (!updated) return NotFound();
        return NoContent();
    }
}
