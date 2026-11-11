using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class CreatePPMPSectionAllocationRequest
{
    [Range(1, int.MaxValue)]
    public int SectionId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Quantity { get; set; }
}