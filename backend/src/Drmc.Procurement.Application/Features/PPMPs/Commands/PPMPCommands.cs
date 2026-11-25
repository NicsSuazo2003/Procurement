using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;

namespace Drmc.Procurement.Application.Features.PPMPs.Commands;

public sealed record CreatePPMPCommand(CreatePPMPRequest Request, int CreatedBy, int DepartmentId) : ICommand<PPMPDetailDto>;
public sealed record SubmitPPMPCommand(int Id, int UserId, int DepartmentId) : ICommand<PPMPDetailDto>;
public sealed record ApprovePPMPCommand(int Id, ApprovePPMPRequest Request, int ApprovedBy, int DepartmentId) : ICommand<PPMPDetailDto>;
public sealed record StartPPMPReviewCommand(int Id) : ICommand<PPMPDetailDto>;
public sealed record CompletePPMPReviewCommand(int Id, int ReviewedBy) : ICommand<PPMPDetailDto>;
public sealed record UpdatePPMPCommand(int Id, UpdatePPMPRequest Request, int ChangedBy) : ICommand<PPMPDetailDto>;
