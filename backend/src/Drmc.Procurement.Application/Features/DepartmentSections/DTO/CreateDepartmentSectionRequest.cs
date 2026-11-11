using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.DepartmentSections.DTO;

public class CreateDepartmentSectionRequest
{
    [Required]
    public int DepartmentId { get; set; }

    [Required]
    [StringLength(30)]
    public string SectionCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string SectionName { get; set; } = string.Empty;
}