namespace Drmc.Procurement.Domain.Entities.Organization;

public class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentCode { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<DepartmentSection> Sections { get; set; }
        = new List<DepartmentSection>();
}