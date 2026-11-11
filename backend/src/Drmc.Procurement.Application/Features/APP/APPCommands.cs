using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.APP.DTO;
using Drmc.Procurement.Application.Features.APP.Interfaces;

namespace Drmc.Procurement.Application.Features.APP.Commands;

public sealed record GenerateAPPCommand(int PlanningYearId, int CreatedBy) : ICommand<GenerateAPPResultDto>;
public sealed record AddReviewedPPMPsCommand(int AppId, int AddedBy) : ICommand<GenerateAPPResultDto>;

public sealed class GenerateAPPHandler(IAPPService service) : IRequestHandler<GenerateAPPCommand, GenerateAPPResultDto>
{
    public Task<GenerateAPPResultDto> Handle(GenerateAPPCommand request, CancellationToken cancellationToken = default) =>
        service.GenerateAsync(request.PlanningYearId, request.CreatedBy);
}

public sealed class AddReviewedPPMPsHandler(IAPPService service) : IRequestHandler<AddReviewedPPMPsCommand, GenerateAPPResultDto>
{
    public Task<GenerateAPPResultDto> Handle(AddReviewedPPMPsCommand request, CancellationToken cancellationToken = default) =>
        service.AddReviewedPPMPsAsync(request.AppId, request.AddedBy);
}
