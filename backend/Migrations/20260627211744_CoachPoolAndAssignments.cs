using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayLeague.Api.Migrations
{
    /// <inheritdoc />
    public partial class CoachPoolAndAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coaches_Teams_TeamId",
                table: "Coaches");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "Coaches",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Coaches_TeamId",
                table: "Coaches",
                newName: "IX_Coaches_CreatedById");

            migrationBuilder.CreateTable(
                name: "CoachAssignments",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachAssignments", x => new { x.CoachId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_CoachAssignments_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachAssignments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachAssignments_TeamId",
                table: "CoachAssignments",
                column: "TeamId");

            // Preserve existing assignments: the old TeamId value now lives in the renamed
            // CreatedById column. Copy it into the new join table before we repurpose the column.
            migrationBuilder.Sql(@"
                INSERT INTO ""CoachAssignments"" (""CoachId"", ""TeamId"", ""AssignedAt"")
                SELECT ""Id"", ""CreatedById"", now() FROM ""Coaches"";");

            // Backfill CreatedById with the team's admin (fallback to the seeded admin) so the
            // new FK to Users is satisfiable.
            migrationBuilder.Sql(@"
                UPDATE ""Coaches"" c
                SET ""CreatedById"" = COALESCE(
                    (SELECT tm.""UserId"" FROM ""TeamMembers"" tm
                     WHERE tm.""TeamId"" = (SELECT ca.""TeamId"" FROM ""CoachAssignments"" ca WHERE ca.""CoachId"" = c.""Id"" LIMIT 1)
                       AND tm.""Role"" = 'ADMIN' LIMIT 1),
                    (SELECT ""Id"" FROM ""Users"" WHERE ""Email"" = 'admin@test.com' LIMIT 1));");

            migrationBuilder.AddForeignKey(
                name: "FK_Coaches_Users_CreatedById",
                table: "Coaches",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coaches_Users_CreatedById",
                table: "Coaches");

            migrationBuilder.DropTable(
                name: "CoachAssignments");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Coaches",
                newName: "TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Coaches_CreatedById",
                table: "Coaches",
                newName: "IX_Coaches_TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coaches_Teams_TeamId",
                table: "Coaches",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
