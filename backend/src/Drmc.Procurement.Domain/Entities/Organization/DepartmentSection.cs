namespace Drmc.Procurement.Domain.Entities.Organization;

public class DepartmentSection
{
    public int SectionId { get; set; }

    public int DepartmentId { get; set; }

    public string SectionCode { get; set; } = string.Empty;

    public string SectionName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation property
    public Department Department { get; set; } = null!;
}