using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drmc.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamsAndPPMPItemTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Team Contributor') INSERT INTO [Roles] ([RoleName], [Description]) VALUES (N'Team Contributor', N'Can contribute procurement items for assigned teams.');");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Unit PPMP Coordinator') INSERT INTO [Roles] ([RoleName], [Description]) VALUES (N'Unit PPMP Coordinator', N'Can consolidate and submit the unit PPMP.');");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "PPMPItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    TeamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Teams_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamMemberships",
                columns: table => new
                {
                    TeamMembershipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMemberships", x => x.TeamMembershipId);
                    table.ForeignKey(
                        name: "FK_TeamMemberships_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PPMPItems_TeamId",
                table: "PPMPItems",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_TeamId_UserId",
                table: "TeamMemberships",
                columns: new[] { "TeamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_UserId",
                table: "TeamMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_DepartmentId_TeamCode",
                table: "Teams",
                columns: new[] { "DepartmentId", "TeamCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PPMPItems_Teams_TeamId",
                table: "PPMPItems",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [Roles] WHERE [RoleName] IN (N'Team Contributor', N'Unit PPMP Coordinator');");

            migrationBuilder.DropForeignKey(
                name: "FK_PPMPItems_Teams_TeamId",
                table: "PPMPItems");

            migrationBuilder.DropTable(
                name: "TeamMemberships");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_PPMPItems_TeamId",
                table: "PPMPItems");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "PPMPItems");
        }
    }
}
