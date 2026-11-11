namespace Drmc.Procurement.Domain.Entities.Identity;

using Drmc.Procurement.Domain.Entities.Organization;

public class User
{
    public int UserId { get; set; }

    public int? DepartmentId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Hashed password. Never store plain-text passwords.
    public string? PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Department? Department { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }
        = new List<UserRole>();

        public ICollection<UserCategoryPreference> CategoryPreferences { get; set; }
    = new List<UserCategoryPreference>();
}