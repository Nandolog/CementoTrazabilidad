using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablasConsumoBolsas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tabla ProveedoresBolsa
            migrationBuilder.CreateTable(
                name: "ProveedoresBolsa",
                columns: table => new
                {
                    ProveedorBolsaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RazonSocial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CUIT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Contacto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProveedoresBolsa", x => x.ProveedorBolsaID);
                });

            // Crear tabla ConsumoBolsas
            migrationBuilder.CreateTable(
                name: "ConsumoBolsas",
                columns: table => new
                {
                    ConsumoBolsasID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorBolsaID = table.Column<int>(type: "int", nullable: false),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    ProduccionMaterialID = table.Column<int>(type: "int", nullable: true),
                    CantidadBolsas = table.Column<int>(type: "int", nullable: false),
                    BolsasDefectuosas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaConsumo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoteBolsa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TipoCemento = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumoBolsas", x => x.ConsumoBolsasID);
                    table.ForeignKey(
                        name: "FK_ConsumoBolsas_ProveedoresBolsa_ProveedorBolsaID",
                        column: x => x.ProveedorBolsaID,
                        principalTable: "ProveedoresBolsa",
                        principalColumn: "ProveedorBolsaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumoBolsas_TurnosProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnosProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumoBolsas_ProduccionMateriales_ProduccionMaterialID",
                        column: x => x.ProduccionMaterialID,
                        principalTable: "ProduccionMateriales",
                        principalColumn: "ProduccionMaterialID",
                        onDelete: ReferentialAction.Restrict);
                });

            // Crear índices
            migrationBuilder.CreateIndex(
                name: "IX_ConsumoBolsas_ProveedorBolsaID",
                table: "ConsumoBolsas",
                column: "ProveedorBolsaID");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumoBolsas_TurnoProduccionID",
                table: "ConsumoBolsas",
                column: "TurnoProduccionID");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumoBolsas_ProduccionMaterialID",
                table: "ConsumoBolsas",
                column: "ProduccionMaterialID");

            // Insertar proveedores de ejemplo
            migrationBuilder.InsertData(
                table: "ProveedoresBolsa",
                columns: new[] { "Nombre", "Descripcion", "Activo" },
                values: new object[,]
                {
                    { "Coselapa", "Proveedor principal de bolsas", true },
                    { "Primo Tedesco", "Proveedor alternativo de bolsas", true },
                    { "Bolpar", "Bolsas de Paraguay", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumoBolsas");

            migrationBuilder.DropTable(
                name: "ProveedoresBolsa");
        }
    }
}