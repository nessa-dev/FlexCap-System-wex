using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexCap.Web.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullCommentInLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "RequestLogs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_ActionByUserId",
                table: "RequestLogs",
                column: "ActionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_RequestId",
                table: "RequestLogs",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_Colaboradores_ActionByUserId",
                table: "RequestLogs",
                column: "ActionByUserId",
                principalTable: "Colaboradores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_Requests_RequestId",
                table: "RequestLogs",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_Colaboradores_ActionByUserId",
                table: "RequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_Requests_RequestId",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_ActionByUserId",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_RequestId",
                table: "RequestLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "RequestLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
