
using Drmc.Procurement.Application.Features.PlanningYearItems.DTO;
using Drmc.Procurement.Application.Features.PlanningYearItems.Interfaces;
using Drmc.Procurement.Domain.Entities.Planning;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.PlanningYearItems;

public class PlanningYearItemService : IPlanningYearItemService
{
    private readonly ApplicationDbContext _context;

    public PlanningYearItemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlanningYearItemDto>> GetAllAsync()
    {
        return await BuildQuery()
            .OrderBy(x => x.CategoryName)
            .ThenBy(x => x.ItemName)
            .ToListAsync();
    }

    public async Task<List<PlanningYearItemDto>>
        GetByPlanningYearCategoryIdAsync(int planningYearCategoryId)
    {
        return await BuildQuery()
            .Where(x =>
                x.PlanningYearCategoryId == planningYearCategoryId)
            .OrderBy(x => x.ItemName)
            .ToListAsync();
    }

    public async Task<PlanningYearItemDto?> GetByIdAsync(int id)
    {
        return await BuildQuery()
            .FirstOrDefaultAsync(x =>
                x.PlanningYearItemId == id);
    }

    public async Task<PlanningYearItemDto> AssignAsync(
        AssignPlanningYearItemRequest request)
    {
        if (request.ABCPerUnit < 0)
        {
            throw new InvalidOperationException(
                "ABC per unit cannot be negative.");
        }

        var planningYearCategory =
            await _context.PlanningYearCategories
                .FirstOrDefaultAsync(x =>
                    x.PlanningYearCategoryId ==
                    request.PlanningYearCategoryId);

        if (planningYearCategory == null)
        {
            throw new KeyNotFoundException(
                "Planning year category was not found.");
        }

        if (!planningYearCategory.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot assign items to an inactive planning year category.");
        }

        var item = await _context.Items
            .FirstOrDefaultAsync(x =>
                x.ItemId == request.ItemId);

        if (item == null)
        {
            throw new KeyNotFoundException(
                "Catalogue item was not found.");
        }

        if (!item.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot assign an inactive catalogue item.");
        }

        if (item.CategoryId != planningYearCategory.CategoryId)
        {
            throw new InvalidOperationException(
                "The catalogue item does not belong to the selected procurement category.");
        }

        var alreadyAssigned =
            await _context.PlanningYearItems.AnyAsync(x =>
                x.PlanningYearCategoryId ==
                    request.PlanningYearCategoryId &&
                x.ItemId == request.ItemId);

        if (alreadyAssigned)
        {
            throw new InvalidOperationException(
                "This catalogue item is already assigned to this planning year category.");
        }

        var planningYearItem = new PlanningYearItem
        {
            PlanningYearCategoryId =
                request.PlanningYearCategoryId,

            ItemId = request.ItemId,

            ABCPerUnit = request.ABCPerUnit,

            IsActive = true
        };

        _context.PlanningYearItems.Add(planningYearItem);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(
            planningYearItem.PlanningYearItemId))!;
    }

    public async Task<PlanningYearItemDto?> UpdateAsync(
        int id,
        UpdatePlanningYearItemRequest request)
    {
        if (request.ABCPerUnit < 0)
        {
            throw new InvalidOperationException(
                "ABC per unit cannot be negative.");
        }

        var planningYearItem =
            await _context.PlanningYearItems
                .FirstOrDefaultAsync(x =>
                    x.PlanningYearItemId == id);

        if (planningYearItem == null)
        {
            return null;
        }

        planningYearItem.ABCPerUnit = request.ABCPerUnit;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> SetActiveStatusAsync(
        int id,
        bool isActive)
    {
        var planningYearItem =
            await _context.PlanningYearItems
                .FirstOrDefaultAsync(x =>
                    x.PlanningYearItemId == id);

        if (planningYearItem == null)
        {
            return false;
        }

        planningYearItem.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }

    private IQueryable<PlanningYearItemDto> BuildQuery()
    {
        return _context.PlanningYearItems
            .AsNoTracking()
            .Select(x => new PlanningYearItemDto
            {
                PlanningYearItemId = x.PlanningYearItemId,

                PlanningYearCategoryId =
                    x.PlanningYearCategoryId,

                PlanningYearId =
                    x.PlanningYearCategory.PlanningYearId,

                PlanningYear =
                    x.PlanningYearCategory.PlanningYear.Year,

                CategoryId =
                    x.PlanningYearCategory.CategoryId,

                CategoryCode =
                    x.PlanningYearCategory.Category.CategoryCode,

                CategoryName =
                    x.PlanningYearCategory.Category.CategoryName,

                ItemId = x.ItemId,

                ItemCode = x.Item.ItemCode,

                ItemName = x.Item.ItemName,

                Unit = x.Item.Unit,

                Specifications = x.Item.Specifications,

                ABCPerUnit = x.ABCPerUnit,

                IsActive = x.IsActive
            });
    }
}
