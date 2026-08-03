using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddAuthAndClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    CLI_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CLI_Lead_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    CLI_Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CLI_Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CLI_Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CLI_Cpf_Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CLI_Senha_Portal = table.Column<string>(type: "text", nullable: true),
                    CLI_Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CLI_Endereco = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.CLI_Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    USR_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    USR_Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    USR_Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    USR_Senha_Hash = table.Column<string>(type: "text", nullable: false),
                    USR_Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    USR_Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    USR_Criado_Em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    USR_Atualizado_Em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.USR_Id);
                });

            migrationBuilder.CreateTable(
                name: "Projetos",
                columns: table => new
                {
                    PJT_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PJT_Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PJT_Descricao = table.Column<string>(type: "text", nullable: true),
                    PJT_Status = table.Column<int>(type: "integer", nullable: true),
                    PJT_Tipo = table.Column<int>(type: "integer", nullable: true),
                    PJT_Data_Inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PJT_Data_Prevista_Entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PJT_Metragem_Total = table.Column<decimal>(type: "numeric", nullable: true),
                    PJT_Cliente_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    PJT_Criado_Em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PJT_Atualizado_Em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projetos", x => x.PJT_Id);
                    table.ForeignKey(
                        name: "FK_Projetos_Clientes_PJT_Cliente_Id",
                        column: x => x.PJT_Cliente_Id,
                        principalTable: "Clientes",
                        principalColumn: "CLI_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Etapas_Projeto",
                columns: table => new
                {
                    ETo_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ETo_Projeto_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    ETo_Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ETo_Descricao = table.Column<string>(type: "text", nullable: true),
                    ETo_Status = table.Column<int>(type: "integer", nullable: true),
                    ETo_Ordem = table.Column<int>(type: "integer", nullable: true),
                    dETo_Data_Conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etapas_Projeto", x => x.ETo_Id);
                    table.ForeignKey(
                        name: "FK_Etapas_Projeto_Projetos_ETo_Projeto_Id",
                        column: x => x.ETo_Projeto_Id,
                        principalTable: "Projetos",
                        principalColumn: "PJT_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CLI_Email",
                table: "Clientes",
                column: "CLI_Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Etapas_Projeto_ETo_Projeto_Id",
                table: "Etapas_Projeto",
                column: "ETo_Projeto_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_PJT_Cliente_Id",
                table: "Projetos",
                column: "PJT_Cliente_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_USR_Email",
                table: "Usuarios",
                column: "USR_Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Etapas_Projeto");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Projetos");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
