using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivoFallaYAccionCorrectiva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccionCorrectiva",
                table: "Parada",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoFalla",
                table: "Parada",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Responsable",
                table: "Parada",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccionCorrectiva",
                table: "Parada");

            migrationBuilder.DropColumn(
                name: "MotivoFalla",
                table: "Parada");

            migrationBuilder.DropColumn(
                name: "Responsable",
                table: "Parada");
        }
    }
}
