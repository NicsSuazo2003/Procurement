namespace Drmc.Procurement.Application.Features.PlanningYears.DTO;

public class PlanningYearDto
{
    public int PlanningYearId { get; set; }

    public int Year { get; set; }

    public string Status { get; set; } = string.Empty;
}