using Drmc.Procurement.Domain.Entities.Organization;

namespace Drmc.Procurement.Domain.Entities.Procurement;

public class PPMPSectionAllocation
{
    public int AllocationId { get; set; }

    public int PPMPItemId { get; set; }

    public int SectionId { get; set; }

    public decimal Quantity { get; set; }

    // Navigation properties
    public PPMPItem PPMPItem { get; set; } = null!;

    public DepartmentSection Section { get; set; } = null!;
}