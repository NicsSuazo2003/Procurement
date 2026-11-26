using Drmc.Procurement.Application.Features.APP.DTO;

namespace Drmc.Procurement.Application.Features.APP.Interfaces;

public interface IAPPService
{
    Task<IReadOnlyList<APPListDto>> GetAllAsync();

    Task<APPDetailDto?> GetByIdAsync(int id);

    Task<GenerateAPPResultDto> GenerateAsync(
        int planningYearId,
        int createdBy);

        Task<GenerateAPPResultDto> AddReviewedPPMPsAsync(
            int appId,
            int addedBy);

        Task<APPDetailDto> MarkPreparedAsync(int appId, int preparedBy);

        Task<APPDetailDto> MarkRecommendedAsync(int appId, int recommendedBy);

        Task<APPDetailDto> MarkApprovedAsync(int appId, int approvedBy);
}