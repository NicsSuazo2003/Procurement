using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Domain.Entities.Planning;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public sealed class CreatePPMPHandlerTests
{
    [Fact]
    public async Task Handle_ValidCatalogueItem_PersistsAggregateAndReturnsDetails()
    {
        var references = new FakeReferenceRepository
        {
            CatalogueItems = new Dictionary<int, Item>
            {
                [7] = new()
                {
                    ItemId = 7,
                    CategoryId = 3,
                    ItemCode = "CAT-7",
                    ItemName = "Monitor",
                    ItemType = "Equipment",
                    Unit = "piece",
                    Specifications = "24 inch"
                }
            }
        };
        var writer = new FakePPMPRepository();
        var reader = new FakeReadRepository
        {
            Details = new PPMPDetailDto { PPMPId = 42, Status = "Draft" }
        };
        var handler = new CreatePPMPHandler(writer, references, reader);
        using var cancellationSource = new CancellationTokenSource();

        var result = await handler.Handle(
            new CreatePPMPCommand(CreateRequest(), CreatedBy: 99, DepartmentId: 10),
            cancellationSource.Token);

        var saved = Assert.IsType<PPMP>(writer.SavedPPMP);
        var item = Assert.Single(saved.Items);
        var allocation = Assert.Single(item.SectionAllocations);

        Assert.Equal(42, result.PPMPId);
        Assert.Matches(@"^PPMP-2028-[0-9A-F]{8}$", saved.PPMPReference);
        Assert.Equal(10, saved.DepartmentId);
        Assert.Equal(2028, saved.PlanningYearId);
        Assert.Equal(99, saved.CreatedBy);
        Assert.Equal("Draft", saved.Status);
        Assert.Equal(25, item.EstimatedBudget);
        Assert.Equal(25, saved.EstimatedBudget);
        Assert.Equal("CAT-7", item.ItemCode);
        Assert.Equal("Monitor", item.ItemDescription);
        Assert.Equal("Equipment", item.ItemType);
        Assert.Equal("piece", item.Unit);
        Assert.Equal("24 inch", item.Specifications);
        Assert.Equal("For clinical workstations", item.GeneralDescriptionAndObjective);
        Assert.Equal("Goods", item.ProjectType);
        Assert.Equal(2, allocation.Quantity);
        Assert.Equal(20, allocation.SectionId);
        Assert.Equal(cancellationSource.Token, writer.CancellationToken);
        Assert.Equal(cancellationSource.Token, reader.CancellationToken);
        Assert.Equal(42, reader.RequestedId);
        Assert.Equal(10, reader.RequestedDepartmentId);
    }

    [Fact]
    public async Task Handle_DepartmentScopeMismatch_RejectsBeforeLoadingReferences()
    {
        var references = new FakeReferenceRepository();
        var writer = new FakePPMPRepository();
        var handler = new CreatePPMPHandler(writer, references, new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreatePPMPCommand(CreateRequest(), CreatedBy: 99, DepartmentId: 11)));

        Assert.Equal("You can only create a PPMP for your own department.", exception.Message);
        Assert.Equal(0, references.LookupCount);
        Assert.Null(writer.SavedPPMP);
    }

    [Fact]
    public async Task Handle_CategoryNotAssignedToPlanningYear_RejectsWithoutSaving()
    {
        var references = new FakeReferenceRepository { ActiveCategoryIds = Array.Empty<int>() };
        var writer = new FakePPMPRepository();
        var handler = new CreatePPMPHandler(writer, references, new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreatePPMPCommand(CreateRequest(), CreatedBy: 99, DepartmentId: 10)));

        Assert.Equal(
            "Category ID 3 is inactive or not assigned to the selected planning year.",
            exception.Message);
        Assert.Null(writer.SavedPPMP);
    }

    [Fact]
    public async Task Handle_AllocationDoesNotMatchItemQuantity_RejectsWithoutSaving()
    {
        var request = CreateRequest();
        request.Items[0].SectionAllocations[0].Quantity = 1;
        var writer = new FakePPMPRepository();
        var handler = new CreatePPMPHandler(writer, new FakeReferenceRepository(), new FakeReadRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreatePPMPCommand(request, CreatedBy: 99, DepartmentId: 10)));

        Assert.Equal(
            "Section allocation quantities for 'Monitor' must equal its total quantity.",
            exception.Message);
        Assert.Null(writer.SavedPPMP);
    }

    private static CreatePPMPRequest CreateRequest() =>
        new()
        {
            DepartmentId = 10,
            PlanningYearId = 2028,
            Items =
            [
                new CreatePPMPItemRequest
                {
                    ItemId = 7,
                    CategoryId = 3,
                    ItemDescription = "Request description is replaced by the catalogue value",
                    Unit = "request unit",
                    GeneralDescriptionAndObjective = " For clinical workstations ",
                    ProjectType = " Goods ",
                    Quantity = 2,
                    UnitPrice = 12.5m,
                    SectionAllocations =
                    [
                        new CreatePPMPSectionAllocationRequest
                        {
                            SectionId = 20,
                            Quantity = 2
                        }
                    ]
                }
            ]
        };

    private sealed class FakePPMPRepository : IPPMPRepository
    {
        public PPMP? SavedPPMP { get; private set; }
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
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PPMP?>(null);

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

        public Task SaveNewAsync(PPMP ppmp, CancellationToken cancellationToken = default)
        {
            SavedPPMP = ppmp;
            SavedPPMP.PPMPId = 42;
            CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
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

    private sealed class FakeReferenceRepository : IPPMPReferenceRepository
    {
        public Department? Department { get; init; } = new() { DepartmentId = 10, IsActive = true };
        public PlanningYear? PlanningYear { get; init; } = new() { PlanningYearId = 2028, Year = 2028, Status = "Open" };
        public User? Creator { get; init; } = new() { UserId = 99, DepartmentId = 10, IsActive = true };
        public IReadOnlyCollection<int> ActiveCategoryIds { get; init; } = [3];
        public IReadOnlyDictionary<int, Item> CatalogueItems { get; init; } =
            new Dictionary<int, Item>
            {
                [7] = new()
                {
                    ItemId = 7,
                    CategoryId = 3,
                    ItemCode = "CAT-7",
                    ItemName = "Monitor",
                    ItemType = "Equipment",
                    Unit = "piece",
                    Specifications = "24 inch"
                }
            };
        public IReadOnlyCollection<int> ActiveSectionIds { get; init; } = [20];
        public int LookupCount { get; private set; }

        public Task<Department?> GetActiveDepartmentAsync(
            int departmentId,
            CancellationToken cancellationToken = default)
        {
            LookupCount++;
            return Task.FromResult(Department);
        }

        public Task<PlanningYear?> GetPlanningYearAsync(
            int planningYearId,
            CancellationToken cancellationToken = default)
        {
            LookupCount++;
            return Task.FromResult(PlanningYear);
        }

        public Task<User?> GetActiveUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            LookupCount++;
            return Task.FromResult(Creator);
        }

        public Task<bool> IsActiveProcurementStaffAsync(
            int userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyCollection<int>> GetActiveCategoryIdsAsync(
            int planningYearId,
            IReadOnlyCollection<int> categoryIds,
            CancellationToken cancellationToken = default)
        {
            LookupCount++;
            IReadOnlyCollection<int> ids = ActiveCategoryIds.Intersect(categoryIds).ToArray();
            return Task.FromResult(ids);
        }

        public Task<IReadOnlyDictionary<int, Item>> GetActiveItemsByIdsAsync(
            IReadOnlyCollection<int> itemIds,
            CancellationToken cancellationToken = default)
        {
            LookupCount++;
            IReadOnlyDictionary<int, Item> items = CatalogueItems
                .Where(entry => itemIds.Contains(entry.Key))
                .ToDictionary(entry => entry.Key, entry => entry.Value);
            return Task.FromResult(items);
        }

        public Task<IReadOnlyCollection<int>> GetActiveDepartmentSectionIdsAsync(
            int departmentId,
            IReadOnlyCollection<int> sectionIds,
            CancellationToken cancellationToken = default)
        {
            LookupCount++;
            IReadOnlyCollection<int> ids = ActiveSectionIds.Intersect(sectionIds).ToArray();
            return Task.FromResult(ids);
        }

        public Task<IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Organization.DepartmentSection>>
            GetActiveDepartmentSectionsAsync(
                int departmentId,
                IReadOnlyCollection<int> sectionIds,
                CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Organization.DepartmentSection>>(
                new Dictionary<int, Drmc.Procurement.Domain.Entities.Organization.DepartmentSection>());
    }
}
