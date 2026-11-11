using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.Departments.DTO;

public class CreateDepartmentRequest
{
    [Required]
    [StringLength(20)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string DepartmentName { get; set; } = string.Empty;
}
