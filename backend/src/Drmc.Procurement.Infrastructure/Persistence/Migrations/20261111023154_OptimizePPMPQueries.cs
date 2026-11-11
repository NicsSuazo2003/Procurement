using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizePPMPQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PPMPs_DepartmentId",
                table: "PPMPs");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPs_DepartmentId_Status",
                table: "PPMPs",
                columns: new[] { "DepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PPMPs_Status",
                table: "PPMPs",
                column: "Status",
                filter: "[Status] IN ('For Unit Head Approval', 'Approved by Unit Head')");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPItems_PPMPId_CategoryId",
                table: "PPMPItems",
                columns: new[] { "PPMPId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoryId_IsActive",
                table: "Items",
                columns: new[] { "CategoryId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PPMPs_DepartmentId_Status",
                table: "PPMPs");

            migrationBuilder.DropIndex(
                name: "IX_PPMPs_Status",
                table: "PPMPs");

            migrationBuilder.DropIndex(
                name: "IX_PPMPItems_PPMPId_CategoryId",
                table: "PPMPItems");

            migrationBuilder.DropIndex(
                name: "IX_Items_CategoryId_IsActive",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPs_DepartmentId",
                table: "PPMPs",
                column: "DepartmentId");
        }
    }
}
