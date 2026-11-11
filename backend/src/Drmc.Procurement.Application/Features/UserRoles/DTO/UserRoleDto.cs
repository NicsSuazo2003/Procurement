namespace Drmc.Procurement.Application.Features.UserRoles.DTO;

public class UserRoleDto
{
    public int UserRoleId { get; set; }
    public int UserId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
}