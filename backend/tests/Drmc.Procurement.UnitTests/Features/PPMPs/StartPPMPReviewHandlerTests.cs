using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public sealed class StartPPMPReviewHandlerTests
{
    [Fact]
    public async Task Handle_UnitHeadApprovedPPMP_TransitionsToUnderReview()
    {
        var ppmp = new PPMP { PPMPId = 9, Status = "Approved by Unit Head" };
        var repository = new FakePPMPRepository { PPMP = ppmp };
        var readRepository = new FakeReadRepository
        {
            Details = new PPMPDetailDto { PPMPId = 9, Status = "Under Review" }
        };
        var handler = new StartPPMPReviewHandler(repository, readRepository);
        using var cancellationSource = new CancellationTokenSource();

        var result = await handler.Handle(
            new StartPPMPReviewCommand(9),
            cancellationSource.Token);

        Assert.Equal("Under Review", ppmp.Status);
        Assert.NotNull(ppmp.UpdatedAt);
        Assert.Equal(1, repository.SaveCount);
        Assert.Equal(cancellationSource.Token, repository.CancellationToken);
        Assert.Equal(9, readRepository.RequestedId);
        Assert.Null(readRepository.RequestedDepartmentId);
        Assert.Equal(cancellationSource.Token, readRepository.CancellationToken);
        Assert.Equal(9, result.PPMPId);
    }

    [Fact]
    public async Task Handle_MissingPPMP_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository();
        var handler = new StartPPMPReviewHandler(repository, new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartPPMPReviewCommand(9)));

        Assert.Equal("PPMP with ID 9 was not found.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_StatusOtherThanUnitHeadApproved_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP { PPMPId = 9, Status = "Draft" }
        };
        var handler = new StartPPMPReviewHandler(repository, new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartPPMPReviewCommand(9)));

        Assert.Equal(
            "Only PPMPs approved by the Unit Head can enter Procurement review.",
            exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    private sealed class FakePPMPRepository : IPPMPRepository
    {
        public PPMP? PPMP { get; init; }
        public int SaveCount { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<PPMP?> GetForSubmissionAsync(
            int ppmpId,
            int userId,
            int departmentId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PPMP?>(null);

        public Task<PPMP?> GetForApprovalAsync(
            int ppmpId,
            int departmentId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PPMP?>(null);

        public Task<PPMP?> GetForReviewAsync(
            int ppmpId,
            CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
            return Task.FromResult(PPMP);
        }

        public Task<PPMP?> GetForUpdateAsync(
            int ppmpId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PPMP?>(null);

        public void AddApproval(PPMPApproval approval)
        {
        }

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
