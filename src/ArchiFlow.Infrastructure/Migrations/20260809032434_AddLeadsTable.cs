using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    LED_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LED_Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LED_Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LED_Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LED_Origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LED_Status = table.Column<int>(type: "integer", nullable: false),
                    LED_Criado_Em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LED_Atualizado_Em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.LED_Id);
                });

            migrationBuilder.CreateTable(
                name: "Historicos_Contato_Lead",
                columns: table => new
                {
                    HCL_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HCL_Lead_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HCL_Data_Contato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HCL_Canal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HCL_Resumo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historicos_Contato_Lead", x => x.HCL_Id);
                    table.ForeignKey(
                        name: "FK_Historicos_Contato_Lead_Leads_HCL_Lead_Id",
                        column: x => x.HCL_Lead_Id,
                        principalTable: "Leads",
                        principalColumn: "LED_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Historicos_Contato_Lead_HCL_Lead_Id",
                table: "Historicos_Contato_Lead",
                column: "HCL_Lead_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LED_Email",
                table: "Leads",
                column: "LED_Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Historicos_Contato_Lead");

            migrationBuilder.DropTable(
                name: "Leads");
        }
    }
}
