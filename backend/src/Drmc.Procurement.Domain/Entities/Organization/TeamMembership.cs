using Drmc.Procurement.Domain.Entities.Identity;

namespace Drmc.Procurement.Domain.Entities.Organization;

public class TeamMembership
{
    public int TeamMembershipId { get; set; }
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public bool IsActive { get; set; } = true;

    public Team Team { get; set; } = null!;
    public User User { get; set; } = null!;
}