using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

#nullable disable

namespace FlexCap.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🛑 AÇÃO CRÍTICA: Adicionar a coluna RejectionReason à tabela Requests.
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Requests", // Nome da sua tabela de requisições
                type: "TEXT",     // Tipo de dado para SQLite (Use NVARCHAR(MAX) se for SQL Server)
                nullable: true);  // Usamos 'true' porque o campo pode ser null (e deve ser string?)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ação de Reversão: Remover a coluna
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Requests");
        }
    }
}