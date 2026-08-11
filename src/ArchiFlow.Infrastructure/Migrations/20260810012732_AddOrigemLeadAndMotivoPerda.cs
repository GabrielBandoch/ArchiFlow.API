using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrigemLeadAndMotivoPerda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LED_Origem",
                table: "Leads");

            migrationBuilder.AddColumn<string>(
                name: "LED_Motivo_Perda",
                table: "Leads",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LED_Origem_Id",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Origens_Lead",
                columns: table => new
                {
                    OL_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OL_Descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OL_Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    OL_Criado_Em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Origens_Lead", x => x.OL_Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LED_Origem_Id",
                table: "Leads",
                column: "LED_Origem_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Origens_Lead_LED_Origem_Id",
                table: "Leads",
                column: "LED_Origem_Id",
                principalTable: "Origens_Lead",
                principalColumn: "OL_Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Origens_Lead_LED_Origem_Id",
                table: "Leads");

            migrationBuilder.DropTable(
                name: "Origens_Lead");

            migrationBuilder.DropIndex(
                name: "IX_Leads_LED_Origem_Id",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LED_Motivo_Perda",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LED_Origem_Id",
                table: "Leads");

            migrationBuilder.AddColumn<string>(
                name: "LED_Origem",
                table: "Leads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
