using Drmc.Procurement.Application.Features.Items.DTO;

namespace Drmc.Procurement.Application.Features.Items.Interfaces;

public interface IItemService
{
    Task<List<ItemDto>> GetAllAsync();

    Task<List<ItemDto>> GetByCategoryIdAsync(int categoryId);

    Task<ItemDto?> GetByIdAsync(int id);

    Task<ItemDto> CreateAsync(CreateItemRequest request);

    Task<ItemDto?> UpdateAsync(int id, UpdateItemRequest request);

    Task<bool> SetActiveStatusAsync(int id, bool isActive);
    
    Task<List<PlanningYearCatalogueItemDto>>
    GetByPlanningYearAndCategoryAsync(int planningYear, int categoryId);}