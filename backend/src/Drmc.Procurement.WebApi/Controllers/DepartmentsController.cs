using Drmc.Procurement.Application.Features.Departments.DTO;
using Drmc.Procurement.Application.Features.Departments.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // GET: api/departments
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();

        return Ok(departments);
    }

    // GET: api/departments/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);

        if (department is null)
        {
            return NotFound(new
            {
                message = "Department not found."
            });
        }

        return Ok(department);
    }

    // POST: api/departments
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(
        [FromBody] CreateDepartmentRequest request)
    {
        try
        {
            var department = await _departmentService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = department.DepartmentId },
                department);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT: api/departments/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Update(
        int id,
        [FromBody] CreateDepartmentRequest request)
    {
        try
        {
            var department = await _departmentService.UpdateAsync(id, request);

            if (department is null)
            {
                return NotFound(new
                {
                    message = "Department not found."
                });
            }

            return Ok(department);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PATCH: api/departments/1/active-status
    [HttpPatch("{id:int}/active-status")]
    public async Task<IActionResult> SetActiveStatus(
        int id,
        [FromBody] bool isActive)
    {
        var updated = await _departmentService.SetActiveStatusAsync(
            id,
            isActive);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Department not found."
            });
        }

        return NoContent();
    }
} 