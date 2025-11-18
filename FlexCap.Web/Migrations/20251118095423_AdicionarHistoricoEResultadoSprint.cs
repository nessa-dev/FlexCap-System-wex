using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexCap.Web.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarHistoricoEResultadoSprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sprints",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SprintResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SprintId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Goal = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletionPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    Blockers = table.Column<string>(type: "TEXT", nullable: false),
                    WorkedWell = table.Column<string>(type: "TEXT", nullable: false),
                    Improvement = table.Column<string>(type: "TEXT", nullable: false),
                    ActualFinishDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SprintResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SprintResults");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sprints");
        }
    }
}
