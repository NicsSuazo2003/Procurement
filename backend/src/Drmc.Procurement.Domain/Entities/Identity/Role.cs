namespace Drmc.Procurement.Domain.Entities.Identity;

public class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; }
        = new List<UserRole>();
}