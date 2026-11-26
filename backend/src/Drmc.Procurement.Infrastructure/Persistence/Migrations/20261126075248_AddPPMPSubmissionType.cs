using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPPMPSubmissionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PPMPType",
                table: "PPMPs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Initial");

            migrationBuilder.Sql(
                @"WITH RankedPPMPs AS
                  (
                      SELECT [PPMPId],
                             ROW_NUMBER() OVER (
                                 PARTITION BY [DepartmentId], [PlanningYearId]
                                 ORDER BY [CreatedAt], [PPMPId]) AS [SequenceNumber]
                      FROM [PPMPs]
                  )
                  UPDATE ppmp
                  SET [PPMPType] = CASE
                      WHEN ranked.[SequenceNumber] = 1 THEN N'Initial'
                      ELSE N'Supplemental'
                  END
                  FROM [PPMPs] AS ppmp
                  INNER JOIN RankedPPMPs AS ranked
                      ON ppmp.[PPMPId] = ranked.[PPMPId];");

            migrationBuilder.CreateIndex(
                name: "IX_PPMPs_DepartmentId_PlanningYearId",
                table: "PPMPs",
                columns: new[] { "DepartmentId", "PlanningYearId" },
                unique: true,
                filter: "[PPMPType] = 'Initial'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PPMPs_DepartmentId_PlanningYearId",
                table: "PPMPs");

            migrationBuilder.DropColumn(
                name: "PPMPType",
                table: "PPMPs");
        }
    }
}
