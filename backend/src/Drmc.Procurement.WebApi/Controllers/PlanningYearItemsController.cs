using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYearItems.Commands;
using Drmc.Procurement.Application.Features.PlanningYearItems.DTO;
using Drmc.Procurement.Application.Features.PlanningYearItems.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningYearItemsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await dispatcher.Send(new GetAllPlanningYearItemsQuery(), HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpGet("planning-year-category/{planningYearCategoryId:int}")]
    public async Task<IActionResult> GetByPlanningYearCategory(int planningYearCategoryId)
    {
        var result = await dispatcher.Send(new GetPlanningYearItemsByPlanningYearCategoryQuery(planningYearCategoryId), HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await dispatcher.Send(new GetPlanningYearItemByIdQuery(id), HttpContext.RequestAborted);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(AssignPlanningYearItemRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new AssignPlanningYearItemCommand(request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = result.PlanningYearItemId }, result);
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePlanningYearItemRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new UpdatePlanningYearItemCommand(id, request), HttpContext.RequestAborted);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/active-status")]
    public async Task<IActionResult> SetActiveStatus(int id, [FromQuery] bool isActive)
    {
        var result = await dispatcher.Send(new SetPlanningYearItemActiveStatusCommand(id, isActive), HttpContext.RequestAborted);
        if (!result) return NotFound();
        return NoContent();
    }
}
