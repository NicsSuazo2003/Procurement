using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Audit;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Domain.Entities.Planning;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public sealed class UpdatePPMPHandlerTests
{
    [Fact]
    public async Task Handle_ChangedExistingItem_UpdatesBudgetAndStoresSnapshots()
    {
        var oldAllocation = new PPMPSectionAllocation
        {
            AllocationId = 15,
            SectionId = 2,
            Quantity = 2,
            Section = new DepartmentSection
            {
                SectionId = 2,
                SectionCode = "OLD-SEC",
                SectionName = "Old Section"
            }
        };
        var item = new PPMPItem
        {
            PPMPItemId = 5,
            ItemCode = "OLD",
            ItemDescription = "Old item",
            Unit = "piece",
            GeneralDescriptionAndObjective = "Old objective",
            ProjectType = "Goods",
            Quantity = 2,
            UnitPrice = 10,
            EstimatedBudget = 20,
            SectionAllocations = [oldAllocation]
        };
        var ppmp = new PPMP
        {
            PPMPId = 1,
            DepartmentId = 10,
            PlanningYearId = 2027,
            Status = "Under Review",
            EstimatedBudget = 20,
            Items = [item]
        };
        var repository = new FakePPMPRepository { PPMP = ppmp };
        var readRepository = new FakeReadRepository
        {
            Details = new PPMPDetailDto { PPMPId = 1, Status = "Updated", EstimatedBudget = 30 }
        };
        var handler = new UpdatePPMPHandler(repository, new FakeReferenceRepository(), readRepository);
        using var cancellationSource = new CancellationTokenSource();

        var result = await handler.Handle(
            new UpdatePPMPCommand(1, Request(5, 3, 3), ChangedBy: 22),
            cancellationSource.Token);

        var historyItem = Assert.Single(repository.SavedHistory!.Items);
        var allocationSnapshot = Assert.Single(historyItem.SectionAllocations);
        Assert.Equal("Updated", ppmp.Status);
        Assert.Equal(30, ppmp.EstimatedBudget);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(10, item.UnitPrice);
        Assert.Equal("New item", item.ItemDescription);
        Assert.Equal("Changed for current requirements", repository.SavedHistory.Reason);
        Assert.Equal(20, repository.SavedHistory.PreviousBudget);
        Assert.Equal(30, repository.SavedHistory.UpdatedBudget);
        Assert.Null(historyItem.PreviousItemId);
        Assert.Equal("Old item", historyItem.PreviousDescription);
        Assert.Equal(2, historyItem.PreviousQuantity);
        Assert.Equal("New item", historyItem.UpdatedDescription);
        Assert.Equal(3, historyItem.UpdatedQuantity);
        Assert.Equal("OLD-SEC", allocationSnapshot.PreviousSectionCode);
        Assert.Equal("NEW-SEC", allocationSnapshot.UpdatedSectionCode);
        Assert.Equal(1, repository.SaveCount);
        Assert.Contains(oldAllocation, repository.RemovedAllocations);
        Assert.Equal(cancellationSource.Token, repository.CancellationToken);
        Assert.Equal(1, result.PPMPId);
    }

    [Fact]
    public async Task Handle_NoChanges_RejectsWithoutSavingHistory()
    {
        var item = new PPMPItem
        {
            PPMPItemId = 5,
            CategoryId = 4,
            ItemCode = "ITEM",
            ItemDescription = "Existing item",
            Unit = "piece",
            GeneralDescriptionAndObjective = "Objective",
            ProjectType = "Goods",
            Quantity = 2,
            UnitPrice = 10,
            EstimatedBudget = 20,
            SectionAllocations =
            [
                new PPMPSectionAllocation
                {
                    SectionId = 2,
                    Quantity = 2,
                    Section = new DepartmentSection
                    {
                        SectionId = 2,
                        SectionCode = "SEC",
                        SectionName = "Section"
                    }
                }
            ]
        };
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP
            {
                PPMPId = 1,
                DepartmentId = 10,
                PlanningYearId = 2027,
                Status = "Under Review",
                EstimatedBudget = 20,
                Items = [item]
            }
        };
        var handler = new UpdatePPMPHandler(
            repository,
            new FakeReferenceRepository
            {
                Sections = new Dictionary<int, DepartmentSection>
                {
                    [2] = new() { SectionId = 2, SectionCode = "SEC", SectionName = "Section" }
                }
            },
            new FakeReadRepository());
        var request = new UpdatePPMPRequest
        {
            Reason = "No actual changes",
            Items =
            [
                new UpdatePPMPItemRequest
                {
                    PPMPItemId = 5,
                    CategoryId = 4,
                    ItemCode = "ITEM",
                    ItemDescription = "Existing item",
                    Unit = "piece",
                    GeneralDescriptionAndObjective = "Objective",
                    ProjectType = "Goods",
                    Quantity = 2,
                    UnitPrice = 10,
                    SectionAllocations = [new UpdatePPMPSectionAllocationRequest { SectionId = 2, Quantity = 2 }]
                }
            ]
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdatePPMPCommand(1, request, ChangedBy: 22)));

        Assert.Equal("No changes were detected in the submitted PPMP.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
        Assert.Null(repository.SavedHistory);
    }

    [Fact]
    public async Task Handle_ItemIdFromAnotherPPMP_RejectsWithoutSaving()
    {
        var repository = new FakePPMPRepository
        {
            PPMP = new PPMP
            {
                PPMPId = 1,
                Status = "Under Review",
                Items = [new PPMPItem { PPMPItemId = 5 }]
            }
        };
        var handler = new UpdatePPMPHandler(repository, new FakeReferenceRepository(), new FakeReadRepository());
        var request = Request(99, 3, 30);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdatePPMPCommand(1, request, ChangedBy: 22)));

        Assert.Equal("One or more PPMP item IDs do not belong to this PPMP.", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    private static UpdatePPMPRequest Request(int ppmpItemId, decimal quantity, decimal allocationQuantity) =>
        new()
        {
            Reason = "  Changed for current requirements ",
            Items =
            [
                new UpdatePPMPItemRequest
                {
                    PPMPItemId = ppmpItemId,
                    CategoryId = 4,
                    ItemCode = "NEW",
                    ItemDescription = "New item",
                    Unit = "piece",
                    GeneralDescriptionAndObjective = "New objective",
                    ProjectType = "Goods",
                    Quantity = quantity,
                    UnitPrice = 10,
                    SectionAllocations =
                    [
                        new UpdatePPMPSectionAllocationRequest
                        {
                            SectionId = 2,
                            Quantity = allocationQuantity
                        }
                    ]
                }
            ]
        };

    private sealed class FakePPMPRepository : IPPMPRepository
    {
        public PPMP? PPMP { get; init; }
        public PPMPChangeHistory? SavedHistory { get; private set; }
        public IReadOnlyCollection<PPMPSectionAllocation> RemovedAllocations { get; private set; } = [];
        public int SaveCount { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<PPMP?> GetForSubmissionAsync(int ppmpId, int userId, int departmentId, CancellationToken cancellationToken = default) => Task.FromResult<PPMP?>(null);
        public Task<PPMP?> GetForApprovalAsync(int ppmpId, int departmentId, CancellationToken cancellationToken = default) => Task.FromResult<PPMP?>(null);
        public Task<PPMP?> GetForReviewAsync(int ppmpId, CancellationToken cancellationToken = default) => Task.FromResult<PPMP?>(null);
        public Task<PPMP?> GetForUpdateAsync(int ppmpId, CancellationToken cancellationToken = default) => Task.FromResult(PPMP);
        public void AddApproval(PPMPApproval approval) { }
        public Task SaveNewAsync(PPMP ppmp, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SaveUpdateAsync(
            PPMP ppmp,
            IReadOnlyCollection<PPMPSectionAllocation> removedAllocations,
            IReadOnlyCollection<PPMPItem> removedItems,
            PPMPChangeHistory history,
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            RemovedAllocations = removedAllocations;
            SavedHistory = history;
            CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeReferenceRepository : IPPMPReferenceRepository
    {
        public IReadOnlyCollection<int> ActiveCategoryIds { get; init; } = [4];
        public IReadOnlyCollection<int> ActiveSectionIds { get; init; } = [2];
        public IReadOnlyDictionary<int, DepartmentSection> Sections { get; init; } =
            new Dictionary<int, DepartmentSection>
            {
                [2] = new() { SectionId = 2, SectionCode = "NEW-SEC", SectionName = "New Section" }
            };

        public Task<User?> GetActiveUserAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(new User { UserId = userId, IsActive = true });
        public Task<bool> IsActiveProcurementStaffAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<Department?> GetActiveDepartmentAsync(int departmentId, CancellationToken cancellationToken = default) => Task.FromResult<Department?>(null);
        public Task<PlanningYear?> GetPlanningYearAsync(int planningYearId, CancellationToken cancellationToken = default) => Task.FromResult<PlanningYear?>(null);
        public Task<IReadOnlyCollection<int>> GetActiveCategoryIdsAsync(int planningYearId, IReadOnlyCollection<int> categoryIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<int>>(ActiveCategoryIds.Intersect(categoryIds).ToArray());
        public Task<IReadOnlyDictionary<int, Item>> GetActiveItemsByIdsAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyDictionary<int, Item>>(new Dictionary<int, Item>());
        public Task<IReadOnlyCollection<int>> GetActiveDepartmentSectionIdsAsync(int departmentId, IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<int>>(ActiveSectionIds.Intersect(sectionIds).ToArray());
        public Task<IReadOnlyDictionary<int, DepartmentSection>> GetActiveDepartmentSectionsAsync(int departmentId, IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyDictionary<int, DepartmentSection>>(Sections.Where(pair => sectionIds.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value));
    }

    private sealed class FakeReadRepository : IPPMPReadRepository
    {
        public PPMPDetailDto? Details { get; init; }
        public Task<PagedResult<PPMPDto>> GetPagedAsync(int? departmentId, int page, int pageSize, string[]? statuses, int? planningYear, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<PPMPDto>());
        public Task<PPPMSummaryDto> GetSummaryAsync(int? departmentId, CancellationToken cancellationToken = default) => Task.FromResult(new PPPMSummaryDto());
        public Task<List<PPMPChangeHistoryDto>> GetChangeHistoryAsync(int id, int? departmentId, CancellationToken cancellationToken = default) => Task.FromResult(new List<PPMPChangeHistoryDto>());
        public Task<PPMPDetailDto?> GetByIdAsync(int id, int? departmentId, CancellationToken cancellationToken = default) => Task.FromResult(Details);
    }
}
