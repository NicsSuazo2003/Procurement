using System.ComponentModel.DataAnnotations;
using Drmc.Procurement.Application.Features.PPMPs.DTO;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public class CreatePPMPItemRequestTests
{
    [Fact]
    public void AllowsItemDescriptionsUpToTheOfficialFormLimit()
    {
        var request = new CreatePPMPItemRequest
        {
            CategoryId = 1,
            ItemDescription = new string('x', 1500),
            Unit = "unit",
            GeneralDescriptionAndObjective = new string('x', 1500),
            ProjectType = "Goods",
            Quantity = 1
        };
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            errors,
            validateAllProperties: true);

        Assert.True(isValid, string.Join("; ", errors.Select(error => error.ErrorMessage)));
    }
}
