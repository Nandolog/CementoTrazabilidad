using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBolsasRotasYHorasMarcha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BolsasAnden",
                table: "LotesProduccion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BolsasPaletizado",
                table: "LotesProduccion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZonaCarga",
                table: "LotesProduccion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BolsasAnden",
                table: "LotesProduccion");

            migrationBuilder.DropColumn(
                name: "BolsasPaletizado",
                table: "LotesProduccion");

            migrationBuilder.DropColumn(
                name: "ZonaCarga",
                table: "LotesProduccion");
        }
    }
}
