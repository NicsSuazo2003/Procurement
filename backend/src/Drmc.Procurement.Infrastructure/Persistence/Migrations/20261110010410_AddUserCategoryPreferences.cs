using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCategoryPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCategoryPreferences",
                columns: table => new
                {
                    UserCategoryPreferenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PlanningYearCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategoryPreferences", x => x.UserCategoryPreferenceId);
                    table.ForeignKey(
                        name: "FK_UserCategoryPreferences_PlanningYearCategories_PlanningYearCategoryId",
                        column: x => x.PlanningYearCategoryId,
                        principalTable: "PlanningYearCategories",
                        principalColumn: "PlanningYearCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCategoryPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryPreferences_PlanningYearCategoryId",
                table: "UserCategoryPreferences",
                column: "PlanningYearCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryPreferences_UserId_PlanningYearCategoryId",
                table: "UserCategoryPreferences",
                columns: new[] { "UserId", "PlanningYearCategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCategoryPreferences");
        }
    }
}
