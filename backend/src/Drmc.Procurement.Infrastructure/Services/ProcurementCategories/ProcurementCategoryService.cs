using Drmc.Procurement.Application.Features.ProcurementCategories.DTO;
using Drmc.Procurement.Application.Features.ProcurementCategories.Interfaces;
using Drmc.Procurement.Domain.Entities.Audit;
using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.ProcurementCategories;

public class ProcurementCategoryService : IProcurementCategoryService
{
    private readonly ApplicationDbContext _context;

    public ProcurementCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProcurementCategoryDto>> GetAllAsync()
    {
        return await _context.ProcurementCategories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new ProcurementCategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryCode = c.CategoryCode,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProcurementCategoryDto?> GetByIdAsync(int id)
    {
        return await _context.ProcurementCategories
            .AsNoTracking()
            .Where(c => c.CategoryId == id)
            .Select(c => new ProcurementCategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryCode = c.CategoryCode,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<ProcurementCategoryAuditLogDto>> GetHistoryAsync(
        int id)
    {
        var categoryExists = await _context.ProcurementCategories
            .AnyAsync(c => c.CategoryId == id);

        if (!categoryExists)
        {
            throw new KeyNotFoundException(
                "Procurement category not found.");
        }

        return await _context.ProcurementCategoryAuditLogs
            .AsNoTracking()
            .Where(a => a.CategoryId == id)
            .OrderByDescending(a => a.ChangedAt)
            .ThenByDescending(a => a.CategoryAuditLogId)
            .Select(a => new ProcurementCategoryAuditLogDto
            {
                CategoryAuditLogId = a.CategoryAuditLogId,
                CategoryId = a.CategoryId,
                CategoryCode = a.Category.CategoryCode,
                CategoryName = a.Category.CategoryName,
                ChangedByUserId = a.ChangedByUserId,
                ChangedByName = a.ChangedByUser.FullName,
                Action = a.Action,
                Reason = a.Reason,
                PreviousCategoryCode = a.PreviousCategoryCode,
                PreviousCategoryName = a.PreviousCategoryName,
                PreviousDescription = a.PreviousDescription,
                PreviousIsActive = a.PreviousIsActive,
                UpdatedCategoryCode = a.UpdatedCategoryCode,
                UpdatedCategoryName = a.UpdatedCategoryName,
                UpdatedDescription = a.UpdatedDescription,
                UpdatedIsActive = a.UpdatedIsActive,
                ChangedAt = a.ChangedAt
            })
            .ToListAsync();
    }

    public async Task<ProcurementCategoryDto> CreateAsync(
        CreateProcurementCategoryRequest request,
        int changedByUserId)
    {
        await EnsureActiveUserAsync(changedByUserId);

        var categoryCode = request.CategoryCode.Trim();
        var categoryName = request.CategoryName.Trim();
        var description = request.Description?.Trim();

        var codeExists = await _context.ProcurementCategories
            .AnyAsync(c => c.CategoryCode == categoryCode);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "A procurement category with this code already exists.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var category = new ProcurementCategory
        {
            CategoryCode = categoryCode,
            CategoryName = categoryName,
            Description = description,
            IsActive = true
        };

        _context.ProcurementCategories.Add(category);

        // Save first to generate CategoryId.
        await _context.SaveChangesAsync();

        var auditLog = new ProcurementCategoryAuditLog
        {
            CategoryId = category.CategoryId,
            ChangedByUserId = changedByUserId,
            Action = "Added",
            UpdatedCategoryCode = category.CategoryCode,
            UpdatedCategoryName = category.CategoryName,
            UpdatedDescription = category.Description,
            UpdatedIsActive = category.IsActive,
            ChangedAt = DateTime.UtcNow
        };

        _context.ProcurementCategoryAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToDto(category);
    }

    public async Task<ProcurementCategoryDto?> UpdateAsync(
        int id,
        CreateProcurementCategoryRequest request,
        int changedByUserId)
    {
        await EnsureActiveUserAsync(changedByUserId);

        var category = await _context.ProcurementCategories
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category is null)
        {
            return null;
        }

        var categoryCode = request.CategoryCode.Trim();
        var categoryName = request.CategoryName.Trim();
        var description = request.Description?.Trim();

        var codeExists = await _context.ProcurementCategories
            .AnyAsync(c =>
                c.CategoryCode == categoryCode &&
                c.CategoryId != id);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Another procurement category already uses this code.");
        }

        // Capture the old values before modifying the category.
        var previousCode = category.CategoryCode;
        var previousName = category.CategoryName;
        var previousDescription = category.Description;
        var previousIsActive = category.IsActive;

        var hasChanges =
            previousCode != categoryCode ||
            previousName != categoryName ||
            previousDescription != description;

        if (!hasChanges)
        {
            return MapToDto(category);
        }

        category.CategoryCode = categoryCode;
        category.CategoryName = categoryName;
        category.Description = description;

        var auditLog = new ProcurementCategoryAuditLog
        {
            CategoryId = category.CategoryId,
            ChangedByUserId = changedByUserId,
            Action = "Updated",

            PreviousCategoryCode = previousCode,
            PreviousCategoryName = previousName,
            PreviousDescription = previousDescription,
            PreviousIsActive = previousIsActive,

            UpdatedCategoryCode = category.CategoryCode,
            UpdatedCategoryName = category.CategoryName,
            UpdatedDescription = category.Description,
            UpdatedIsActive = category.IsActive,

            ChangedAt = DateTime.UtcNow
        };

        _context.ProcurementCategoryAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> SetActiveStatusAsync(
        int id,
        bool isActive,
        int changedByUserId)
    {
        await EnsureActiveUserAsync(changedByUserId);

        var category = await _context.ProcurementCategories
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category is null)
        {
            return false;
        }

        // Do not create duplicate logs for an unchanged status.
        if (category.IsActive == isActive)
        {
            return true;
        }

        var previousCode = category.CategoryCode;
        var previousName = category.CategoryName;
        var previousDescription = category.Description;
        var previousIsActive = category.IsActive;

        category.IsActive = isActive;

        var auditLog = new ProcurementCategoryAuditLog
        {
            CategoryId = category.CategoryId,
            ChangedByUserId = changedByUserId,
            Action = isActive ? "Activated" : "Deactivated",

            PreviousCategoryCode = previousCode,
            PreviousCategoryName = previousName,
            PreviousDescription = previousDescription,
            PreviousIsActive = previousIsActive,

            UpdatedCategoryCode = category.CategoryCode,
            UpdatedCategoryName = category.CategoryName,
            UpdatedDescription = category.Description,
            UpdatedIsActive = category.IsActive,

            ChangedAt = DateTime.UtcNow
        };

        _context.ProcurementCategoryAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task EnsureActiveUserAsync(int userId)
    {
        var isActive = await _context.Users
            .AnyAsync(u => u.UserId == userId && u.IsActive);

        if (!isActive)
        {
            throw new InvalidOperationException(
                "The authenticated user does not exist or is inactive.");
        }
    }

    private static ProcurementCategoryDto MapToDto(
        ProcurementCategory category)
    {
        return new ProcurementCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryCode = category.CategoryCode,
            CategoryName = category.CategoryName,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }
}