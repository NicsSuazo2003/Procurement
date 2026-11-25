using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Users.Commands;
using Drmc.Procurement.Application.Features.Users.DTO;
using Drmc.Procurement.Application.Features.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await dispatcher.Send(new GetAllUsersQuery(), HttpContext.RequestAborted));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await dispatcher.Send(new GetUserByIdQuery(id), HttpContext.RequestAborted);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new CreateUserCommand(request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = result.UserId }, result);
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
    public async Task<IActionResult> Update(int id, [FromBody] CreateUserRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new UpdateUserCommand(id, request), HttpContext.RequestAborted);
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
        var updated = await dispatcher.Send(new SetUserActiveStatusCommand(id, isActive), HttpContext.RequestAborted);
        if (!updated) return NotFound();
        return NoContent();
    }
}
