using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningYearItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanningYearItems",
                columns: table => new
                {
                    PlanningYearItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanningYearCategoryId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ABCPerUnit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningYearItems", x => x.PlanningYearItemId);
                    table.ForeignKey(
                        name: "FK_PlanningYearItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanningYearItems_PlanningYearCategories_PlanningYearCategoryId",
                        column: x => x.PlanningYearCategoryId,
                        principalTable: "PlanningYearCategories",
                        principalColumn: "PlanningYearCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningYearItems_ItemId",
                table: "PlanningYearItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningYearItems_PlanningYearCategoryId_ItemId",
                table: "PlanningYearItems",
                columns: new[] { "PlanningYearCategoryId", "ItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningYearItems");
        }
    }
}
