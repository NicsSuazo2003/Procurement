namespace Drmc.Procurement.Application.Features.DepartmentSections.DTO;

public class DepartmentSectionDto
{
    public int SectionId { get; set; }

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string SectionCode { get; set; } = string.Empty;

    public string SectionName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}