using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public sealed class ApprovePPMPHandlerTests
{
    [Fact]
    public async Task Handle_ValidApproval_CreatesApprovalAndUpdatesStatusTogether()
    {
        var ppmp = new PPMP
        {
            PPMPId = 5,
            DepartmentId = 7,
            Status = "For Unit Head Approval"
        };
        var repository = new FakePPMPRepository { PPMP = ppmp };
        var references = new FakeReferenceRepository
        {
            Approver = new User { UserId = 12, DepartmentId = 7, IsActive = true }
        };
        var reader = new FakeReadRepository
        {
            Details = new PPMPDetailDto
            {
                PPMPId = 5,
                DepartmentId = 7,
                Status = "Approved by Unit Head"
            }
        };
        var handler = new ApprovePPMPHandler(repository, references, reader);
        using var cancellationSource = new CancellationTokenSource();

        var result = await handler.Handle(
            new ApprovePPMPCommand(
                Id: 5,
                Request: new ApprovePPMPRequest { Remarks = "  Approved  " },
                ApprovedBy: 12,
                DepartmentId: 7),
            cancellationSource.Token);

        Assert.Equal("Approved by Unit Head", ppmp.Status);
        Assert.NotNull(ppmp.UpdatedAt);
        Assert.Equal("Unit Head Approval", repository.AddedApproval?.ApprovalType);
        Assert.Equal(5, repository.AddedApproval?.PPMPId);
        Assert.Equal(12, repository.AddedApproval?.ApprovedBy);
        Assert.Equal("Approved", repository.AddedApproval?.Remarks);
        Assert.NotEqual(default, repository.AddedApproval?.ApprovedAt);
        Assert.Equal(1, repository.SaveCount);
        Assert.Equal(cancellationSource.Token, repository.CancellationToken);
        Assert.Equal(cancellationSource.Token, reader.CancellationToken);
        Assert.Equal(5, result.PPMPId);
        Assert.Equal(5, repository.RequestedPPMPId);
        Assert.Equal(7, repository.RequestedDepartmentId);
    }

    [Fact]
    public async Task Handle_WhitespaceRemarks_StoresNull()
    {
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP
            {
                PPMPId = 5,
                DepartmentId = 7,
                Status = "For Unit Head Approval"
            }
        };
        var handler = new ApprovePPMPHandler(
            repository,
            new FakeReferenceRepository(),
            new FakeReadRepository
            {
                Details = new PPMPDetailDto { PPMPId = 5, DepartmentId = 7 }
            });

        await handler.Handle(new ApprovePPMPCommand(
            5,
            new ApprovePPMPRequest { Remarks = "  " },
            12,
            7));

        Assert.Null(repository.AddedApproval?.Remarks);
    }

    [Fact]
    public async Task Handle_PpmpNotInDepartment_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository();
        var handler = new ApprovePPMPHandler(
            repository,
            new FakeReferenceRepository(),
            new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ApprovePPMPCommand(5, new ApprovePPMPRequest(), 12, 7)));

        Assert.Equal("PPMP with ID 5 was not found.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_WrongStatus_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP { PPMPId = 5, DepartmentId = 7, Status = "Draft" }
        };
        var handler = new ApprovePPMPHandler(
            repository,
            new FakeReferenceRepository(),
            new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ApprovePPMPCommand(5, new ApprovePPMPRequest(), 12, 7)));

        Assert.Equal("Only PPMPs awaiting Unit Head approval can be approved.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_InactiveApprover_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP { PPMPId = 5, DepartmentId = 7, Status = "For Unit Head Approval" }
        };
        var handler = new ApprovePPMPHandler(
            repository,
            new FakeReferenceRepository { Approver = null },
            new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ApprovePPMPCommand(5, new ApprovePPMPRequest(), 12, 7)));

        Assert.Equal("The selected approver does not exist or is inactive.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_ApproverFromAnotherDepartment_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP { PPMPId = 5, DepartmentId = 7, Status = "For Unit Head Approval" }
        };
        var handler = new ApprovePPMPHandler(
            repository,
            new FakeReferenceRepository
            {
                Approver = new User { UserId = 12, DepartmentId = 8, IsActive = true }
            },
            new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ApprovePPMPCommand(5, new ApprovePPMPRequest(), 12, 7)));

        Assert.Equal("The approver does not belong to the PPMP department.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    private sealed class FakePPMPRepository : IPPMPRepository
    {
        public PPMP? PPMP { get; init; }
        public PPMPApproval? AddedApproval { get; private set; }
        public int RequestedPPMPId { get; private set; }
        public int RequestedDepartmentId { get; private set; }
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
            CancellationToken cancellationToken = default)
        {
            RequestedPPMPId = ppmpId;
            RequestedDepartmentId = departmentId;
            CancellationToken = cancellationToken;
            return Task.FromResult(PPMP);
        }

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

    private sealed class FakeReferenceRepository : IPPMPReferenceRepository
    {
        public User? Approver { get; init; } = new() { UserId = 12, DepartmentId = 7, IsActive = true };

        public Task<User?> GetActiveUserAsync(
            int userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Approver);

        public Task<bool> IsActiveProcurementStaffAsync(
            int userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Organization.DepartmentSection>>
            GetActiveDepartmentSectionsAsync(
                int departmentId,
                IReadOnlyCollection<int> sectionIds,
                CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Organization.DepartmentSection>>(
                new Dictionary<int, Drmc.Procurement.Domain.Entities.Organization.DepartmentSection>());

        public Task<Drmc.Procurement.Domain.Entities.Organization.Department?> GetActiveDepartmentAsync(
            int departmentId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Drmc.Procurement.Domain.Entities.Organization.Department?>(null);

        public Task<Drmc.Procurement.Domain.Entities.Planning.PlanningYear?> GetPlanningYearAsync(
            int planningYearId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Drmc.Procurement.Domain.Entities.Planning.PlanningYear?>(null);

        public Task<IReadOnlyCollection<int>> GetActiveCategoryIdsAsync(
            int planningYearId,
            IReadOnlyCollection<int> categoryIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<int>>([]);

        public Task<IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Procurement.Item>>
            GetActiveItemsByIdsAsync(
                IReadOnlyCollection<int> itemIds,
                CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Procurement.Item>>(
                new Dictionary<int, Drmc.Procurement.Domain.Entities.Procurement.Item>());

        public Task<IReadOnlyCollection<int>> GetActiveDepartmentSectionIdsAsync(
            int departmentId,
            IReadOnlyCollection<int> sectionIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<int>>([]);
    }

    private sealed class FakeReadRepository : IPPMPReadRepository
    {
        public PPMPDetailDto? Details { get; init; }
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
            CancellationToken = cancellationToken;
            return Task.FromResult(Details);
        }
    }
}
