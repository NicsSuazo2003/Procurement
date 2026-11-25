using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Items.Commands;
using Drmc.Procurement.Application.Features.Items.DTO;
using Drmc.Procurement.Application.Features.Items.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await dispatcher.Send(new GetAllItemsQuery(), HttpContext.RequestAborted));
    }

    [HttpGet("category/{categoryId:int}")]
    public async Task<IActionResult> GetByCategoryId(int categoryId)
    {
        return Ok(await dispatcher.Send(new GetItemsByCategoryQuery(categoryId), HttpContext.RequestAborted));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await dispatcher.Send(new GetItemByIdQuery(id), HttpContext.RequestAborted);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("planning-year/{planningYear:int}/category/{categoryId:int}")]
    public async Task<IActionResult> GetByPlanningYearAndCategory(int planningYear, int categoryId)
    {
        var result = await dispatcher.Send(new GetItemsByPlanningYearAndCategoryQuery(planningYear, categoryId), HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new CreateItemCommand(request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = result.ItemId }, result);
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new UpdateItemCommand(id, request), HttpContext.RequestAborted);
            if (result is null) return NotFound();
            return Ok(result);
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
        var updated = await dispatcher.Send(new SetItemActiveStatusCommand(id, isActive), HttpContext.RequestAborted);
        if (!updated) return NotFound();
        return NoContent();
    }
}
