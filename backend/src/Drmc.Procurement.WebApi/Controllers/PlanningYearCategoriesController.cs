using Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningYearCategoriesController : ControllerBase
{
    private readonly IPlanningYearCategoryService _service;

    public PlanningYearCategoriesController(
        IPlanningYearCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("planning-year/{planningYearId:int}")]
    public async Task<IActionResult> GetByPlanningYearId(int planningYearId)
    {
        return Ok(await _service.GetByPlanningYearIdAsync(planningYearId));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(
        [FromBody] AssignPlanningYearCategoryRequest request)
    {
        try
        {
            var result = await _service.AssignAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.PlanningYearCategoryId },
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
