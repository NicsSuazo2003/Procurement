using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.APP.DTO;
using Drmc.Procurement.Application.Features.APP.Interfaces;

namespace Drmc.Procurement.Application.Features.APP.Commands;

public sealed record GenerateAPPCommand(int PlanningYearId, int CreatedBy) : ICommand<GenerateAPPResultDto>;
public sealed record AddReviewedPPMPsCommand(int AppId, int AddedBy) : ICommand<GenerateAPPResultDto>;
public sealed record PrepareAPPCommand(int AppId, int PreparedBy) : ICommand<APPDetailDto>;
public sealed record RecommendAPPCommand(int AppId, int RecommendedBy) : ICommand<APPDetailDto>;
public sealed record ApproveAPPCommand(int AppId, int ApprovedBy) : ICommand<APPDetailDto>;

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

public sealed class PrepareAPPHandler(IAPPService service) : IRequestHandler<PrepareAPPCommand, APPDetailDto>
{
    public Task<APPDetailDto> Handle(PrepareAPPCommand request, CancellationToken cancellationToken = default) =>
        service.MarkPreparedAsync(request.AppId, request.PreparedBy);
}

public sealed class RecommendAPPHandler(IAPPService service) : IRequestHandler<RecommendAPPCommand, APPDetailDto>
{
    public Task<APPDetailDto> Handle(RecommendAPPCommand request, CancellationToken cancellationToken = default) =>
        service.MarkRecommendedAsync(request.AppId, request.RecommendedBy);
}

public sealed class ApproveAPPHandler(IAPPService service) : IRequestHandler<ApproveAPPCommand, APPDetailDto>
{
    public Task<APPDetailDto> Handle(ApproveAPPCommand request, CancellationToken cancellationToken = default) =>
        service.MarkApprovedAsync(request.AppId, request.ApprovedBy);
}
