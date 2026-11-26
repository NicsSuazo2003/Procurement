using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAPPApprovalStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "APPs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreparedAt",
                table: "APPs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreparedByUserId",
                table: "APPs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecommendedAt",
                table: "APPs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecommendedByUserId",
                table: "APPs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_APPs_ApprovedByUserId",
                table: "APPs",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_APPs_PreparedByUserId",
                table: "APPs",
                column: "PreparedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_APPs_RecommendedByUserId",
                table: "APPs",
                column: "RecommendedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_APPs_Users_ApprovedByUserId",
                table: "APPs",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_APPs_Users_PreparedByUserId",
                table: "APPs",
                column: "PreparedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_APPs_Users_RecommendedByUserId",
                table: "APPs",
                column: "RecommendedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Bids and Awards Committee Secretariat')
                  INSERT INTO [Roles] ([RoleName], [Description])
                  VALUES (N'Bids and Awards Committee Secretariat', N'Prepares the Annual Procurement Plan as Supervising Administrative Officer.');

                  IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Bids and Awards Committee Chairperson')
                  INSERT INTO [Roles] ([RoleName], [Description])
                  VALUES (N'Bids and Awards Committee Chairperson', N'Recommends the Annual Procurement Plan as Chief Administrative Officer.');

                  IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Head of the Procuring Entity')
                  INSERT INTO [Roles] ([RoleName], [Description])
                  VALUES (N'Head of the Procuring Entity', N'Approves the Annual Procurement Plan as Medical Center Chief II.');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_APPs_Users_ApprovedByUserId",
                table: "APPs");

            migrationBuilder.DropForeignKey(
                name: "FK_APPs_Users_PreparedByUserId",
                table: "APPs");

            migrationBuilder.DropForeignKey(
                name: "FK_APPs_Users_RecommendedByUserId",
                table: "APPs");

            migrationBuilder.DropIndex(
                name: "IX_APPs_ApprovedByUserId",
                table: "APPs");

            migrationBuilder.DropIndex(
                name: "IX_APPs_PreparedByUserId",
                table: "APPs");

            migrationBuilder.DropIndex(
                name: "IX_APPs_RecommendedByUserId",
                table: "APPs");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "APPs");

            migrationBuilder.DropColumn(
                name: "PreparedAt",
                table: "APPs");

            migrationBuilder.DropColumn(
                name: "PreparedByUserId",
                table: "APPs");

            migrationBuilder.DropColumn(
                name: "RecommendedAt",
                table: "APPs");

            migrationBuilder.DropColumn(
                name: "RecommendedByUserId",
                table: "APPs");

            migrationBuilder.Sql(
                @"DELETE role
                  FROM [Roles] AS role
                  WHERE role.[RoleName] IN (
                      N'Bids and Awards Committee Secretariat',
                      N'Bids and Awards Committee Chairperson',
                      N'Head of the Procuring Entity')
                    AND NOT EXISTS (
                      SELECT 1
                      FROM [UserRoles] AS userRole
                      WHERE userRole.[RoleId] = role.[RoleId]);");
        }
    }
}
