using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.DepartmentSections.Commands;
using Drmc.Procurement.Application.Features.DepartmentSections.DTO;
using Drmc.Procurement.Application.Features.DepartmentSections.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentSectionsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DepartmentSectionDto>>> GetAll()
    {
        var sections = await dispatcher.Send(new GetAllDepartmentSectionsQuery(), HttpContext.RequestAborted);
        return Ok(sections);
    }

    [HttpGet("department/{departmentId:int}")]
    public async Task<ActionResult<List<DepartmentSectionDto>>> GetByDepartmentId(int departmentId)
    {
        var sections = await dispatcher.Send(new GetDepartmentSectionsByDepartmentQuery(departmentId), HttpContext.RequestAborted);
        return Ok(sections);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentSectionDto>> GetById(int id)
    {
        var section = await dispatcher.Send(new GetDepartmentSectionByIdQuery(id), HttpContext.RequestAborted);
        if (section is null) return NotFound(new { message = "Department section not found." });
        return Ok(section);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentSectionDto>> Create([FromBody] CreateDepartmentSectionRequest request)
    {
        try
        {
            var section = await dispatcher.Send(new CreateDepartmentSectionCommand(request), HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = section.SectionId }, section);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentSectionDto>> Update(int id, [FromBody] CreateDepartmentSectionRequest request)
    {
        try
        {
            var section = await dispatcher.Send(new UpdateDepartmentSectionCommand(id, request), HttpContext.RequestAborted);
            if (section is null) return NotFound(new { message = "Department section not found." });
            return Ok(section);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/active-status")]
    public async Task<IActionResult> SetActiveStatus(int id, [FromBody] bool isActive)
    {
        var updated = await dispatcher.Send(new SetDepartmentSectionActiveStatusCommand(id, isActive), HttpContext.RequestAborted);
        if (!updated) return NotFound(new { message = "Department section not found." });
        return NoContent();
    }
}
