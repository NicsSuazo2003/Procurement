namespace Drmc.Procurement.Domain.Entities.Planning;

public class PlanningYear
{
    public int PlanningYearId { get; set; }

    public int Year { get; set; }

    public string Status { get; set; } = "Open";

    public ICollection<PlanningYearCategory> Categories { get; set; }
        = new List<PlanningYearCategory>();
}