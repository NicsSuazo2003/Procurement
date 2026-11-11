IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Departments] (
    [DepartmentId] int NOT NULL IDENTITY,
    [DepartmentCode] nvarchar(30) NOT NULL,
    [DepartmentName] nvarchar(150) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Departments] PRIMARY KEY ([DepartmentId])
);

CREATE TABLE [PlanningYears] (
    [PlanningYearId] int NOT NULL IDENTITY,
    [Year] int NOT NULL,
    [Status] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_PlanningYears] PRIMARY KEY ([PlanningYearId])
);

CREATE TABLE [ProcurementCategories] (
    [CategoryId] int NOT NULL IDENTITY,
    [CategoryCode] nvarchar(50) NOT NULL,
    [CategoryName] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_ProcurementCategories] PRIMARY KEY ([CategoryId])
);

CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId])
);

CREATE TABLE [DepartmentSections] (
    [SectionId] int NOT NULL IDENTITY,
    [DepartmentId] int NOT NULL,
    [SectionCode] nvarchar(30) NOT NULL,
    [SectionName] nvarchar(150) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_DepartmentSections] PRIMARY KEY ([SectionId]),
    CONSTRAINT [FK_DepartmentSections_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]) ON DELETE NO ACTION
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [DepartmentId] int NULL,
    [EmployeeNumber] nvarchar(30) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]) ON DELETE NO ACTION
);

CREATE TABLE [Items] (
    [ItemId] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [ItemCode] nvarchar(100) NOT NULL,
    [ItemName] nvarchar(300) NOT NULL,
    [ItemType] nvarchar(100) NULL,
    [Unit] nvarchar(50) NOT NULL,
    [Specifications] nvarchar(1000) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Items] PRIMARY KEY ([ItemId]),
    CONSTRAINT [FK_Items_ProcurementCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProcurementCategories] ([CategoryId]) ON DELETE NO ACTION
);

CREATE TABLE [PlanningYearCategories] (
    [PlanningYearCategoryId] int NOT NULL IDENTITY,
    [PlanningYearId] int NOT NULL,
    [CategoryId] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_PlanningYearCategories] PRIMARY KEY ([PlanningYearCategoryId]),
    CONSTRAINT [FK_PlanningYearCategories_PlanningYears_PlanningYearId] FOREIGN KEY ([PlanningYearId]) REFERENCES [PlanningYears] ([PlanningYearId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanningYearCategories_ProcurementCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProcurementCategories] ([CategoryId]) ON DELETE NO ACTION
);

CREATE TABLE [APPs] (
    [APPId] int NOT NULL IDENTITY,
    [PlanningYearId] int NOT NULL,
    [CreatedBy] int NOT NULL,
    [APPReference] nvarchar(50) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ApprovedAt] datetime2 NULL,
    CONSTRAINT [PK_APPs] PRIMARY KEY ([APPId]),
    CONSTRAINT [FK_APPs_PlanningYears_PlanningYearId] FOREIGN KEY ([PlanningYearId]) REFERENCES [PlanningYears] ([PlanningYearId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_APPs_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [PPMPs] (
    [PPMPId] int NOT NULL IDENTITY,
    [PPMPReference] nvarchar(50) NOT NULL,
    [DepartmentId] int NOT NULL,
    [PlanningYearId] int NOT NULL,
    [CreatedBy] int NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [EstimatedBudget] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [SubmittedAt] datetime2 NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_PPMPs] PRIMARY KEY ([PPMPId]),
    CONSTRAINT [FK_PPMPs_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PPMPs_PlanningYears_PlanningYearId] FOREIGN KEY ([PlanningYearId]) REFERENCES [PlanningYears] ([PlanningYearId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PPMPs_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [ProcurementCategoryAuditLogs] (
    [CategoryAuditLogId] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [ChangedByUserId] int NOT NULL,
    [Action] nvarchar(50) NOT NULL,
    [Reason] nvarchar(1000) NULL,
    [PreviousCategoryCode] nvarchar(50) NULL,
    [PreviousCategoryName] nvarchar(200) NULL,
    [PreviousDescription] nvarchar(500) NULL,
    [PreviousIsActive] bit NULL,
    [UpdatedCategoryCode] nvarchar(50) NULL,
    [UpdatedCategoryName] nvarchar(200) NULL,
    [UpdatedDescription] nvarchar(500) NULL,
    [UpdatedIsActive] bit NULL,
    [ChangedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ProcurementCategoryAuditLogs] PRIMARY KEY ([CategoryAuditLogId]),
    CONSTRAINT [FK_ProcurementCategoryAuditLogs_ProcurementCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProcurementCategories] ([CategoryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProcurementCategoryAuditLogs_Users_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [UserRoles] (
    [UserRoleId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [APPCategories] (
    [APPCategoryId] int NOT NULL IDENTITY,
    [APPId] int NOT NULL,
    [CategoryId] int NOT NULL,
    [TotalEstimatedBudget] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_APPCategories] PRIMARY KEY ([APPCategoryId]),
    CONSTRAINT [FK_APPCategories_APPs_APPId] FOREIGN KEY ([APPId]) REFERENCES [APPs] ([APPId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_APPCategories_ProcurementCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProcurementCategories] ([CategoryId]) ON DELETE NO ACTION
);

CREATE TABLE [PPMPApprovals] (
    [ApprovalId] int NOT NULL IDENTITY,
    [PPMPId] int NOT NULL,
    [ApprovedBy] int NOT NULL,
    [ApprovalType] nvarchar(100) NOT NULL,
    [ApprovedAt] datetime2 NOT NULL,
    [Remarks] nvarchar(1000) NULL,
    CONSTRAINT [PK_PPMPApprovals] PRIMARY KEY ([ApprovalId]),
    CONSTRAINT [FK_PPMPApprovals_PPMPs_PPMPId] FOREIGN KEY ([PPMPId]) REFERENCES [PPMPs] ([PPMPId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PPMPApprovals_Users_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [PPMPChangeHistories] (
    [ChangeHistoryId] int NOT NULL IDENTITY,
    [PPMPId] int NOT NULL,
    [ChangedBy] int NOT NULL,
    [Reason] nvarchar(1000) NOT NULL,
    [PreviousBudget] decimal(18,2) NOT NULL,
    [UpdatedBudget] decimal(18,2) NOT NULL,
    [ChangedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PPMPChangeHistories] PRIMARY KEY ([ChangeHistoryId]),
    CONSTRAINT [FK_PPMPChangeHistories_PPMPs_PPMPId] FOREIGN KEY ([PPMPId]) REFERENCES [PPMPs] ([PPMPId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PPMPChangeHistories_Users_ChangedBy] FOREIGN KEY ([ChangedBy]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [PPMPItems] (
    [PPMPItemId] int NOT NULL IDENTITY,
    [PPMPId] int NOT NULL,
    [ItemId] int NULL,
    [CategoryId] int NOT NULL,
    [ItemCode] nvarchar(100) NOT NULL,
    [ItemDescription] nvarchar(500) NOT NULL,
    [ItemType] nvarchar(100) NULL,
    [Unit] nvarchar(50) NOT NULL,
    [Specifications] nvarchar(1000) NULL,
    [Quantity] decimal(18,2) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [EstimatedBudget] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_PPMPItems] PRIMARY KEY ([PPMPItemId]),
    CONSTRAINT [FK_PPMPItems_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([ItemId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PPMPItems_PPMPs_PPMPId] FOREIGN KEY ([PPMPId]) REFERENCES [PPMPs] ([PPMPId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PPMPItems_ProcurementCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProcurementCategories] ([CategoryId]) ON DELETE NO ACTION
);

CREATE TABLE [APPItems] (
    [APPItemId] int NOT NULL IDENTITY,
    [APPCategoryId] int NOT NULL,
    [ItemId] int NULL,
    [ItemCode] nvarchar(100) NOT NULL,
    [ItemDescription] nvarchar(500) NOT NULL,
    [ItemType] nvarchar(100) NULL,
    [Unit] nvarchar(50) NOT NULL,
    [Specifications] nvarchar(1000) NULL,
    [TotalQuantity] decimal(18,2) NOT NULL,
    [EstimatedUnitPrice] decimal(18,2) NOT NULL,
    [EstimatedBudget] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_APPItems] PRIMARY KEY ([APPItemId]),
    CONSTRAINT [FK_APPItems_APPCategories_APPCategoryId] FOREIGN KEY ([APPCategoryId]) REFERENCES [APPCategories] ([APPCategoryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_APPItems_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([ItemId]) ON DELETE NO ACTION
);

CREATE TABLE [PPMPChangeHistoryItems] (
    [ChangeHistoryItemId] int NOT NULL IDENTITY,
    [ChangeHistoryId] int NOT NULL,
    [PreviousItemId] int NULL,
    [PreviousItemCode] nvarchar(100) NULL,
    [PreviousDescription] nvarchar(500) NULL,
    [PreviousUnit] nvarchar(50) NULL,
    [PreviousQuantity] decimal(18,2) NULL,
    [PreviousUnitPrice] decimal(18,2) NULL,
    [PreviousEstimatedBudget] decimal(18,2) NULL,
    [UpdatedItemId] int NULL,
    [UpdatedItemCode] nvarchar(100) NULL,
    [UpdatedDescription] nvarchar(500) NULL,
    [UpdatedUnit] nvarchar(50) NULL,
    [UpdatedQuantity] decimal(18,2) NULL,
    [UpdatedUnitPrice] decimal(18,2) NULL,
    [UpdatedEstimatedBudget] decimal(18,2) NULL,
    CONSTRAINT [PK_PPMPChangeHistoryItems] PRIMARY KEY ([ChangeHistoryItemId]),
    CONSTRAINT [FK_PPMPChangeHistoryItems_PPMPChangeHistories_ChangeHistoryId] FOREIGN KEY ([ChangeHistoryId]) REFERENCES [PPMPChangeHistories] ([ChangeHistoryId]) ON DELETE NO ACTION
);

CREATE TABLE [PPMPSectionAllocations] (
    [AllocationId] int NOT NULL IDENTITY,
    [PPMPItemId] int NOT NULL,
    [SectionId] int NOT NULL,
    [Quantity] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_PPMPSectionAllocations] PRIMARY KEY ([AllocationId]),
    CONSTRAINT [FK_PPMPSectionAllocations_DepartmentSections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [DepartmentSections] ([SectionId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PPMPSectionAllocations_PPMPItems_PPMPItemId] FOREIGN KEY ([PPMPItemId]) REFERENCES [PPMPItems] ([PPMPItemId]) ON DELETE NO ACTION
);

CREATE TABLE [APPItemSources] (
    [APPItemSourceId] int NOT NULL IDENTITY,
    [APPItemId] int NOT NULL,
    [PPMPItemId] int NOT NULL,
    [ContributedQuantity] decimal(18,2) NOT NULL,
    [ContributedBudget] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_APPItemSources] PRIMARY KEY ([APPItemSourceId]),
    CONSTRAINT [FK_APPItemSources_APPItems_APPItemId] FOREIGN KEY ([APPItemId]) REFERENCES [APPItems] ([APPItemId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_APPItemSources_PPMPItems_PPMPItemId] FOREIGN KEY ([PPMPItemId]) REFERENCES [PPMPItems] ([PPMPItemId]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_APPCategories_APPId_CategoryId] ON [APPCategories] ([APPId], [CategoryId]);

CREATE INDEX [IX_APPCategories_CategoryId] ON [APPCategories] ([CategoryId]);

CREATE INDEX [IX_APPItems_APPCategoryId] ON [APPItems] ([APPCategoryId]);

CREATE INDEX [IX_APPItems_ItemId] ON [APPItems] ([ItemId]);

CREATE UNIQUE INDEX [IX_APPItemSources_APPItemId_PPMPItemId] ON [APPItemSources] ([APPItemId], [PPMPItemId]);

CREATE INDEX [IX_APPItemSources_PPMPItemId] ON [APPItemSources] ([PPMPItemId]);

CREATE UNIQUE INDEX [IX_APPs_APPReference] ON [APPs] ([APPReference]);

CREATE INDEX [IX_APPs_CreatedBy] ON [APPs] ([CreatedBy]);

CREATE INDEX [IX_APPs_PlanningYearId] ON [APPs] ([PlanningYearId]);

CREATE UNIQUE INDEX [IX_Departments_DepartmentCode] ON [Departments] ([DepartmentCode]);

CREATE UNIQUE INDEX [IX_DepartmentSections_DepartmentId_SectionCode] ON [DepartmentSections] ([DepartmentId], [SectionCode]);

CREATE UNIQUE INDEX [IX_Items_CategoryId_ItemCode] ON [Items] ([CategoryId], [ItemCode]);

CREATE INDEX [IX_PlanningYearCategories_CategoryId] ON [PlanningYearCategories] ([CategoryId]);

CREATE UNIQUE INDEX [IX_PlanningYearCategories_PlanningYearId_CategoryId] ON [PlanningYearCategories] ([PlanningYearId], [CategoryId]);

CREATE UNIQUE INDEX [IX_PlanningYears_Year] ON [PlanningYears] ([Year]);

CREATE INDEX [IX_PPMPApprovals_ApprovedBy] ON [PPMPApprovals] ([ApprovedBy]);

CREATE INDEX [IX_PPMPApprovals_PPMPId] ON [PPMPApprovals] ([PPMPId]);

CREATE INDEX [IX_PPMPChangeHistories_ChangedBy] ON [PPMPChangeHistories] ([ChangedBy]);

CREATE INDEX [IX_PPMPChangeHistories_PPMPId] ON [PPMPChangeHistories] ([PPMPId]);

CREATE INDEX [IX_PPMPChangeHistoryItems_ChangeHistoryId] ON [PPMPChangeHistoryItems] ([ChangeHistoryId]);

CREATE INDEX [IX_PPMPItems_CategoryId] ON [PPMPItems] ([CategoryId]);

CREATE INDEX [IX_PPMPItems_ItemId] ON [PPMPItems] ([ItemId]);

CREATE INDEX [IX_PPMPItems_PPMPId] ON [PPMPItems] ([PPMPId]);

CREATE INDEX [IX_PPMPs_CreatedBy] ON [PPMPs] ([CreatedBy]);

CREATE INDEX [IX_PPMPs_DepartmentId] ON [PPMPs] ([DepartmentId]);

CREATE INDEX [IX_PPMPs_PlanningYearId] ON [PPMPs] ([PlanningYearId]);

CREATE UNIQUE INDEX [IX_PPMPs_PPMPReference] ON [PPMPs] ([PPMPReference]);

CREATE UNIQUE INDEX [IX_PPMPSectionAllocations_PPMPItemId_SectionId] ON [PPMPSectionAllocations] ([PPMPItemId], [SectionId]);

CREATE INDEX [IX_PPMPSectionAllocations_SectionId] ON [PPMPSectionAllocations] ([SectionId]);

CREATE UNIQUE INDEX [IX_ProcurementCategories_CategoryCode] ON [ProcurementCategories] ([CategoryCode]);

CREATE INDEX [IX_ProcurementCategoryAuditLogs_CategoryId_ChangedAt] ON [ProcurementCategoryAuditLogs] ([CategoryId], [ChangedAt]);

CREATE INDEX [IX_ProcurementCategoryAuditLogs_ChangedByUserId] ON [ProcurementCategoryAuditLogs] ([ChangedByUserId]);

CREATE UNIQUE INDEX [IX_Roles_RoleName] ON [Roles] ([RoleName]);

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);

CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles] ([UserId], [RoleId]);

CREATE INDEX [IX_Users_DepartmentId] ON [Users] ([DepartmentId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE UNIQUE INDEX [IX_Users_EmployeeNumber] ON [Users] ([EmployeeNumber]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261105064200_InitialCreate', N'10.0.3');

COMMIT;
GO

