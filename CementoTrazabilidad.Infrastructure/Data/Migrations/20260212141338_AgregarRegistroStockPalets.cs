using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRegistroStockPalets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosStockPalets",
                columns: table => new
                {
                    RegistroStockPaletsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    PersonalID = table.Column<int>(type: "int", nullable: false),
                    StockInicialC32 = table.Column<int>(type: "int", nullable: false),
                    StockInicialF40 = table.Column<int>(type: "int", nullable: false),
                    StockFinalC32 = table.Column<int>(type: "int", nullable: true),
                    StockFinalF40 = table.Column<int>(type: "int", nullable: true),
                    FechaHoraRegistroInicial = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraRegistroFinal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacionesInicio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ObservacionesFin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosStockPalets", x => x.RegistroStockPaletsID);
                    table.ForeignKey(
                        name: "FK_RegistrosStockPalets_Personal_PersonalID",
                        column: x => x.PersonalID,
                        principalTable: "Personal",
                        principalColumn: "PersonalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosStockPalets_TurnoProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnoProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosStockPalets_PersonalID",
                table: "RegistrosStockPalets",
                column: "PersonalID");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosStockPalets_TurnoProduccionID_Activo",
                table: "RegistrosStockPalets",
                columns: new[] { "TurnoProduccionID", "Activo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosStockPalets");
        }
    }
}
