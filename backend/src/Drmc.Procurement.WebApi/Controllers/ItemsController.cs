using Drmc.Procurement.Application.Features.Items.DTO;
using Drmc.Procurement.Application.Features.Items.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;

    public ItemsController(IItemService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("category/{categoryId:int}")]
    public async Task<IActionResult> GetByCategoryId(int categoryId)
    {
        return Ok(await _service.GetByCategoryIdAsync(categoryId));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("planning-year/{planningYear:int}/category/{categoryId:int}")]
public async Task<IActionResult> GetByPlanningYearAndCategory(
    int planningYear,
    int categoryId)
{
    var result = await _service.GetByPlanningYearAndCategoryAsync(
        planningYear,
        categoryId);

    return Ok(result);
}

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateItemRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.ItemId },
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
        [FromBody] UpdateItemRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);

            if (result is null)
                return NotFound();

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
    public async Task<IActionResult> SetActiveStatus(
        int id,
        [FromBody] bool isActive)
    {
        var updated = await _service.SetActiveStatusAsync(id, isActive);

        if (!updated)
            return NotFound();

        return NoContent();
    }
}