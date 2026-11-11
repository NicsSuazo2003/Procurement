using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.Roles.DTO;

public class CreateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Description { get; set; } = string.Empty;
}