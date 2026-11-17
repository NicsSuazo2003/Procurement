namespace Drmc.Procurement.Application.Features.PPMPs;

public static class PPMPProjectTypes
{
    public static string Normalize(string? value)
    {
        var projectType = value?.Trim();

        if (string.Equals(projectType, "Goods", StringComparison.OrdinalIgnoreCase))
            return "Goods";

        if (string.Equals(projectType, "Infrastructure", StringComparison.OrdinalIgnoreCase))
            return "Infrastructure";

        if (string.Equals(projectType, "Consulting Services", StringComparison.OrdinalIgnoreCase))
            return "Consulting Services";

        throw new InvalidOperationException(
            "Project type must be Goods, Infrastructure, or Consulting Services.");
    }
}
