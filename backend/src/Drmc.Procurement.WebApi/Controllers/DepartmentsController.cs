using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Departments.Commands;
using Drmc.Procurement.Application.Features.Departments.DTO;
using Drmc.Procurement.Application.Features.Departments.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll()
    {
        var departments = await dispatcher.Send(new GetAllDepartmentsQuery(), HttpContext.RequestAborted);
        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var department = await dispatcher.Send(new GetDepartmentByIdQuery(id), HttpContext.RequestAborted);
        if (department is null) return NotFound(new { message = "Department not found." });
        return Ok(department);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentRequest request)
    {
        try
        {
            var department = await dispatcher.Send(new CreateDepartmentCommand(request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = department.DepartmentId }, department);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Update(int id, [FromBody] CreateDepartmentRequest request)
    {
        try
        {
            var department = await dispatcher.Send(new UpdateDepartmentCommand(id, request), HttpContext.RequestAborted);
            if (department is null) return NotFound(new { message = "Department not found." });
            return Ok(department);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/active-status")]
    public async Task<IActionResult> SetActiveStatus(int id, [FromBody] bool isActive)
    {
        var updated = await dispatcher.Send(new SetDepartmentActiveStatusCommand(id, isActive), HttpContext.RequestAborted);
        if (!updated) return NotFound(new { message = "Department not found." });
        return NoContent();
    }
}
