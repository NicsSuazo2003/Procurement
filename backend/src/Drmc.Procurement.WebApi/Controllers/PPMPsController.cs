using System.Security.Claims;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PPMPsController : ControllerBase
{
    private readonly IPPMPService _ppmpService;

    public PPMPsController(IPPMPService ppmpService)
    {
        _ppmpService = ppmpService;
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var userId)
            ? userId
            : null;
    }

    private int? GetDepartmentId()
    {
        var value = User.FindFirstValue("department_id");

        return int.TryParse(value, out var departmentId)
            ? departmentId
            : null;
    }

    private bool CanViewAllDepartments()
    {
        return User.IsInRole("Procurement Staff") ||
               User.IsInRole("System Administrator");
    }

    // GET: api/PPMPs?page=1&pageSize=25&status=Approved%20by%20Unit%20Head
    // status may be repeated (?status=A&status=B) or comma-separated.
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string[]? status = null,
        [FromQuery] int? planningYear = null)
    {
        var departmentId = CanViewAllDepartments()
            ? null
            : GetDepartmentId();

        if (!CanViewAllDepartments() && !departmentId.HasValue)
        {
            return Forbid();
        }

        var result = await _ppmpService.GetPagedAsync(
            departmentId,
            page,
            pageSize,
            status,
            planningYear);

        return Ok(result);
    }

    // GET: api/PPMPs/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var departmentId = CanViewAllDepartments()
            ? null
            : GetDepartmentId();

        if (!CanViewAllDepartments() && !departmentId.HasValue)
        {
            return Forbid();
        }

        var summary = await _ppmpService.GetSummaryAsync(departmentId);

        return Ok(summary);
    }

    // GET: api/PPMPs/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var departmentId = CanViewAllDepartments()
            ? null
            : GetDepartmentId();

        if (!CanViewAllDepartments() && !departmentId.HasValue)
        {
            return Forbid();
        }

        var ppmp = await _ppmpService.GetByIdAsync(
            id,
            departmentId);

        if (ppmp == null)
        {
            return NotFound(new
            {
                message = $"PPMP with ID {id} was not found."
            });
        }

        return Ok(ppmp);
    }

    // GET: api/PPMPs/1/change-history
    [HttpGet("{id:int}/change-history")]
    public async Task<IActionResult> GetChangeHistory(int id)
    {
        var departmentId = CanViewAllDepartments()
            ? null
            : GetDepartmentId();

        if (!CanViewAllDepartments() && !departmentId.HasValue)
        {
            return Forbid();
        }

        // Verify that the PPMP exists and the user can access it.
        var ppmp = await _ppmpService.GetByIdAsync(
            id,
            departmentId);

        if (ppmp == null)
        {
            return NotFound(new
            {
                message = $"PPMP with ID {id} was not found."
            });
        }

        var history = await _ppmpService.GetChangeHistoryAsync(
            id,
            departmentId);

        return Ok(history);
    }

    // POST: api/PPMPs
    [HttpPost]
    [Authorize(Roles = "End-user")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePPMPRequest request)
    {
        var userId = GetUserId();
        var departmentId = GetDepartmentId();

        if (!userId.HasValue || !departmentId.HasValue)
        {
            return Forbid();
        }

        try
        {
            var createdPPMP = await _ppmpService.CreateAsync(
                request,
                userId.Value,
                departmentId.Value);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdPPMP.PPMPId },
                createdPPMP);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PATCH: api/PPMPs/1/submit
    [HttpPatch("{id:int}/submit")]
    [Authorize(Roles = "End-user")]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = GetUserId();
        var departmentId = GetDepartmentId();

        if (!userId.HasValue || !departmentId.HasValue)
        {
            return Forbid();
        }

        try
        {
            var result = await _ppmpService.SubmitAsync(
                id,
                userId.Value,
                departmentId.Value);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PATCH: api/PPMPs/1/approve
    // Fixed: explicit [Consumes] + nullable [FromBody] so a body-less
    // PATCH returns a clean validation error (or succeeds) instead of
    // a 415 Unsupported Media Type.
    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = "Unit Head")]
    [Consumes("application/json")]
    public async Task<IActionResult> Approve(
        int id,
        [FromBody] ApprovePPMPRequest? request)
    {
        var userId = GetUserId();
        var departmentId = GetDepartmentId();

        if (!userId.HasValue || !departmentId.HasValue)
        {
            return Forbid();
        }

        try
        {
            var result = await _ppmpService.ApproveAsync(
                id,
                request ?? new ApprovePPMPRequest(),
                userId.Value,
                departmentId.Value);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PATCH: api/PPMPs/1/start-review
    [HttpPatch("{id:int}/start-review")]
    [Authorize(Roles = "Procurement Staff")]
    public async Task<IActionResult> StartReview(int id)
    {
        try
        {
            var result = await _ppmpService.StartReviewAsync(id);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PATCH: api/PPMPs/1/complete-review
    [HttpPatch("{id:int}/complete-review")]
    [Authorize(Roles = "Procurement Staff")]
    public async Task<IActionResult> CompleteReview(int id)
    {
        var userId = GetUserId();

        if (!userId.HasValue)
        {
            return Forbid();
        }

        try
        {
            var result = await _ppmpService.CompleteReviewAsync(
                id,
                userId.Value);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/PPMPs/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Procurement Staff")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePPMPRequest request)
    {
        var userId = GetUserId();

        if (!userId.HasValue)
        {
            return Forbid();
        }

        try
        {
            var result = await _ppmpService.UpdateAsync(
                id,
                request,
                userId.Value);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}