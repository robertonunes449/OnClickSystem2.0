using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnClickSystem.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoImagemKit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "Kits",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Kits");
        }
    }
}
