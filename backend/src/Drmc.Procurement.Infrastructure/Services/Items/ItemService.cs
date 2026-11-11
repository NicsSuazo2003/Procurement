using System.Data;
using Drmc.Procurement.Application.Features.Items.DTO;
using Drmc.Procurement.Application.Features.Items.Interfaces;
using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.Items;

public class ItemService : IItemService
{
    private readonly ApplicationDbContext _context;

    public ItemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemDto>> GetAllAsync()
    {
        return await BuildQuery().ToListAsync();
    }

    public async Task<List<ItemDto>> GetByCategoryIdAsync(int categoryId)
    {
        return await BuildQuery()
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<ItemDto?> GetByIdAsync(int id)
    {
        return await BuildQuery()
            .FirstOrDefaultAsync(x => x.ItemId == id);
    }

    public async Task<ItemDto> CreateAsync(CreateItemRequest request)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var category = await _context.ProcurementCategories
            .FirstOrDefaultAsync(x => x.CategoryId == request.CategoryId);

        if (category is null)
            throw new KeyNotFoundException("Procurement category not found.");

        var categoryCode = category.CategoryCode.Trim();

        // Read existing codes in this category while holding
        // the serializable transaction's range locks.
        var existingCodes = await _context.Items
            .Where(x => x.CategoryId == request.CategoryId)
            .Select(x => x.ItemCode)
            .ToListAsync();

        var prefix = $"{categoryCode}-";

        var highestNumber = existingCodes
            .Where(code => code.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
            .Select(code => code[prefix.Length..])
            .Where(suffix => int.TryParse(suffix, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        var nextNumber = highestNumber + 1;
        var generatedCode = $"{prefix}{nextNumber:D4}";

        var item = new Item
        {
            CategoryId = request.CategoryId,
            ItemCode = generatedCode,
            ItemName = request.ItemName.Trim(),
            ItemType = request.ItemType?.Trim(),
            Unit = request.Unit.Trim(),
            Specifications = request.Specifications?.Trim(),
            IsActive = true
        };

        _context.Items.Add(item);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (await GetByIdAsync(item.ItemId))!;
    }

    public async Task<ItemDto?> UpdateAsync(
        int id,
        UpdateItemRequest request)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(x => x.ItemId == id);

        if (item is null)
            return null;

        var categoryExists = await _context.ProcurementCategories
            .AnyAsync(x => x.CategoryId == request.CategoryId);

        if (!categoryExists)
            throw new KeyNotFoundException("Procurement category not found.");

        var duplicateCode = await _context.Items
            .AnyAsync(x =>
                x.ItemId != id &&
                x.CategoryId == request.CategoryId &&
                x.ItemCode == item.ItemCode);

        if (duplicateCode)
            throw new InvalidOperationException(
                "This item code already exists in the selected category.");

        // Preserve the existing generated ItemCode.
        item.CategoryId = request.CategoryId;
        item.ItemName = request.ItemName.Trim();
        item.ItemType = request.ItemType?.Trim();
        item.Unit = request.Unit.Trim();
        item.Specifications = request.Specifications?.Trim();

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(x => x.ItemId == id);

        if (item is null)
            return false;

        item.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }

    private IQueryable<ItemDto> BuildQuery()
    {
        return _context.Items
            .AsNoTracking()
            .Select(x => new ItemDto
            {
                ItemId = x.ItemId,
                CategoryId = x.CategoryId,
                CategoryCode = x.Category.CategoryCode,
                CategoryName = x.Category.CategoryName,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                ItemType = x.ItemType,
                Unit = x.Unit,
                Specifications = x.Specifications,
                IsActive = x.IsActive
            });
    }
    public async Task<List<PlanningYearCatalogueItemDto>>
    GetByPlanningYearAndCategoryAsync(int planningYear, int categoryId)
{
    return await _context.PlanningYearCategories
        .AsNoTracking()
        .Where(x =>
            x.PlanningYear.Year == planningYear &&
            x.CategoryId == categoryId &&
            x.IsActive)
        .SelectMany(x => x.Items
            .Where(pyi =>
                pyi.IsActive &&
                pyi.Item.IsActive)
            .Select(pyi => new PlanningYearCatalogueItemDto
            {
                PlanningYear = x.PlanningYear.Year,

                ItemId = pyi.Item.ItemId,
                ItemCode = pyi.Item.ItemCode,
                ItemName = pyi.Item.ItemName,
                ItemType = pyi.Item.ItemType,
                Unit = pyi.Item.Unit,
                Specifications = pyi.Item.Specifications,

                CategoryId = x.Category.CategoryId,
                CategoryCode = x.Category.CategoryCode,
                CategoryName = x.Category.CategoryName,

                ABCPerUnit = pyi.ABCPerUnit
            }))
        .OrderBy(x => x.ItemCode)
        .ToListAsync();
}
}