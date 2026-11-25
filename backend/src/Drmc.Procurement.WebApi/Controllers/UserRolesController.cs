using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.UserRoles.Commands;
using Drmc.Procurement.Application.Features.UserRoles.DTO;
using Drmc.Procurement.Application.Features.UserRoles.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/Users/{userId:int}/roles")]
public class UserRolesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUserRoles(int userId)
    {
        try
        {
            return Ok(await dispatcher.Send(new GetUserRolesQuery(userId), HttpContext.RequestAborted));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignUserRoleRequest request)
    {
        try
        {
            var result = await dispatcher.Send(new AssignUserRoleCommand(userId, request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetUserRoles), new { userId }, result);
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

    [HttpDelete("{roleId:int}")]
    public async Task<IActionResult> RemoveRole(int userId, int roleId)
    {
        var removed = await dispatcher.Send(new RemoveUserRoleCommand(userId, roleId), HttpContext.RequestAborted);
        if (!removed) return NotFound(new { message = "User role assignment not found." });
        return NoContent();
    }
}
