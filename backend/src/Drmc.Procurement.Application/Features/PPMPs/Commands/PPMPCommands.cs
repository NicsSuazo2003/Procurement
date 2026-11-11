using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;

namespace Drmc.Procurement.Application.Features.PPMPs.Commands;

public sealed record CreatePPMPCommand(CreatePPMPRequest Request, int CreatedBy, int DepartmentId) : ICommand<PPMPDetailDto>;
public sealed record SubmitPPMPCommand(int Id, int UserId, int DepartmentId) : ICommand<PPMPDetailDto>;
public sealed record ApprovePPMPCommand(int Id, ApprovePPMPRequest Request, int ApprovedBy, int DepartmentId) : ICommand<PPMPDetailDto>;
public sealed record StartPPMPReviewCommand(int Id) : ICommand<PPMPDetailDto>;
public sealed record CompletePPMPReviewCommand(int Id, int ReviewedBy) : ICommand<PPMPDetailDto>;
public sealed record UpdatePPMPCommand(int Id, UpdatePPMPRequest Request, int ChangedBy) : ICommand<PPMPDetailDto>;

public sealed class CreatePPMPHandler(IPPMPService service) : IRequestHandler<CreatePPMPCommand, PPMPDetailDto>
{
    public Task<PPMPDetailDto> Handle(CreatePPMPCommand command, CancellationToken cancellationToken = default) =>
        service.CreateAsync(command.Request, command.CreatedBy, command.DepartmentId);
}

public sealed class SubmitPPMPHandler(IPPMPService service) : IRequestHandler<SubmitPPMPCommand, PPMPDetailDto>
{
    public Task<PPMPDetailDto> Handle(SubmitPPMPCommand command, CancellationToken cancellationToken = default) =>
        service.SubmitAsync(command.Id, command.UserId, command.DepartmentId);
}

public sealed class ApprovePPMPHandler(IPPMPService service) : IRequestHandler<ApprovePPMPCommand, PPMPDetailDto>
{
    public Task<PPMPDetailDto> Handle(ApprovePPMPCommand command, CancellationToken cancellationToken = default) =>
        service.ApproveAsync(command.Id, command.Request, command.ApprovedBy, command.DepartmentId);
}

public sealed class StartPPMPReviewHandler(IPPMPService service) : IRequestHandler<StartPPMPReviewCommand, PPMPDetailDto>
{
    public Task<PPMPDetailDto> Handle(StartPPMPReviewCommand command, CancellationToken cancellationToken = default) =>
        service.StartReviewAsync(command.Id);
}

public sealed class CompletePPMPReviewHandler(IPPMPService service) : IRequestHandler<CompletePPMPReviewCommand, PPMPDetailDto>
{
    public Task<PPMPDetailDto> Handle(CompletePPMPReviewCommand command, CancellationToken cancellationToken = default) =>
        service.CompleteReviewAsync(command.Id, command.ReviewedBy);
}

public sealed class UpdatePPMPHandler(IPPMPService service) : IRequestHandler<UpdatePPMPCommand, PPMPDetailDto>
{
    public Task<PPMPDetailDto> Handle(UpdatePPMPCommand command, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(command.Id, command.Request, command.ChangedBy);
}
