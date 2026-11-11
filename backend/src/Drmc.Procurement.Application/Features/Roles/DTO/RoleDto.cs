namespace Drmc.Procurement.Application.Features.Roles.DTO;

public class RoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AssignedUserCount { get; set; }
}