using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracionTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionTurnos",
                columns: table => new
                {
                    ConfiguracionTurnoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoNumero = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    OverrideActivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsuarioModifico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionTurnos", x => x.ConfiguracionTurnoID);
                });

            migrationBuilder.CreateTable(
                name: "ProgramacionProduccion",
                columns: table => new
                {
                    ProgramacionProduccionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramacionProduccion", x => x.ProgramacionProduccionID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionTurnos_TurnoFecha",
                table: "ConfiguracionTurnos",
                columns: new[] { "TurnoNumero", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramacionProduccion_Fecha",
                table: "ProgramacionProduccion",
                column: "Fecha",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionTurnos");

            migrationBuilder.DropTable(
                name: "ProgramacionProduccion");
        }
    }
}
