using System.Security.Claims;
using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PPMPsController(IDispatcher dispatcher) : ControllerBase
{
    private int? GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private int? GetDepartmentId() => int.TryParse(User.FindFirstValue("department_id"), out var id) ? id : null;
    private bool CanViewAllDepartments() => User.IsInRole("Procurement Staff") || User.IsInRole("System Administrator");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string[]? status = null, [FromQuery] int? planningYear = null)
    {
        var departmentId = CanViewAllDepartments() ? null : GetDepartmentId();
        if (!CanViewAllDepartments() && !departmentId.HasValue) return Forbid();
        return Ok(await dispatcher.Send(new GetPagedPPMPsQuery(departmentId, page, pageSize, status, planningYear), HttpContext.RequestAborted));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var departmentId = CanViewAllDepartments() ? null : GetDepartmentId();
        if (!CanViewAllDepartments() && !departmentId.HasValue) return Forbid();
        return Ok(await dispatcher.Send(new GetPPPMSummaryQuery(departmentId), HttpContext.RequestAborted));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var departmentId = CanViewAllDepartments() ? null : GetDepartmentId();
        if (!CanViewAllDepartments() && !departmentId.HasValue) return Forbid();
        var ppmp = await dispatcher.Send(new GetPPMPByIdQuery(id, departmentId), HttpContext.RequestAborted);
        return ppmp == null ? NotFound(new { message = $"PPMP with ID {id} was not found." }) : Ok(ppmp);
    }

    [HttpGet("{id:int}/change-history")]
    public async Task<IActionResult> GetChangeHistory(int id)
    {
        var departmentId = CanViewAllDepartments() ? null : GetDepartmentId();
        if (!CanViewAllDepartments() && !departmentId.HasValue) return Forbid();
        var ppmp = await dispatcher.Send(new GetPPMPByIdQuery(id, departmentId), HttpContext.RequestAborted);
        if (ppmp == null) return NotFound(new { message = $"PPMP with ID {id} was not found." });
        return Ok(await dispatcher.Send(new GetPPMPChangeHistoryQuery(id, departmentId), HttpContext.RequestAborted));
    }

    [HttpPost]
    [Authorize(Roles = "End-user")]
    public async Task<IActionResult> Create([FromBody] CreatePPMPRequest request)
    {
        var userId = GetUserId();
        var departmentId = GetDepartmentId();
        if (!userId.HasValue || !departmentId.HasValue) return Forbid();
        try
        {
            var created = await dispatcher.Send(new CreatePPMPCommand(request, userId.Value, departmentId.Value), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = created.PPMPId }, created);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPatch("{id:int}/submit")]
    [Authorize(Roles = "End-user")]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = GetUserId();
        var departmentId = GetDepartmentId();
        if (!userId.HasValue || !departmentId.HasValue) return Forbid();
        try { return Ok(await dispatcher.Send(new SubmitPPMPCommand(id, userId.Value, departmentId.Value), HttpContext.RequestAborted)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = "Unit Head")]
    [Consumes("application/json")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApprovePPMPRequest? request)
    {
        var userId = GetUserId();
        var departmentId = GetDepartmentId();
        if (!userId.HasValue || !departmentId.HasValue) return Forbid();
        try { return Ok(await dispatcher.Send(new ApprovePPMPCommand(id, request ?? new ApprovePPMPRequest(), userId.Value, departmentId.Value), HttpContext.RequestAborted)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPatch("{id:int}/start-review")]
    [Authorize(Roles = "Procurement Staff")]
    public async Task<IActionResult> StartReview(int id)
    {
        try { return Ok(await dispatcher.Send(new StartPPMPReviewCommand(id), HttpContext.RequestAborted)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPatch("{id:int}/complete-review")]
    [Authorize(Roles = "Procurement Staff")]
    public async Task<IActionResult> CompleteReview(int id)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Forbid();
        try { return Ok(await dispatcher.Send(new CompletePPMPReviewCommand(id, userId.Value), HttpContext.RequestAborted)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Procurement Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePPMPRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Forbid();
        try { return Ok(await dispatcher.Send(new UpdatePPMPCommand(id, request, userId.Value), HttpContext.RequestAborted)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
