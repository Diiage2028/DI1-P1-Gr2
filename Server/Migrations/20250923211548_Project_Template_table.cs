using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Project_Template_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Reward",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "projects");

            migrationBuilder.RenameColumn(
                name: "Rounds",
                table: "projects",
                newName: "TemplateId");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "project_templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    Rounds = table.Column<int>(type: "integer", nullable: false),
                    Reward = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_templates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "project_templates",
                columns: new[] { "Id", "Name", "Reward", "Rounds" },
                values: new object[,]
                {
                    { 1, "Web app", 30000, 6 },
                    { 2, "Mini game job hunting turn by turn", 45000, 2 },
                    { 3, "E-commerce platform", 80000, 4 },
                    { 4, "Mobile banking app", 70000, 1 },
                    { 5, "Inventory management system", 65000, 5 },
                    { 6, "Social networking site", 90000, 3 },
                    { 7, "Online booking system", 55000, 6 },
                    { 8, "Chatbot assistant", 30000, 2 },
                    { 9, "Learning management system", 60000, 1 },
                    { 10, "IoT smart home dashboard", 75000, 4 },
                    { 11, "Data visualization tool", 40000, 5 },
                    { 12, "Cloud file storage service", 50000, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_projects_CompanyId",
                table: "projects",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_TemplateId",
                table: "projects",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_companies_CompanyId",
                table: "projects",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_project_templates_TemplateId",
                table: "projects",
                column: "TemplateId",
                principalTable: "project_templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_companies_CompanyId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_project_templates_TemplateId",
                table: "projects");

            migrationBuilder.DropTable(
                name: "project_templates");

            migrationBuilder.DropIndex(
                name: "IX_projects_CompanyId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_TemplateId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "projects");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "projects",
                newName: "Rounds");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "projects",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Reward",
                table: "projects",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "projects",
                type: "jsonb",
                nullable: true);
        }
    }
}
