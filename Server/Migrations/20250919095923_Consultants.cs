using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Consultants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultant_games_GameId",
                table: "Consultant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Consultant",
                table: "Consultant");

            migrationBuilder.RenameTable(
                name: "Consultant",
                newName: "consultants");

            migrationBuilder.RenameIndex(
                name: "IX_Consultant_GameId",
                table: "consultants",
                newName: "IX_consultants_GameId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "consultants",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "consultants",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_consultants",
                table: "consultants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_consultants_games_GameId",
                table: "consultants",
                column: "GameId",
                principalTable: "games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_consultants_games_GameId",
                table: "consultants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_consultants",
                table: "consultants");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "consultants");

            migrationBuilder.RenameTable(
                name: "consultants",
                newName: "Consultant");

            migrationBuilder.RenameIndex(
                name: "IX_consultants_GameId",
                table: "Consultant",
                newName: "IX_Consultant_GameId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Consultant",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Consultant",
                table: "Consultant",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultant_games_GameId",
                table: "Consultant",
                column: "GameId",
                principalTable: "games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
