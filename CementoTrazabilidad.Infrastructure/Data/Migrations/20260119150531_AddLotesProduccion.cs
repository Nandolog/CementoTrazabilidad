using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLotesProduccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_ProveedorBolsa_ProveedorBolsaID",
                table: "ConsumoBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_TurnoProduccion_TurnoProduccionID",
                table: "ConsumoBolsas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProveedorBolsa",
                table: "ProveedorBolsa");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "ProveedorBolsa");

            migrationBuilder.RenameTable(
                name: "ProveedorBolsa",
                newName: "ProveedoresBolsa");

            migrationBuilder.AddColumn<int>(
                name: "DuracionMinutos",
                table: "Parada",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LoteBolsa",
                table: "ConsumoBolsas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BolsasDefectuosas",
                table: "ConsumoBolsas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "ConsumoBolsas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProduccionMaterialID",
                table: "ConsumoBolsas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoCemento",
                table: "ConsumoBolsas",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "ProveedoresBolsa",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "ProveedoresBolsa",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ProveedoresBolsa",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Contacto",
                table: "ProveedoresBolsa",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CUIT",
                table: "ProveedoresBolsa",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "ProveedoresBolsa",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                table: "ProveedoresBolsa",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProveedoresBolsa",
                table: "ProveedoresBolsa",
                column: "ProveedorBolsaID");

            migrationBuilder.CreateTable(
                name: "LoteProduccion",
                columns: table => new
                {
                    LoteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoID = table.Column<int>(type: "int", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CantidadBolsas = table.Column<int>(type: "int", nullable: false),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoRegistro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoteProduccion", x => x.LoteID);
                    table.ForeignKey(
                        name: "FK_LoteProduccion_TurnoProduccion_TurnoID",
                        column: x => x.TurnoID,
                        principalTable: "TurnoProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LotesProveedorBolsa",
                columns: table => new
                {
                    LoteProveedorBolsaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorBolsaID = table.Column<int>(type: "int", nullable: false),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoCemento = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FechaRecepcion = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    CantidadInicial = table.Column<int>(type: "int", nullable: false),
                    CantidadActual = table.Column<int>(type: "int", nullable: false),
                    CantidadDefectuosa = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotesProveedorBolsa", x => x.LoteProveedorBolsaID);
                    table.ForeignKey(
                        name: "FK_LotesProveedorBolsa_ProveedoresBolsa_ProveedorBolsaID",
                        column: x => x.ProveedorBolsaID,
                        principalTable: "ProveedoresBolsa",
                        principalColumn: "ProveedorBolsaID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosDefectosBolsas",
                columns: table => new
                {
                    RegistroDefectoBolsaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteProveedorBolsaID = table.Column<int>(type: "int", nullable: false),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    ProduccionMaterialID = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    TipoDefecto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaHoraRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gravedad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProveedorNotificado = table.Column<bool>(type: "bit", nullable: false),
                    FechaNotificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosDefectosBolsas", x => x.RegistroDefectoBolsaID);
                    table.ForeignKey(
                        name: "FK_RegistrosDefectosBolsas_LotesProveedorBolsa_LoteProveedorBolsaID",
                        column: x => x.LoteProveedorBolsaID,
                        principalTable: "LotesProveedorBolsa",
                        principalColumn: "LoteProveedorBolsaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosDefectosBolsas_ProduccionMaterial_ProduccionMaterialID",
                        column: x => x.ProduccionMaterialID,
                        principalTable: "ProduccionMaterial",
                        principalColumn: "ProduccionMaterialID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosDefectosBolsas_TurnoProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnoProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ProveedoresBolsa",
                columns: new[] { "ProveedorBolsaID", "Activo", "CUIT", "Contacto", "Descripcion", "Email", "Nombre", "RazonSocial", "Telefono" },
                values: new object[,]
                {
                    { 1, true, null, null, "Cooperativa de cemento", null, "Coselapa", null, null },
                    { 2, true, null, null, "Fabricante de bolsas", null, "Primo Tedesco", null, null },
                    { 3, true, null, null, "Bolsas para cemento", null, "Bolpar", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumoBolsas_ProduccionMaterialID",
                table: "ConsumoBolsas",
                column: "ProduccionMaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_LoteProduccion_FechaHoraFin",
                table: "LoteProduccion",
                column: "FechaHoraFin");

            migrationBuilder.CreateIndex(
                name: "IX_LoteProduccion_FechaHoraInicio",
                table: "LoteProduccion",
                column: "FechaHoraInicio");

            migrationBuilder.CreateIndex(
                name: "IX_LoteProduccion_NumeroLote",
                table: "LoteProduccion",
                column: "NumeroLote",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoteProduccion_TurnoID",
                table: "LoteProduccion",
                column: "TurnoID");

            migrationBuilder.CreateIndex(
                name: "IX_LotesProveedorBolsa_ProveedorBolsaID_NumeroLote",
                table: "LotesProveedorBolsa",
                columns: new[] { "ProveedorBolsaID", "NumeroLote" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDefectosBolsas_LoteProveedorBolsaID",
                table: "RegistrosDefectosBolsas",
                column: "LoteProveedorBolsaID");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDefectosBolsas_ProduccionMaterialID",
                table: "RegistrosDefectosBolsas",
                column: "ProduccionMaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDefectosBolsas_TurnoProduccionID",
                table: "RegistrosDefectosBolsas",
                column: "TurnoProduccionID");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumoBolsas_ProduccionMaterial_ProduccionMaterialID",
                table: "ConsumoBolsas",
                column: "ProduccionMaterialID",
                principalTable: "ProduccionMaterial",
                principalColumn: "ProduccionMaterialID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumoBolsas_ProveedoresBolsa_ProveedorBolsaID",
                table: "ConsumoBolsas",
                column: "ProveedorBolsaID",
                principalTable: "ProveedoresBolsa",
                principalColumn: "ProveedorBolsaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumoBolsas_TurnoProduccion_TurnoProduccionID",
                table: "ConsumoBolsas",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_ProduccionMaterial_ProduccionMaterialID",
                table: "ConsumoBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_ProveedoresBolsa_ProveedorBolsaID",
                table: "ConsumoBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_TurnoProduccion_TurnoProduccionID",
                table: "ConsumoBolsas");

            migrationBuilder.DropTable(
                name: "LoteProduccion");

            migrationBuilder.DropTable(
                name: "RegistrosDefectosBolsas");

            migrationBuilder.DropTable(
                name: "LotesProveedorBolsa");

            migrationBuilder.DropIndex(
                name: "IX_ConsumoBolsas_ProduccionMaterialID",
                table: "ConsumoBolsas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProveedoresBolsa",
                table: "ProveedoresBolsa");

            migrationBuilder.DeleteData(
                table: "ProveedoresBolsa",
                keyColumn: "ProveedorBolsaID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProveedoresBolsa",
                keyColumn: "ProveedorBolsaID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProveedoresBolsa",
                keyColumn: "ProveedorBolsaID",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "DuracionMinutos",
                table: "Parada");

            migrationBuilder.DropColumn(
                name: "BolsasDefectuosas",
                table: "ConsumoBolsas");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "ConsumoBolsas");

            migrationBuilder.DropColumn(
                name: "ProduccionMaterialID",
                table: "ConsumoBolsas");

            migrationBuilder.DropColumn(
                name: "TipoCemento",
                table: "ConsumoBolsas");

            migrationBuilder.DropColumn(
                name: "CUIT",
                table: "ProveedoresBolsa");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "ProveedoresBolsa");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                table: "ProveedoresBolsa");

            migrationBuilder.RenameTable(
                name: "ProveedoresBolsa",
                newName: "ProveedorBolsa");

            migrationBuilder.AlterColumn<string>(
                name: "LoteBolsa",
                table: "ConsumoBolsas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "ProveedorBolsa",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "ProveedorBolsa",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ProveedorBolsa",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Contacto",
                table: "ProveedorBolsa",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "ProveedorBolsa",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProveedorBolsa",
                table: "ProveedorBolsa",
                column: "ProveedorBolsaID");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumoBolsas_ProveedorBolsa_ProveedorBolsaID",
                table: "ConsumoBolsas",
                column: "ProveedorBolsaID",
                principalTable: "ProveedorBolsa",
                principalColumn: "ProveedorBolsaID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumoBolsas_TurnoProduccion_TurnoProduccionID",
                table: "ConsumoBolsas",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
