using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Interfaces;

namespace Drmc.Procurement.Application.Features.UserCategoryPreferences.Queries;

public sealed record GetUserCategoryPreferencesQuery(int UserId, int PlanningYearId) : IQuery<List<UserCategoryPreferenceDto>>;

public sealed class GetUserCategoryPreferencesHandler(IUserCategoryPreferenceService service) : IRequestHandler<GetUserCategoryPreferencesQuery, List<UserCategoryPreferenceDto>>
{
    public Task<List<UserCategoryPreferenceDto>> Handle(GetUserCategoryPreferencesQuery request, CancellationToken cancellationToken = default) =>
        service.GetByUserAndPlanningYearAsync(request.UserId, request.PlanningYearId);
}
