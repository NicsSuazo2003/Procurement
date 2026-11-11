
using Drmc.Procurement.Application.Features.PlanningYearItems.DTO;
using Drmc.Procurement.Application.Features.PlanningYearItems.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningYearItemsController : ControllerBase
{
    private readonly IPlanningYearItemService _service;

    public PlanningYearItemsController(
        IPlanningYearItemService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("planning-year-category/{planningYearCategoryId:int}")]
    public async Task<IActionResult> GetByPlanningYearCategory(
        int planningYearCategoryId)
    {
        var result =
            await _service.GetByPlanningYearCategoryIdAsync(
                planningYearCategoryId);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(
        AssignPlanningYearItemRequest request)
    {
        try
        {
            var result = await _service.AssignAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.PlanningYearItemId },
                result);
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
    public async Task<IActionResult> Update(
        int id,
        UpdatePlanningYearItemRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/active-status")]
    public async Task<IActionResult> SetActiveStatus(
        int id,
        [FromQuery] bool isActive)
    {
        var result = await _service.SetActiveStatusAsync(
            id,
            isActive);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
