using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexCap.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInactivityDetailsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Colaboradores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InactivityReason",
                table: "Colaboradores",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "InactivityReason",
                table: "Colaboradores");
        }
    }
}
