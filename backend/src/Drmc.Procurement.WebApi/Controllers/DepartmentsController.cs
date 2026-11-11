using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.APP.Commands;
using Drmc.Procurement.Application.Features.APP.Queries;
using Drmc.Procurement.Application.Features.DepartmentSections.Commands;
using Drmc.Procurement.Application.Features.DepartmentSections.Queries;
using Drmc.Procurement.Application.Features.Departments.Commands;
using Drmc.Procurement.Application.Features.Departments.Queries;
using Drmc.Procurement.Application.Features.Items.Commands;
using Drmc.Procurement.Application.Features.Items.Queries;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Commands;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Queries;
using Drmc.Procurement.Application.Features.PlanningYearItems.Commands;
using Drmc.Procurement.Application.Features.PlanningYearItems.Queries;
using Drmc.Procurement.Application.Features.PlanningYears.Commands;
using Drmc.Procurement.Application.Features.PlanningYears.Queries;
using Drmc.Procurement.Application.Features.ProcurementCategories.Commands;
using Drmc.Procurement.Application.Features.ProcurementCategories.Queries;
using Drmc.Procurement.Application.Features.Roles.Commands;
using Drmc.Procurement.Application.Features.Roles.Queries;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Commands;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Queries;
using Drmc.Procurement.Application.Features.UserRoles.Commands;
using Drmc.Procurement.Application.Features.UserRoles.Queries;
using Drmc.Procurement.Application.Features.Users.Commands;
using Drmc.Procurement.Application.Features.Users.Queries;
using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.Queries;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
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
