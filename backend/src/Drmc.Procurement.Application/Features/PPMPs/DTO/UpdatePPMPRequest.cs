using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class UpdatePPMPRequest
{
    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<UpdatePPMPItemRequest> Items { get; set; } = new();
}