
using Drmc.Procurement.Domain.Entities.APP;
using Drmc.Procurement.Domain.Entities.Audit;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Domain.Entities.Planning;
using Drmc.Procurement.Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Organization
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentSection> DepartmentSections => Set<DepartmentSection>();

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<UserCategoryPreference> UserCategoryPreferences
    => Set<UserCategoryPreference>();

    // Planning
    public DbSet<PlanningYear> PlanningYears => Set<PlanningYear>();
    public DbSet<PlanningYearCategory> PlanningYearCategories
        => Set<PlanningYearCategory>();

        public DbSet<PlanningYearItem> PlanningYearItems
    => Set<PlanningYearItem>();

    // Procurement
    public DbSet<ProcurementCategory> ProcurementCategories
        => Set<ProcurementCategory>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<PPMP> PPMPs => Set<PPMP>();
    public DbSet<PPMPItem> PPMPItems => Set<PPMPItem>();
    public DbSet<PPMPSectionAllocation> PPMPSectionAllocations
        => Set<PPMPSectionAllocation>();
    public DbSet<PPMPApproval> PPMPApprovals => Set<PPMPApproval>();

    // Audit
    public DbSet<PPMPChangeHistory> PPMPChangeHistories
        => Set<PPMPChangeHistory>();
    public DbSet<PPMPChangeHistoryItem> PPMPChangeHistoryItems
        => Set<PPMPChangeHistoryItem>();

    public DbSet<ProcurementCategoryAuditLog> ProcurementCategoryAuditLogs
    => Set<ProcurementCategoryAuditLog>();

   public DbSet<PPMPChangeHistorySectionAllocation>
    PPMPChangeHistorySectionAllocations
    => Set<PPMPChangeHistorySectionAllocation>();

    // APP
    public DbSet<APP> APPs => Set<APP>();
    public DbSet<APPCategory> APPCategories => Set<APPCategory>();
    public DbSet<APPItem> APPItems => Set<APPItem>();
    public DbSet<APPItemSource> APPItemSources => Set<APPItemSource>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(ApplicationDbContext).Assembly);
}
}
