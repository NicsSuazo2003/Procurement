namespace Drmc.Procurement.Application.Features.Users.DTO;

public class UserDto
{
    public int UserId { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> RoleNames { get; set; } = new();
}