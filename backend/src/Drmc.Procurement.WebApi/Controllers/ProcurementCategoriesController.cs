using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.ProcurementCategories.Commands;
using Drmc.Procurement.Application.Features.ProcurementCategories.DTO;
using Drmc.Procurement.Application.Features.ProcurementCategories.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcurementCategoriesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProcurementCategoryDto>>> GetAll()
    {
        return Ok(await dispatcher.Send(new GetAllProcurementCategoriesQuery(), HttpContext.RequestAborted));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcurementCategoryDto>> GetById(int id)
    {
        var category = await dispatcher.Send(new GetProcurementCategoryByIdQuery(id), HttpContext.RequestAborted);
        if (category is null) return NotFound(new { message = "Procurement category not found." });
        return Ok(category);
    }

    [HttpGet("{id:int}/history")]
    [Authorize(Roles = "Procurement Staff,System Administrator")]
    public async Task<ActionResult<List<ProcurementCategoryAuditLogDto>>> GetHistory(int id)
    {
        try
        {
            var history = await dispatcher.Send(new GetProcurementCategoryHistoryQuery(id), HttpContext.RequestAborted);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Procurement Staff,System Administrator")]
    public async Task<ActionResult<ProcurementCategoryDto>> Create([FromBody] CreateProcurementCategoryRequest request)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null) return Forbid();
        try
        {
            var category = await dispatcher.Send(new CreateProcurementCategoryCommand(request, userId.Value), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Procurement Staff,System Administrator")]
    public async Task<ActionResult<ProcurementCategoryDto>> Update(int id, [FromBody] CreateProcurementCategoryRequest request)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null) return Forbid();
        try
        {
            var category = await dispatcher.Send(new UpdateProcurementCategoryCommand(id, request, userId.Value), HttpContext.RequestAborted);
            if (category is null) return NotFound(new { message = "Procurement category not found." });
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/active-status")]
    [Authorize(Roles = "Procurement Staff,System Administrator")]
    public async Task<IActionResult> SetActiveStatus(int id, [FromBody] bool isActive)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null) return Forbid();
        try
        {
            var updated = await dispatcher.Send(new SetProcurementCategoryActiveStatusCommand(id, isActive, userId.Value), HttpContext.RequestAborted);
            if (!updated) return NotFound(new { message = "Procurement category not found." });
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
    }

    private int? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
