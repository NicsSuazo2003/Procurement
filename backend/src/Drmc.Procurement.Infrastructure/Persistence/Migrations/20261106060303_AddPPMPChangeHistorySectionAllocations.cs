using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPPMPChangeHistorySectionAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PPMPChangeHistorySectionAllocations",
                columns: table => new
                {
                    ChangeHistorySectionAllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangeHistoryItemId = table.Column<int>(type: "int", nullable: false),
                    PreviousSectionId = table.Column<int>(type: "int", nullable: true),
                    PreviousSectionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreviousSectionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PreviousQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UpdatedSectionId = table.Column<int>(type: "int", nullable: true),
                    UpdatedSectionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedSectionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPMPChangeHistorySectionAllocations", x => x.ChangeHistorySectionAllocationId);
                    table.ForeignKey(
                        name: "FK_PPMPChangeHistorySectionAllocations_PPMPChangeHistoryItems_ChangeHistoryItemId",
                        column: x => x.ChangeHistoryItemId,
                        principalTable: "PPMPChangeHistoryItems",
                        principalColumn: "ChangeHistoryItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PPMPChangeHistorySectionAllocations_ChangeHistoryItemId",
                table: "PPMPChangeHistorySectionAllocations",
                column: "ChangeHistoryItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PPMPChangeHistorySectionAllocations");
        }
    }
}
