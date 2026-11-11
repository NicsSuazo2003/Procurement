using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class ApprovePPMPRequest
{
    [StringLength(1000)]
    public string? Remarks { get; set; }
}