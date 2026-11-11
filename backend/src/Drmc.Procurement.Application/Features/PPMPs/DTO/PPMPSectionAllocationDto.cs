namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class PPMPSectionAllocationDto
{
    public int AllocationId { get; set; }

    public int SectionId { get; set; }

    public string SectionCode { get; set; } = string.Empty;

    public string SectionName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
}