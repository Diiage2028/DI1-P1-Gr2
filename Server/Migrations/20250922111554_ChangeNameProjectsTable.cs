using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameProjectsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_games_GameId",
                table: "project");

            migrationBuilder.DropPrimaryKey(
                name: "PK_project",
                table: "project");

            migrationBuilder.RenameTable(
                name: "project",
                newName: "projects");

            migrationBuilder.RenameIndex(
                name: "IX_project_GameId",
                table: "projects",
                newName: "IX_projects_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_projects",
                table: "projects",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_games_GameId",
                table: "projects",
                column: "GameId",
                principalTable: "games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_games_GameId",
                table: "projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_projects",
                table: "projects");

            migrationBuilder.RenameTable(
                name: "projects",
                newName: "project");

            migrationBuilder.RenameIndex(
                name: "IX_projects_GameId",
                table: "project",
                newName: "IX_project_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_project",
                table: "project",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_project_games_GameId",
                table: "project",
                column: "GameId",
                principalTable: "games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
