using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnClickSystem.Migrations
{
    /// <inheritdoc />
    public partial class Fase7Recomeco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_ID_Associado_Responsavel",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Comissoes_Pedidos_ID_Pedido",
                table: "Comissoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Comissoes_Usuarios_ID_Beneficiario",
                table: "Comissoes");

            migrationBuilder.DropIndex(
                name: "IX_Comissoes_ID_Beneficiario",
                table: "Comissoes");

            migrationBuilder.DropIndex(
                name: "IX_Comissoes_ID_Pedido",
                table: "Comissoes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_ID_Associado_Responsavel",
                table: "Clientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfiguracaoComissao",
                table: "ConfiguracaoComissao");

            migrationBuilder.RenameTable(
                name: "ConfiguracaoComissao",
                newName: "ConfiguracoesComissao");

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Endereco",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfiguracoesComissao",
                table: "ConfiguracoesComissao",
                column: "Nivel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfiguracoesComissao",
                table: "ConfiguracoesComissao");

            migrationBuilder.RenameTable(
                name: "ConfiguracoesComissao",
                newName: "ConfiguracaoComissao");

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Endereco",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfiguracaoComissao",
                table: "ConfiguracaoComissao",
                column: "Nivel");

            migrationBuilder.CreateIndex(
                name: "IX_Comissoes_ID_Beneficiario",
                table: "Comissoes",
                column: "ID_Beneficiario");

            migrationBuilder.CreateIndex(
                name: "IX_Comissoes_ID_Pedido",
                table: "Comissoes",
                column: "ID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ID_Associado_Responsavel",
                table: "Clientes",
                column: "ID_Associado_Responsavel");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_ID_Associado_Responsavel",
                table: "Clientes",
                column: "ID_Associado_Responsavel",
                principalTable: "Usuarios",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comissoes_Pedidos_ID_Pedido",
                table: "Comissoes",
                column: "ID_Pedido",
                principalTable: "Pedidos",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comissoes_Usuarios_ID_Beneficiario",
                table: "Comissoes",
                column: "ID_Beneficiario",
                principalTable: "Usuarios",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
