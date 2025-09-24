using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Training : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.CreateTable(
                name: "trainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    Cost = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    NbRound = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainings_skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "in_trainings",
                columns: table => new
                {
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    StartRoundId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_in_trainings", x => new { x.EmployeeId, x.TrainingId, x.StartRoundId });
                    table.ForeignKey(
                        name: "FK_in_trainings_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_in_trainings_rounds_StartRoundId",
                        column: x => x.StartRoundId,
                        principalTable: "rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_in_trainings_trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "trainings",
                columns: new[] { "Id", "Cost", "Name", "NbRound", "SkillId" },
                values: new object[,]
                {
                    { 1, 5000, "Formation HTML", 2, 1 },
                    { 2, 5000, "Formation CSS", 2, 2 },
                    { 3, 5000, "Formation JavaScript", 2, 3 },
                    { 4, 5000, "Formation TypeScript", 2, 4 },
                    { 5, 5000, "Formation React", 2, 5 },
                    { 6, 5000, "Formation Angular", 2, 6 },
                    { 7, 5000, "Formation Vue.js", 2, 7 },
                    { 8, 5000, "Formation Node.js", 2, 8 },
                    { 9, 5000, "Formation Express.js", 2, 9 },
                    { 10, 5000, "Formation ASP.NET Core", 2, 10 },
                    { 11, 5000, "Formation Ruby on Rails", 2, 11 },
                    { 12, 5000, "Formation Django", 2, 12 },
                    { 13, 5000, "Formation Flask", 2, 13 },
                    { 14, 5000, "Formation PHP", 2, 14 },
                    { 15, 5000, "Formation Laravel", 2, 15 },
                    { 16, 5000, "Formation Spring Boot", 2, 16 },
                    { 17, 5000, "Formation SQL", 2, 17 },
                    { 18, 5000, "Formation NoSQL", 2, 18 },
                    { 19, 5000, "Formation GraphQL", 2, 19 },
                    { 20, 5000, "Formation REST APIs", 2, 20 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_in_trainings_StartRoundId",
                table: "in_trainings",
                column: "StartRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_in_trainings_TrainingId",
                table: "in_trainings",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_trainings_SkillId",
                table: "trainings",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "in_trainings");

            migrationBuilder.DropTable(
                name: "trainings");

                
        }
    }
}
