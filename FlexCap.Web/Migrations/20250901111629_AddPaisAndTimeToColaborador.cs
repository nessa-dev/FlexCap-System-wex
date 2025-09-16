using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexCap.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPaisAndTimeToColaborador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Colaboradores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pais",
                table: "Colaboradores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Time",
                table: "Colaboradores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "Pais",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "Colaboradores");
        }
    }
}
