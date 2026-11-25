using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Domain.Entities.Planning;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public sealed class CompletePPMPReviewHandlerTests
{
    [Theory]
    [InlineData("Under Review")]
    [InlineData("Updated")]
    public async Task Handle_AllowedStatusAndActiveProcurementReviewer_MarksReviewed(string status)
    {
        var ppmp = new PPMP { PPMPId = 9, Status = status };
        var repository = new FakePPMPRepository { PPMP = ppmp };
        var references = new FakeReferenceRepository { IsProcurementStaff = true };
        var readRepository = new FakeReadRepository
        {
            Details = new PPMPDetailDto { PPMPId = 9, Status = "Reviewed" }
        };
        var handler = new CompletePPMPReviewHandler(repository, references, readRepository);
        using var cancellationSource = new CancellationTokenSource();

        var result = await handler.Handle(
            new CompletePPMPReviewCommand(9, ReviewedBy: 22),
            cancellationSource.Token);

        Assert.Equal("Reviewed", ppmp.Status);
        Assert.NotNull(ppmp.UpdatedAt);
        Assert.Equal(1, repository.SaveCount);
        Assert.Equal(22, references.RequestedUserId);
        Assert.Equal(cancellationSource.Token, repository.CancellationToken);
        Assert.Equal(cancellationSource.Token, references.CancellationToken);
        Assert.Equal(9, readRepository.RequestedId);
        Assert.Null(readRepository.RequestedDepartmentId);
        Assert.Equal(9, result.PPMPId);
    }

    [Fact]
    public async Task Handle_MissingPPMP_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository();
        var handler = new CompletePPMPReviewHandler(
            repository,
            new FakeReferenceRepository(),
            new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CompletePPMPReviewCommand(9, 22)));

        Assert.Equal("PPMP with ID 9 was not found.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_DisallowedStatus_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository { PPMP = new PPMP { PPMPId = 9, Status = "Draft" } };
        var handler = new CompletePPMPReviewHandler(
            repository,
            new FakeReferenceRepository(),
            new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CompletePPMPReviewCommand(9, 22)));

        Assert.Equal(
            "Only PPMPs under review or already updated can be marked as Reviewed.",
            exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_InactiveReviewer_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository { PPMP = new PPMP { PPMPId = 9, Status = "Under Review" } };
        var handler = new CompletePPMPReviewHandler(
            repository,
            new FakeReferenceRepository { Reviewer = null },
            new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CompletePPMPReviewCommand(9, 22)));

        Assert.Equal("The reviewer does not exist or is inactive.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_ReviewerWithoutProcurementRole_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository { PPMP = new PPMP { PPMPId = 9, Status = "Under Review" } };
        var handler = new CompletePPMPReviewHandler(
            repository,
            new FakeReferenceRepository { IsProcurementStaff = false },
            new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CompletePPMPReviewCommand(9, 22)));

        Assert.Equal("Only Procurement Staff can complete a PPMP review.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    private sealed class FakePPMPRepository : IPPMPRepository
    {
        public PPMP? PPMP { get; init; }
        public int SaveCount { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<PPMP?> GetForSubmissionAsync(int ppmpId, int userId, int departmentId, CancellationToken cancellationToken = default) => Task.FromResult<PPMP?>(null);
        public Task<PPMP?> GetForApprovalAsync(int ppmpId, int departmentId, CancellationToken cancellationToken = default) => Task.FromResult<PPMP?>(null);
        public Task<PPMP?> GetForReviewAsync(int ppmpId, CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
            return Task.FromResult(PPMP);
        }
        public Task<PPMP?> GetForUpdateAsync(int ppmpId, CancellationToken cancellationToken = default) => Task.FromResult<PPMP?>(null);
        public void AddApproval(PPMPApproval approval) { }
        public Task SaveUpdateAsync(PPMP ppmp, IReadOnlyCollection<PPMPSectionAllocation> removedAllocations, IReadOnlyCollection<PPMPItem> removedItems, Drmc.Procurement.Domain.Entities.Audit.PPMPChangeHistory history, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveNewAsync(PPMP ppmp, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeReferenceRepository : IPPMPReferenceRepository
    {
        public User? Reviewer { get; init; } = new() { UserId = 22, IsActive = true };
        public bool IsProcurementStaff { get; init; } = true;
        public int RequestedUserId { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<User?> GetActiveUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            RequestedUserId = userId;
            CancellationToken = cancellationToken;
            return Task.FromResult(Reviewer);
        }

        public Task<bool> IsActiveProcurementStaffAsync(int userId, CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
            return Task.FromResult(IsProcurementStaff);
        }

        public Task<IReadOnlyDictionary<int, DepartmentSection>> GetActiveDepartmentSectionsAsync(int departmentId, IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<int, DepartmentSection>>(new Dictionary<int, DepartmentSection>());

        public Task<Department?> GetActiveDepartmentAsync(int departmentId, CancellationToken cancellationToken = default) => Task.FromResult<Department?>(null);
        public Task<PlanningYear?> GetPlanningYearAsync(int planningYearId, CancellationToken cancellationToken = default) => Task.FromResult<PlanningYear?>(null);
        public Task<IReadOnlyCollection<int>> GetActiveCategoryIdsAsync(int planningYearId, IReadOnlyCollection<int> categoryIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<int>>([]);
        public Task<IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Procurement.Item>> GetActiveItemsByIdsAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Procurement.Item>>(new Dictionary<int, Drmc.Procurement.Domain.Entities.Procurement.Item>());
        public Task<IReadOnlyCollection<int>> GetActiveDepartmentSectionIdsAsync(int departmentId, IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<int>>([]);
    }

    private sealed class FakeReadRepository : IPPMPReadRepository
    {
        public PPMPDetailDto? Details { get; init; }
        public int RequestedId { get; private set; }
        public int? RequestedDepartmentId { get; private set; }

        public Task<PagedResult<PPMPDto>> GetPagedAsync(int? departmentId, int page, int pageSize, string[]? statuses, int? planningYear, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<PPMPDto>());
        public Task<PPPMSummaryDto> GetSummaryAsync(int? departmentId, CancellationToken cancellationToken = default) => Task.FromResult(new PPPMSummaryDto());
        public Task<List<PPMPChangeHistoryDto>> GetChangeHistoryAsync(int id, int? departmentId, CancellationToken cancellationToken = default) => Task.FromResult(new List<PPMPChangeHistoryDto>());

        public Task<PPMPDetailDto?> GetByIdAsync(int id, int? departmentId, CancellationToken cancellationToken = default)
        {
            RequestedId = id;
            RequestedDepartmentId = departmentId;
            return Task.FromResult(Details);
        }
    }
}
