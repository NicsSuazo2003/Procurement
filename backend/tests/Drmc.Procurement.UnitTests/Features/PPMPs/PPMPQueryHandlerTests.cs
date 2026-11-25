using Drmc.Procurement.Application.Features.PPMPs.Commands;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Application.Features.PPMPs.Queries;

namespace Drmc.Procurement.UnitTests.Features.PPMPs;

public sealed class PPMPQueryHandlerTests
{
    [Fact]
    public async Task GetPagedPPMPsHandler_ForwardsFiltersAndCancellation()
    {
        var repository = new FakeReadRepository();
        var handler = new GetPagedPPMPsHandler(repository);
        using var cancellationSource = new CancellationTokenSource();

        await handler.Handle(
            new GetPagedPPMPsQuery(DepartmentId: 7, Page: 2, PageSize: 50, ["Draft"], PlanningYear: 2028),
            cancellationSource.Token);

        Assert.Equal((7, 2, 50, 2028), repository.PagedParameters);
        Assert.Equal("Draft", Assert.Single(repository.Statuses!));
        Assert.Equal(cancellationSource.Token, repository.CancellationToken);
    }

    [Fact]
    public async Task GetPPPMSummaryHandler_ForwardsDepartmentScopeAndCancellation()
    {
        var repository = new FakeReadRepository();
        var handler = new GetPPPMSummaryHandler(repository);
        using var cancellationSource = new CancellationTokenSource();

        await handler.Handle(new GetPPPMSummaryQuery(DepartmentId: 7), cancellationSource.Token);

        Assert.Equal(7, repository.DepartmentId);
        Assert.Equal(cancellationSource.Token, repository.CancellationToken);
    }

    [Fact]
    public async Task GetPPMPByIdHandler_ForwardsIdAndDepartmentScope()
    {
        var repository = new FakeReadRepository();
        var handler = new GetPPMPByIdHandler(repository);

        await handler.Handle(new GetPPMPByIdQuery(Id: 4, DepartmentId: 7));

        Assert.Equal((4, 7), repository.DetailParameters);
    }

    [Fact]
    public async Task GetPPMPChangeHistoryHandler_ForwardsIdAndDepartmentScope()
    {
        var repository = new FakeReadRepository();
        var handler = new GetPPMPChangeHistoryHandler(repository);

        await handler.Handle(new GetPPMPChangeHistoryQuery(Id: 4, DepartmentId: 7));

        Assert.Equal((4, 7), repository.HistoryParameters);
    }

    private sealed class FakeReadRepository : IPPMPReadRepository
    {
        public (int? DepartmentId, int Page, int PageSize, int? PlanningYear) PagedParameters { get; private set; }
        public string[]? Statuses { get; private set; }
        public int? DepartmentId { get; private set; }
        public (int Id, int? DepartmentId) DetailParameters { get; private set; }
        public (int Id, int? DepartmentId) HistoryParameters { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<PagedResult<PPMPDto>> GetPagedAsync(
            int? departmentId,
            int page,
            int pageSize,
            string[]? statuses,
            int? planningYear,
            CancellationToken cancellationToken = default)
        {
            PagedParameters = (departmentId, page, pageSize, planningYear);
            Statuses = statuses;
            CancellationToken = cancellationToken;
            return Task.FromResult(new PagedResult<PPMPDto>());
        }

        public Task<PPPMSummaryDto> GetSummaryAsync(
            int? departmentId,
            CancellationToken cancellationToken = default)
        {
            DepartmentId = departmentId;
            CancellationToken = cancellationToken;
            return Task.FromResult(new PPPMSummaryDto());
        }

        public Task<PPMPDetailDto?> GetByIdAsync(
            int id,
            int? departmentId,
            CancellationToken cancellationToken = default)
        {
            DetailParameters = (id, departmentId);
            CancellationToken = cancellationToken;
            return Task.FromResult<PPMPDetailDto?>(null);
        }

        public Task<List<PPMPChangeHistoryDto>> GetChangeHistoryAsync(
            int id,
            int? departmentId,
            CancellationToken cancellationToken = default)
        {
            HistoryParameters = (id, departmentId);
            CancellationToken = cancellationToken;
            return Task.FromResult(new List<PPMPChangeHistoryDto>());
        }
    }
}
