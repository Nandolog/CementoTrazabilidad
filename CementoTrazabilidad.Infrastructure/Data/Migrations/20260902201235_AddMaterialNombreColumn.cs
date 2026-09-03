using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialNombreColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                table: "Material");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                table: "Material",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
