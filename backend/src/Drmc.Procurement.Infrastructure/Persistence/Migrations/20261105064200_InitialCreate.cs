using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "PlanningYears",
                columns: table => new
                {
                    PlanningYearId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningYears", x => x.PlanningYearId);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentSections",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    SectionCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentSections", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_DepartmentSections_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_ProcurementCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProcurementCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanningYearCategories",
                columns: table => new
                {
                    PlanningYearCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanningYearId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningYearCategories", x => x.PlanningYearCategoryId);
                    table.ForeignKey(
                        name: "FK_PlanningYearCategories_PlanningYears_PlanningYearId",
                        column: x => x.PlanningYearId,
                        principalTable: "PlanningYears",
                        principalColumn: "PlanningYearId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanningYearCategories_ProcurementCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProcurementCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APPs",
                columns: table => new
                {
                    APPId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanningYearId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    APPReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APPs", x => x.APPId);
                    table.ForeignKey(
                        name: "FK_APPs_PlanningYears_PlanningYearId",
                        column: x => x.PlanningYearId,
                        principalTable: "PlanningYears",
                        principalColumn: "PlanningYearId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APPs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PPMPs",
                columns: table => new
                {
                    PPMPId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PPMPReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    PlanningYearId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPMPs", x => x.PPMPId);
                    table.ForeignKey(
                        name: "FK_PPMPs_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPMPs_PlanningYears_PlanningYearId",
                        column: x => x.PlanningYearId,
                        principalTable: "PlanningYears",
                        principalColumn: "PlanningYearId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPMPs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementCategoryAuditLogs",
                columns: table => new
                {
                    CategoryAuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PreviousCategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreviousCategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreviousDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviousIsActive = table.Column<bool>(type: "bit", nullable: true),
                    UpdatedCategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedCategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedIsActive = table.Column<bool>(type: "bit", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementCategoryAuditLogs", x => x.CategoryAuditLogId);
                    table.ForeignKey(
                        name: "FK_ProcurementCategoryAuditLogs_ProcurementCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProcurementCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementCategoryAuditLogs_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APPCategories",
                columns: table => new
                {
                    APPCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    APPId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    TotalEstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APPCategories", x => x.APPCategoryId);
                    table.ForeignKey(
                        name: "FK_APPCategories_APPs_APPId",
                        column: x => x.APPId,
                        principalTable: "APPs",
                        principalColumn: "APPId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APPCategories_ProcurementCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProcurementCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PPMPApprovals",
                columns: table => new
                {
                    ApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PPMPId = table.Column<int>(type: "int", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: false),
                    ApprovalType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPMPApprovals", x => x.ApprovalId);
                    table.ForeignKey(
                        name: "FK_PPMPApprovals_PPMPs_PPMPId",
                        column: x => x.PPMPId,
                        principalTable: "PPMPs",
                        principalColumn: "PPMPId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPMPApprovals_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PPMPChangeHistories",
                columns: table => new
                {
                    ChangeHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PPMPId = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PreviousBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPMPChangeHistories", x => x.ChangeHistoryId);
                    table.ForeignKey(
                        name: "FK_PPMPChangeHistories_PPMPs_PPMPId",
                        column: x => x.PPMPId,
                        principalTable: "PPMPs",
                        principalColumn: "PPMPId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPMPChangeHistories_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PPMPItems",
                columns: table => new
                {
                    PPMPItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PPMPId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPMPItems", x => x.PPMPItemId);
                    table.ForeignKey(
                        name: "FK_PPMPItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPMPItems_PPMPs_PPMPId",
                        column: x => x.PPMPId,
                        principalTable: "PPMPs",
                        principalColumn: "PPMPId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPMPItems_ProcurementCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProcurementCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APPItems",
                columns: table => new
                {
                    APPItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    APPCategoryId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedUnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APPItems", x => x.APPItemId);
                    table.ForeignKey(
                        name: "FK_APPItems_APPCategories_APPCategoryId",
                        column: x => x.APPCategoryId,
                        principalTable: "APPCategories",
                        principalColumn: "APPCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APPItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PPMPChangeHistoryItems",
                columns: table => new
                {
                    ChangeHistoryItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangeHistoryId = table.Column<int>(type: "int", nullable: false),
                    PreviousItemId = table.Column<int>(type: "int", nullable: true),
                    PreviousItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreviousDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviousUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreviousQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PreviousUnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PreviousEstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UpdatedItemId = table.Column<int>(type: "int", nullable: true),
                    UpdatedItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UpdatedUnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UpdatedEstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPMPChangeHistoryItems", x => x.ChangeHistoryItemId);
                    table.ForeignKey(
                        name: "FK_PPMPChangeHistoryItems_PPMPChangeHistories_ChangeHistoryId",
                        column: x => x.ChangeHistoryId,
                        principalTable: "PPMPChangeHistories",
                        principalColumn: "ChangeHistoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PPMPSectionAllocations",
                columns: table => new
                {
                    AllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PPMPItemId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPMPSectionAllocations", x => x.AllocationId);
                    table.ForeignKey(
                        name: "FK_PPMPSectionAllocations_DepartmentSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "DepartmentSections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPMPSectionAllocations_PPMPItems_PPMPItemId",
                        column: x => x.PPMPItemId,
                        principalTable: "PPMPItems",
                        principalColumn: "PPMPItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APPItemSources",
                columns: table => new
                {
                    APPItemSourceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    APPItemId = table.Column<int>(type: "int", nullable: false),
                    PPMPItemId = table.Column<int>(type: "int", nullable: false),
                    ContributedQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ContributedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APPItemSources", x => x.APPItemSourceId);
                    table.ForeignKey(
                        name: "FK_APPItemSources_APPItems_APPItemId",
                        column: x => x.APPItemId,
                        principalTable: "APPItems",
                        principalColumn: "APPItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APPItemSources_PPMPItems_PPMPItemId",
                        column: x => x.PPMPItemId,
                        principalTable: "PPMPItems",
                        principalColumn: "PPMPItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APPCategories_APPId_CategoryId",
                table: "APPCategories",
                columns: new[] { "APPId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APPCategories_CategoryId",
                table: "APPCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_APPItems_APPCategoryId",
                table: "APPItems",
                column: "APPCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_APPItems_ItemId",
                table: "APPItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_APPItemSources_APPItemId_PPMPItemId",
                table: "APPItemSources",
                columns: new[] { "APPItemId", "PPMPItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APPItemSources_PPMPItemId",
                table: "APPItemSources",
                column: "PPMPItemId");

            migrationBuilder.CreateIndex(
                name: "IX_APPs_APPReference",
                table: "APPs",
                column: "APPReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APPs_CreatedBy",
                table: "APPs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_APPs_PlanningYearId",
                table: "APPs",
                column: "PlanningYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentSections_DepartmentId_SectionCode",
                table: "DepartmentSections",
                columns: new[] { "DepartmentId", "SectionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoryId_ItemCode",
                table: "Items",
                columns: new[] { "CategoryId", "ItemCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanningYearCategories_CategoryId",
                table: "PlanningYearCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningYearCategories_PlanningYearId_CategoryId",
                table: "PlanningYearCategories",
                columns: new[] { "PlanningYearId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanningYears_Year",
                table: "PlanningYears",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPMPApprovals_ApprovedBy",
                table: "PPMPApprovals",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPApprovals_PPMPId",
                table: "PPMPApprovals",
                column: "PPMPId");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPChangeHistories_ChangedBy",
                table: "PPMPChangeHistories",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPChangeHistories_PPMPId",
                table: "PPMPChangeHistories",
                column: "PPMPId");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPChangeHistoryItems_ChangeHistoryId",
                table: "PPMPChangeHistoryItems",
                column: "ChangeHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPItems_CategoryId",
                table: "PPMPItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPItems_ItemId",
                table: "PPMPItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPItems_PPMPId",
                table: "PPMPItems",
                column: "PPMPId");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPs_CreatedBy",
                table: "PPMPs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPs_DepartmentId",
                table: "PPMPs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPs_PlanningYearId",
                table: "PPMPs",
                column: "PlanningYearId");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPs_PPMPReference",
                table: "PPMPs",
                column: "PPMPReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPMPSectionAllocations_PPMPItemId_SectionId",
                table: "PPMPSectionAllocations",
                columns: new[] { "PPMPItemId", "SectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPMPSectionAllocations_SectionId",
                table: "PPMPSectionAllocations",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementCategories_CategoryCode",
                table: "ProcurementCategories",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementCategoryAuditLogs_CategoryId_ChangedAt",
                table: "ProcurementCategoryAuditLogs",
                columns: new[] { "CategoryId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementCategoryAuditLogs_ChangedByUserId",
                table: "ProcurementCategoryAuditLogs",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeNumber",
                table: "Users",
                column: "EmployeeNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APPItemSources");

            migrationBuilder.DropTable(
                name: "PlanningYearCategories");

            migrationBuilder.DropTable(
                name: "PPMPApprovals");

            migrationBuilder.DropTable(
                name: "PPMPChangeHistoryItems");

            migrationBuilder.DropTable(
                name: "PPMPSectionAllocations");

            migrationBuilder.DropTable(
                name: "ProcurementCategoryAuditLogs");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "APPItems");

            migrationBuilder.DropTable(
                name: "PPMPChangeHistories");

            migrationBuilder.DropTable(
                name: "DepartmentSections");

            migrationBuilder.DropTable(
                name: "PPMPItems");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "APPCategories");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "PPMPs");

            migrationBuilder.DropTable(
                name: "APPs");

            migrationBuilder.DropTable(
                name: "ProcurementCategories");

            migrationBuilder.DropTable(
                name: "PlanningYears");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
