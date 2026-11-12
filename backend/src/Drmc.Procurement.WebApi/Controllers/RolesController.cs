using System.Security.Claims;
using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Roles.Commands;
using Drmc.Procurement.Application.Features.Roles.DTO;
using Drmc.Procurement.Application.Features.Roles.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await dispatcher.Send(new GetAllRolesQuery(), HttpContext.RequestAborted));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await dispatcher.Send(new GetRoleByIdQuery(id), HttpContext.RequestAborted);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new CreateRoleCommand(request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = result.RoleId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRoleRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new UpdateRoleCommand(id, request), HttpContext.RequestAborted);
            if (result is null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await dispatcher.Send(new DeleteRoleCommand(id), HttpContext.RequestAborted);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
