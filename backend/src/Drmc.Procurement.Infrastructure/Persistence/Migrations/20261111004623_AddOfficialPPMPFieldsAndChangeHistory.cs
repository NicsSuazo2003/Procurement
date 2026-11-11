using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialPPMPFieldsAndChangeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneralDescriptionAndObjective",
                table: "PPMPItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PreProcurementConference",
                table: "PPMPItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProjectType",
                table: "PPMPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousGeneralDescriptionAndObjective",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreviousPreProcurementConference",
                table: "PPMPChangeHistoryItems",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousProjectType",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedGeneralDescriptionAndObjective",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UpdatedPreProcurementConference",
                table: "PPMPChangeHistoryItems",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedProjectType",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneralDescriptionAndObjective",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "PreProcurementConference",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "ProjectType",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "PreviousGeneralDescriptionAndObjective",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "PreviousPreProcurementConference",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "PreviousProjectType",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedGeneralDescriptionAndObjective",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedPreProcurementConference",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedProjectType",
                table: "PPMPChangeHistoryItems");
        }
    }
}
