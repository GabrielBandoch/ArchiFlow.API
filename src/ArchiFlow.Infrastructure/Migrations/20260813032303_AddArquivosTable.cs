using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArquivosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arquivos",
                columns: table => new
                {
                    ARQ_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ARQ_Projeto_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ARQ_Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ARQ_Url_Storage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ARQ_Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ARQ_Visivel_Cliente = table.Column<bool>(type: "boolean", nullable: false),
                    ARQ_Criado_Em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arquivos", x => x.ARQ_Id);
                    table.ForeignKey(
                        name: "FK_Arquivos_Projetos_ARQ_Projeto_Id",
                        column: x => x.ARQ_Projeto_Id,
                        principalTable: "Projetos",
                        principalColumn: "PJT_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Arquivos_ARQ_Projeto_Id",
                table: "Arquivos",
                column: "ARQ_Projeto_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Arquivos");
        }
    }
}
