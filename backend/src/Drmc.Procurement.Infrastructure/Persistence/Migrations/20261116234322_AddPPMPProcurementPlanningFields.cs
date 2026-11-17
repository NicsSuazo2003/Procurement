using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPPMPProcurementPlanningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachedSupportingDocuments",
                table: "PPMPItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EndOfProcurementActivity",
                table: "PPMPItems",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedDeliveryImplementationPeriod",
                table: "PPMPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecommendedModeOfProcurement",
                table: "PPMPItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceOfFunds",
                table: "PPMPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartOfProcurementActivity",
                table: "PPMPItems",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousAttachedSupportingDocuments",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousEndOfProcurementActivity",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousExpectedDeliveryImplementationPeriod",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousRecommendedModeOfProcurement",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousSourceOfFunds",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousStartOfProcurementActivity",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedAttachedSupportingDocuments",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedEndOfProcurementActivity",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedExpectedDeliveryImplementationPeriod",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedRecommendedModeOfProcurement",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedSourceOfFunds",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedStartOfProcurementActivity",
                table: "PPMPChangeHistoryItems",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachedSupportingDocuments",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "EndOfProcurementActivity",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryImplementationPeriod",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "RecommendedModeOfProcurement",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "SourceOfFunds",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "StartOfProcurementActivity",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "PreviousAttachedSupportingDocuments",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "PreviousEndOfProcurementActivity",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "PreviousExpectedDeliveryImplementationPeriod",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "PreviousRecommendedModeOfProcurement",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "PreviousSourceOfFunds",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "PreviousStartOfProcurementActivity",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAttachedSupportingDocuments",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedEndOfProcurementActivity",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedExpectedDeliveryImplementationPeriod",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedRecommendedModeOfProcurement",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedSourceOfFunds",
                table: "PPMPChangeHistoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedStartOfProcurementActivity",
                table: "PPMPChangeHistoryItems");
        }
    }
}
