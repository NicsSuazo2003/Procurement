using System.Security.Claims;
using Drmc.Procurement.Application.Features.ProcurementCategories.DTO;
using Drmc.Procurement.Application.Features.ProcurementCategories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcurementCategoriesController : ControllerBase
{
    private readonly IProcurementCategoryService _categoryService;

    public ProcurementCategoriesController(
        IProcurementCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: api/ProcurementCategories
    [HttpGet]
    public async Task<ActionResult<List<ProcurementCategoryDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        return Ok(categories);
    }

    // GET: api/ProcurementCategories/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcurementCategoryDto>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category is null)
        {
            return NotFound(new
            {
                message = "Procurement category not found."
            });
        }

        return Ok(category);
    }

    // GET: api/ProcurementCategories/1/history
    [HttpGet("{id:int}/history")]
    [Authorize(Roles = "Procurement Staff,System Administrator")]
    public async Task<ActionResult<List<ProcurementCategoryAuditLogDto>>>
        GetHistory(int id)
    {
        try
        {
            var history = await _categoryService.GetHistoryAsync(id);

            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // POST: api/ProcurementCategories
    [HttpPost]
    [Authorize(Roles = "Procurement Staff,System Administrator")]
    public async Task<ActionResult<ProcurementCategoryDto>> Create(
        [FromBody] CreateProcurementCategoryRequest request)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return Forbid();
        }

        try
        {
            var category = await _categoryService.CreateAsync(
                request,
                userId.Value);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.CategoryId },
                category);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT: api/ProcurementCategories/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Procurement Staff,System Administrator")]
    public async Task<ActionResult<ProcurementCategoryDto>> Update(
        int id,
        [FromBody] CreateProcurementCategoryRequest request)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return Forbid();
        }

        try
        {
            var category = await _categoryService.UpdateAsync(
                id,
                request,
                userId.Value);

            if (category is null)
            {
                return NotFound(new
                {
                    message = "Procurement category not found."
                });
            }

            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PATCH: api/ProcurementCategories/1/active-status
    [HttpPatch("{id:int}/active-status")]
    [Authorize(Roles = "Procurement Staff,System Administrator")]
    public async Task<IActionResult> SetActiveStatus(
        int id,
        [FromBody] bool isActive)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return Forbid();
        }

        try
        {
            var updated = await _categoryService.SetActiveStatusAsync(
                id,
                isActive,
                userId.Value);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Procurement category not found."
                });
            }

            return NoContent();
        }
      catch (InvalidOperationException)
{
    return Forbid();
}
    }

    private int? GetAuthenticatedUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return int.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }
}