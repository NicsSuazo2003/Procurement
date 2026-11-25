namespace Drmc.Procurement.Domain.Entities.Organization;

public class Team
{
    public int TeamId { get; set; }
    public int DepartmentId { get; set; }
    public string TeamCode { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Department Department { get; set; } = null!;
    public ICollection<TeamMembership> Memberships { get; set; } = new List<TeamMembership>();
}