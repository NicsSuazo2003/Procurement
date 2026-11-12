using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public sealed class SubmitPPMPHandlerTests
{
    [Fact]
    public async Task Handle_OwnDraftWithItems_UpdatesAndReturnsDetails()
    {
        var ppmp = new PPMP
        {
            PPMPId = 5,
            CreatedBy = 12,
            DepartmentId = 7,
            Status = "Draft",
            Items = [new PPMPItem()]
        };
        var repository = new FakePPMPRepository { PPMP = ppmp };
        var reader = new FakeReadRepository
        {
            Details = new PPMPDetailDto
            {
                PPMPId = 5,
                DepartmentId = 7,
                Status = "For Unit Head Approval"
            }
        };
        var handler = new SubmitPPMPHandler(repository, reader);
        using var cancellationSource = new CancellationTokenSource();

        var result = await handler.Handle(
            new SubmitPPMPCommand(Id: 5, UserId: 12, DepartmentId: 7),
            cancellationSource.Token);

        Assert.Equal("For Unit Head Approval", ppmp.Status);
        Assert.NotNull(ppmp.SubmittedAt);
        Assert.NotNull(ppmp.UpdatedAt);
        Assert.Equal(5, result.PPMPId);
        Assert.Equal(5, repository.RequestedPPMPId);
        Assert.Equal(12, repository.RequestedUserId);
        Assert.Equal(7, repository.RequestedDepartmentId);
        Assert.Equal(cancellationSource.Token, repository.CancellationToken);
        Assert.Equal(cancellationSource.Token, reader.CancellationToken);
        Assert.Equal(5, reader.RequestedId);
        Assert.Equal(7, reader.RequestedDepartmentId);
    }

    [Fact]
    public async Task Handle_PpmpNotOwnedByUserOrDepartment_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository();
        var handler = new SubmitPPMPHandler(repository, new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new SubmitPPMPCommand(Id: 5, UserId: 12, DepartmentId: 7)));

        Assert.Equal("PPMP with ID 5 was not found.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_NonDraftPpmp_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP { PPMPId = 5, Status = "Under Review", Items = [new PPMPItem()] }
        };
        var handler = new SubmitPPMPHandler(repository, new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new SubmitPPMPCommand(Id: 5, UserId: 12, DepartmentId: 7)));

        Assert.Equal("Only Draft PPMPs can be submitted.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_DraftWithoutItems_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP { PPMPId = 5, Status = "Draft" }
        };
        var handler = new SubmitPPMPHandler(repository, new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new SubmitPPMPCommand(Id: 5, UserId: 12, DepartmentId: 7)));

        Assert.Equal(
            "A PPMP must contain at least one item before submission.",
            exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    private sealed class FakePPMPRepository : IPPMPRepository
    {
        public PPMP? PPMP { get; init; }
        public int RequestedPPMPId { get; private set; }
        public int RequestedUserId { get; private set; }
        public int RequestedDepartmentId { get; private set; }
        public int SaveCount { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        public PPMPApproval? AddedApproval { get; private set; }

        public Task<PPMP?> GetForSubmissionAsync(
            int ppmpId,
            int userId,
            int departmentId,
            CancellationToken cancellationToken = default)
        {
            RequestedPPMPId = ppmpId;
            RequestedUserId = userId;
            RequestedDepartmentId = departmentId;
            CancellationToken = cancellationToken;
            return Task.FromResult(PPMP);
        }

        public Task<PPMP?> GetForApprovalAsync(
            int ppmpId,
            int departmentId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PPMP?>(null);

        public Task<PPMP?> GetForReviewAsync(
            int ppmpId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PPMP?>(null);

        public Task<PPMP?> GetForUpdateAsync(
            int ppmpId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PPMP?>(null);

        public void AddApproval(PPMPApproval approval) => AddedApproval = approval;

        public Task SaveUpdateAsync(
            PPMP ppmp,
            IReadOnlyCollection<PPMPSectionAllocation> removedAllocations,
            IReadOnlyCollection<PPMPItem> removedItems,
            Drmc.Procurement.Domain.Entities.Audit.PPMPChangeHistory history,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveNewAsync(PPMP ppmp, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeReadRepository : IPPMPReadRepository
    {
        public PPMPDetailDto? Details { get; init; }
        public int RequestedId { get; private set; }
        public int? RequestedDepartmentId { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<PagedResult<PPMPDto>> GetPagedAsync(int? departmentId, int page, int pageSize, string[]? statuses, int? planningYear, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PagedResult<PPMPDto>());
        public Task<PPPMSummaryDto> GetSummaryAsync(int? departmentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PPPMSummaryDto());
        public Task<List<PPMPChangeHistoryDto>> GetChangeHistoryAsync(int id, int? departmentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new List<PPMPChangeHistoryDto>());

        public Task<PPMPDetailDto?> GetByIdAsync(
            int id,
            int? departmentId,
            CancellationToken cancellationToken = default)
        {
            RequestedId = id;
            RequestedDepartmentId = departmentId;
            CancellationToken = cancellationToken;
            return Task.FromResult(Details);
        }
    }
}
