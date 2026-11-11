namespace Drmc.Procurement.Application.Features.Departments.DTO;

public class DepartmentDto
{
    public int DepartmentId { get; set; }

    public string DepartmentCode { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}