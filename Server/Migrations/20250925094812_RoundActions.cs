using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Server.Models;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class RoundActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actions",
                table: "rounds");

            migrationBuilder.AddColumn<int>(
                name: "RoundId",
                table: "rounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "round_actions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoundId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    FireAnEmployeeRoundAction_Payload = table.Column<FireAnEmployeeRoundAction.FireAnEmployeePayload>(type: "jsonb", nullable: true),
                    Payload = table.Column<ParticipateInProjectRoundAction.ParticipateInProjectPayload>(type: "jsonb", nullable: true),
                    SendEmployeeForTrainingRoundAction_Payload = table.Column<SendEmployeeForTrainingRoundAction.SendEmployeeForTrainingPayload>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_round_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_round_actions_rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_round_actions_RoundId",
                table: "round_actions",
                column: "RoundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "round_actions");

            migrationBuilder.DropColumn(
                name: "RoundId",
                table: "rounds");

            migrationBuilder.AddColumn<string>(
                name: "Actions",
                table: "rounds",
                type: "jsonb",
                nullable: true);
        }
    }
}
