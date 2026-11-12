using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.APP.Commands;
using Drmc.Procurement.Application.Features.APP.DTO;
using Drmc.Procurement.Application.Features.APP.Queries;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Procurement Staff,System Administrator")]
public class APPsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await dispatcher.Send(new GetAllAPPsQuery(), HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await dispatcher.Send(new GetAPPByIdQuery(id), HttpContext.RequestAborted);
        if (result == null) return NotFound(new { message = "APP not found." });
        return Ok(result);
    }

    [HttpPost("generate/{planningYearId:int}")]
    [Authorize(Roles = "Procurement Staff")]
    public async Task<IActionResult> Generate(int planningYearId)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Forbid();
        try
        {
            var result = await dispatcher.Send(new GenerateAPPCommand(planningYearId, userId.Value), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = result.APPId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/add-reviewed-ppmp")]
    [Authorize(Roles = "Procurement Staff")]
    public async Task<IActionResult> AddReviewedPPMPs(int id)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Forbid();
        try
        {
            var result = await dispatcher.Send(new AddReviewedPPMPsCommand(id, userId.Value), HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : null;
    }
}
