using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.PlanningYears.DTO;

public class CreatePlanningYearRequest
{
    [Range(2000, 2100)]
    public int Year { get; set; }
}