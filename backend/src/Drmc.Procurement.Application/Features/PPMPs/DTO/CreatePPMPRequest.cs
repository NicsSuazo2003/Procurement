using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class CreatePPMPRequest
{
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int PlanningYearId { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreatePPMPItemRequest> Items { get; set; } = new();
}