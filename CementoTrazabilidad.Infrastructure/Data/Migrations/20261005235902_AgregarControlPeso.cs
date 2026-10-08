using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarControlPeso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionToleranciaPeso",
                columns: table => new
                {
                    ConfiguracionToleranciaPesoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialID = table.Column<int>(type: "int", nullable: false),
                    PesoObjetivo = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    ToleranciaMinima = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    ToleranciaMaxima = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    ToleranciaAjuste = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionToleranciaPeso", x => x.ConfiguracionToleranciaPesoID);
                    table.ForeignKey(
                        name: "FK_ConfiguracionToleranciaPeso_Material_MaterialID",
                        column: x => x.MaterialID,
                        principalTable: "Material",
                        principalColumn: "MaterialID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ControlPeso",
                columns: table => new
                {
                    ControlPesoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroControl = table.Column<int>(type: "int", nullable: false),
                    OperadorResponsable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PesoObjetivo = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    PesoPromedioControlador = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    PesoPromedioBalanza = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    DiferenciaPromedio = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    DesviacionEstandar = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    DentroDeTolerancia = table.Column<bool>(type: "bit", nullable: false),
                    EstadoControl = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlPeso", x => x.ControlPesoID);
                    table.ForeignKey(
                        name: "FK_ControlPeso_TurnoProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnoProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ControlPesoDetalle",
                columns: table => new
                {
                    ControlPesoDetalleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ControlPesoID = table.Column<int>(type: "int", nullable: false),
                    NumeroBoquilla = table.Column<int>(type: "int", nullable: false),
                    PesoControlador = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    PesoBalanza = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    Diferencia = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    AjusteAplicado = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    DentroDeTolerancia = table.Column<bool>(type: "bit", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlPesoDetalle", x => x.ControlPesoDetalleID);
                    table.ForeignKey(
                        name: "FK_ControlPesoDetalle_ControlPeso_ControlPesoID",
                        column: x => x.ControlPesoID,
                        principalTable: "ControlPeso",
                        principalColumn: "ControlPesoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigTolerancia_Material_Activo",
                table: "ConfiguracionToleranciaPeso",
                columns: new[] { "MaterialID", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_ControlPeso_Turno",
                table: "ControlPeso",
                column: "TurnoProduccionID");

            migrationBuilder.CreateIndex(
                name: "IX_ControlPeso_Turno_Numero",
                table: "ControlPeso",
                columns: new[] { "TurnoProduccionID", "NumeroControl" });

            migrationBuilder.CreateIndex(
                name: "IX_ControlPesoDetalle_Control_Boquilla",
                table: "ControlPesoDetalle",
                columns: new[] { "ControlPesoID", "NumeroBoquilla" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionToleranciaPeso");

            migrationBuilder.DropTable(
                name: "ControlPesoDetalle");

            migrationBuilder.DropTable(
                name: "ControlPeso");
        }
    }
}
