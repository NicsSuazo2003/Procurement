using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Interfaces;

namespace Drmc.Procurement.Application.Features.UserCategoryPreferences.Commands;

public sealed record SaveUserCategoryPreferencesCommand(int UserId, SaveUserCategoryPreferencesRequest Request) : ICommand<List<UserCategoryPreferenceDto>>;

public sealed class SaveUserCategoryPreferencesHandler(IUserCategoryPreferenceService service) : IRequestHandler<SaveUserCategoryPreferencesCommand, List<UserCategoryPreferenceDto>>
{
    public Task<List<UserCategoryPreferenceDto>> Handle(SaveUserCategoryPreferencesCommand request, CancellationToken cancellationToken = default) =>
        service.SaveAsync(request.UserId, request.Request);
}
