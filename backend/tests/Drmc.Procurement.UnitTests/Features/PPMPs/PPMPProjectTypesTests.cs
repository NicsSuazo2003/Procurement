using Drmc.Procurement.Application.Features.PPMPs;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public sealed class PPMPProjectTypesTests
{
    [Theory]
    [InlineData(" Goods ", "Goods")]
    [InlineData("infrastructure", "Infrastructure")]
    [InlineData("CONSULTING SERVICES", "Consulting Services")]
    public void Normalize_AllowedValue_ReturnsCanonicalValue(string value, string expected)
    {
        Assert.Equal(expected, PPMPProjectTypes.Normalize(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Services")]
    public void Normalize_DisallowedValue_Throws(string? value)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => PPMPProjectTypes.Normalize(value));

        Assert.Equal(
            "Project type must be Goods, Infrastructure, or Consulting Services.",
            exception.Message);
    }
}
