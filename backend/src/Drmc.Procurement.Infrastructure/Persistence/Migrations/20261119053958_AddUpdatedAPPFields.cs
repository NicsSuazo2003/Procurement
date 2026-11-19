using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAPPFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RevisionNo",
                table: "APPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BidEvaluationCriteria",
                table: "APPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CapitalOutlay",
                table: "APPItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Classification",
                table: "APPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollaboratingEntities",
                table: "APPItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndOfProcurementActivity",
                table: "APPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndUser",
                table: "APPItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralDescription",
                table: "APPItems",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GreenTechnicalSpecifications",
                table: "APPItems",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LifeCycleCost",
                table: "APPItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MOOE",
                table: "APPItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarginalizedSectorConsideration",
                table: "APPItems",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeOfProcurement",
                table: "APPItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MultiYearContractCost",
                table: "APPItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectCode",
                table: "APPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PAPCode",
                table: "APPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PotentialObservers",
                table: "APPItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementAgent",
                table: "APPItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementProjectName",
                table: "APPItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementStrategyTools",
                table: "APPItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProspectiveBidders",
                table: "APPItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "APPItems",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceOfFunds",
                table: "APPItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartOfProcurementActivity",
                table: "APPItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetPRSubmissionDate",
                table: "APPItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevisionNo",
                table: "APPs");

            migrationBuilder.DropColumn(
                name: "BidEvaluationCriteria",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "CapitalOutlay",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "CollaboratingEntities",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "EndOfProcurementActivity",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "EndUser",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "GeneralDescription",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "GreenTechnicalSpecifications",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "LifeCycleCost",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "MOOE",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "MarginalizedSectorConsideration",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "ModeOfProcurement",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "MultiYearContractCost",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "ObjectCode",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "PAPCode",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "PotentialObservers",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "ProcurementAgent",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "ProcurementProjectName",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "ProcurementStrategyTools",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "ProspectiveBidders",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "SourceOfFunds",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "StartOfProcurementActivity",
                table: "APPItems");

            migrationBuilder.DropColumn(
                name: "TargetPRSubmissionDate",
                table: "APPItems");
        }
    }
}
