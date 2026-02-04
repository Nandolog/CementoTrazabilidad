using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Despacho_Material_MaterialID",
                table: "Despacho");

            migrationBuilder.DropForeignKey(
                name: "FK_Despacho_TurnoProduccion_TurnoProduccionID",
                table: "Despacho");

            migrationBuilder.DropForeignKey(
                name: "FK_LoteProduccion_TurnoProduccion_TurnoID",
                table: "LoteProduccion");

            migrationBuilder.DropForeignKey(
                name: "FK_LotesProveedorBolsa_ProveedoresBolsa_ProveedorBolsaID",
                table: "LotesProveedorBolsa");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosDefectosBolsas_LotesProveedorBolsa_LoteProveedorBolsaID",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosDefectosBolsas_ProduccionMaterial_ProduccionMaterialID",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosDefectosBolsas_TurnoProduccion_TurnoProduccionID",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegistrosDefectosBolsas",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LotesProveedorBolsa",
                table: "LotesProveedorBolsa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoteProduccion",
                table: "LoteProduccion");

            migrationBuilder.DropColumn(
                name: "CantidadBolsas",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "NumeroRemito",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "PatenteCamion",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "PesoTotal",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "Transportista",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.DropColumn(
                name: "FechaNotificacion",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.DropColumn(
                name: "Gravedad",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.DropColumn(
                name: "ProveedorNotificado",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.DropColumn(
                name: "RegistradoPor",
                table: "RegistrosDefectosBolsas");

            migrationBuilder.RenameTable(
                name: "RegistrosDefectosBolsas",
                newName: "RegistroDefectoBolsa");

            migrationBuilder.RenameTable(
                name: "LotesProveedorBolsa",
                newName: "LoteProveedorBolsa");

            migrationBuilder.RenameTable(
                name: "LoteProduccion",
                newName: "LotesProduccion");

            migrationBuilder.RenameColumn(
                name: "FechaHoraRegistro",
                table: "RegistroDefectoBolsa",
                newName: "FechaRegistro");

            migrationBuilder.RenameColumn(
                name: "Cantidad",
                table: "RegistroDefectoBolsa",
                newName: "CantidadDefectuosa");

            migrationBuilder.RenameColumn(
                name: "RegistroDefectoBolsaID",
                table: "RegistroDefectoBolsa",
                newName: "RegistroDefectoID");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosDefectosBolsas_TurnoProduccionID",
                table: "RegistroDefectoBolsa",
                newName: "IX_RegistroDefectoBolsa_TurnoProduccionID");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosDefectosBolsas_ProduccionMaterialID",
                table: "RegistroDefectoBolsa",
                newName: "IX_RegistroDefectoBolsa_ProduccionMaterialID");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosDefectosBolsas_LoteProveedorBolsaID",
                table: "RegistroDefectoBolsa",
                newName: "IX_RegistroDefectoBolsa_LoteProveedorBolsaID");

            migrationBuilder.RenameIndex(
                name: "IX_LotesProveedorBolsa_ProveedorBolsaID_NumeroLote",
                table: "LoteProveedorBolsa",
                newName: "IX_LoteProveedorBolsa_ProveedorBolsaID_NumeroLote");

            migrationBuilder.RenameIndex(
                name: "IX_LoteProduccion_TurnoID",
                table: "LotesProduccion",
                newName: "IX_LotesProduccion_TurnoID");

            migrationBuilder.RenameIndex(
                name: "IX_LoteProduccion_NumeroLote",
                table: "LotesProduccion",
                newName: "IX_LotesProduccion_NumeroLote");

            migrationBuilder.RenameIndex(
                name: "IX_LoteProduccion_FechaHoraInicio",
                table: "LotesProduccion",
                newName: "IX_LotesProduccion_FechaHoraInicio");

            migrationBuilder.RenameIndex(
                name: "IX_LoteProduccion_FechaHoraFin",
                table: "LotesProduccion",
                newName: "IX_LotesProduccion_FechaHoraFin");

            migrationBuilder.AddColumn<int>(
                name: "CantidadBolsas",
                table: "EventosCarga",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TurnoProduccionID",
                table: "Despacho",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaterialID",
                table: "Despacho",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraDespacho",
                table: "Despacho",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Destino",
                table: "Despacho",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Bolsas",
                table: "Despacho",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Camiones",
                table: "Despacho",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modalidad",
                table: "Despacho",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Toneladas",
                table: "Despacho",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "LotesProduccion",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraFin",
                table: "LotesProduccion",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "MaterialID",
                table: "LotesProduccion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegistroDefectoBolsa",
                table: "RegistroDefectoBolsa",
                column: "RegistroDefectoID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoteProveedorBolsa",
                table: "LoteProveedorBolsa",
                column: "LoteProveedorBolsaID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LotesProduccion",
                table: "LotesProduccion",
                column: "LoteID");

            migrationBuilder.CreateIndex(
                name: "IX_LotesProduccion_MaterialID",
                table: "LotesProduccion",
                column: "MaterialID");

            migrationBuilder.AddForeignKey(
                name: "FK_Despacho_Material_MaterialID",
                table: "Despacho",
                column: "MaterialID",
                principalTable: "Material",
                principalColumn: "MaterialID");

            migrationBuilder.AddForeignKey(
                name: "FK_Despacho_TurnoProduccion_TurnoProduccionID",
                table: "Despacho",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LoteProveedorBolsa_ProveedoresBolsa_ProveedorBolsaID",
                table: "LoteProveedorBolsa",
                column: "ProveedorBolsaID",
                principalTable: "ProveedoresBolsa",
                principalColumn: "ProveedorBolsaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LotesProduccion_Material_MaterialID",
                table: "LotesProduccion",
                column: "MaterialID",
                principalTable: "Material",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LotesProduccion_TurnoProduccion_TurnoID",
                table: "LotesProduccion",
                column: "TurnoID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroDefectoBolsa_LoteProveedorBolsa_LoteProveedorBolsaID",
                table: "RegistroDefectoBolsa",
                column: "LoteProveedorBolsaID",
                principalTable: "LoteProveedorBolsa",
                principalColumn: "LoteProveedorBolsaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroDefectoBolsa_ProduccionMaterial_ProduccionMaterialID",
                table: "RegistroDefectoBolsa",
                column: "ProduccionMaterialID",
                principalTable: "ProduccionMaterial",
                principalColumn: "ProduccionMaterialID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroDefectoBolsa_TurnoProduccion_TurnoProduccionID",
                table: "RegistroDefectoBolsa",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Despacho_Material_MaterialID",
                table: "Despacho");

            migrationBuilder.DropForeignKey(
                name: "FK_Despacho_TurnoProduccion_TurnoProduccionID",
                table: "Despacho");

            migrationBuilder.DropForeignKey(
                name: "FK_LoteProveedorBolsa_ProveedoresBolsa_ProveedorBolsaID",
                table: "LoteProveedorBolsa");

            migrationBuilder.DropForeignKey(
                name: "FK_LotesProduccion_Material_MaterialID",
                table: "LotesProduccion");

            migrationBuilder.DropForeignKey(
                name: "FK_LotesProduccion_TurnoProduccion_TurnoID",
                table: "LotesProduccion");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistroDefectoBolsa_LoteProveedorBolsa_LoteProveedorBolsaID",
                table: "RegistroDefectoBolsa");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistroDefectoBolsa_ProduccionMaterial_ProduccionMaterialID",
                table: "RegistroDefectoBolsa");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistroDefectoBolsa_TurnoProduccion_TurnoProduccionID",
                table: "RegistroDefectoBolsa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegistroDefectoBolsa",
                table: "RegistroDefectoBolsa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LotesProduccion",
                table: "LotesProduccion");

            migrationBuilder.DropIndex(
                name: "IX_LotesProduccion_MaterialID",
                table: "LotesProduccion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoteProveedorBolsa",
                table: "LoteProveedorBolsa");

            migrationBuilder.DropColumn(
                name: "CantidadBolsas",
                table: "EventosCarga");

            migrationBuilder.DropColumn(
                name: "Bolsas",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "Camiones",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "Modalidad",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "Toneladas",
                table: "Despacho");

            migrationBuilder.DropColumn(
                name: "MaterialID",
                table: "LotesProduccion");

            migrationBuilder.RenameTable(
                name: "RegistroDefectoBolsa",
                newName: "RegistrosDefectosBolsas");

            migrationBuilder.RenameTable(
                name: "LotesProduccion",
                newName: "LoteProduccion");

            migrationBuilder.RenameTable(
                name: "LoteProveedorBolsa",
                newName: "LotesProveedorBolsa");

            migrationBuilder.RenameColumn(
                name: "FechaRegistro",
                table: "RegistrosDefectosBolsas",
                newName: "FechaHoraRegistro");

            migrationBuilder.RenameColumn(
                name: "CantidadDefectuosa",
                table: "RegistrosDefectosBolsas",
                newName: "Cantidad");

            migrationBuilder.RenameColumn(
                name: "RegistroDefectoID",
                table: "RegistrosDefectosBolsas",
                newName: "RegistroDefectoBolsaID");

            migrationBuilder.RenameIndex(
                name: "IX_RegistroDefectoBolsa_TurnoProduccionID",
                table: "RegistrosDefectosBolsas",
                newName: "IX_RegistrosDefectosBolsas_TurnoProduccionID");

            migrationBuilder.RenameIndex(
                name: "IX_RegistroDefectoBolsa_ProduccionMaterialID",
                table: "RegistrosDefectosBolsas",
                newName: "IX_RegistrosDefectosBolsas_ProduccionMaterialID");

            migrationBuilder.RenameIndex(
                name: "IX_RegistroDefectoBolsa_LoteProveedorBolsaID",
                table: "RegistrosDefectosBolsas",
                newName: "IX_RegistrosDefectosBolsas_LoteProveedorBolsaID");

            migrationBuilder.RenameIndex(
                name: "IX_LotesProduccion_TurnoID",
                table: "LoteProduccion",
                newName: "IX_LoteProduccion_TurnoID");

            migrationBuilder.RenameIndex(
                name: "IX_LotesProduccion_NumeroLote",
                table: "LoteProduccion",
                newName: "IX_LoteProduccion_NumeroLote");

            migrationBuilder.RenameIndex(
                name: "IX_LotesProduccion_FechaHoraInicio",
                table: "LoteProduccion",
                newName: "IX_LoteProduccion_FechaHoraInicio");

            migrationBuilder.RenameIndex(
                name: "IX_LotesProduccion_FechaHoraFin",
                table: "LoteProduccion",
                newName: "IX_LoteProduccion_FechaHoraFin");

            migrationBuilder.RenameIndex(
                name: "IX_LoteProveedorBolsa_ProveedorBolsaID_NumeroLote",
                table: "LotesProveedorBolsa",
                newName: "IX_LotesProveedorBolsa_ProveedorBolsaID_NumeroLote");

            migrationBuilder.AlterColumn<int>(
                name: "TurnoProduccionID",
                table: "Despacho",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaterialID",
                table: "Despacho",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraDespacho",
                table: "Despacho",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Destino",
                table: "Despacho",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "CantidadBolsas",
                table: "Despacho",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "Despacho",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Despacho",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroRemito",
                table: "Despacho",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PatenteCamion",
                table: "Despacho",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PesoTotal",
                table: "Despacho",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Transportista",
                table: "Despacho",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "RegistrosDefectosBolsas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNotificacion",
                table: "RegistrosDefectosBolsas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gravedad",
                table: "RegistrosDefectosBolsas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProveedorNotificado",
                table: "RegistrosDefectosBolsas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RegistradoPor",
                table: "RegistrosDefectosBolsas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "LoteProduccion",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraFin",
                table: "LoteProduccion",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegistrosDefectosBolsas",
                table: "RegistrosDefectosBolsas",
                column: "RegistroDefectoBolsaID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoteProduccion",
                table: "LoteProduccion",
                column: "LoteID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LotesProveedorBolsa",
                table: "LotesProveedorBolsa",
                column: "LoteProveedorBolsaID");

            migrationBuilder.AddForeignKey(
                name: "FK_Despacho_Material_MaterialID",
                table: "Despacho",
                column: "MaterialID",
                principalTable: "Material",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Despacho_TurnoProduccion_TurnoProduccionID",
                table: "Despacho",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoteProduccion_TurnoProduccion_TurnoID",
                table: "LoteProduccion",
                column: "TurnoID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LotesProveedorBolsa_ProveedoresBolsa_ProveedorBolsaID",
                table: "LotesProveedorBolsa",
                column: "ProveedorBolsaID",
                principalTable: "ProveedoresBolsa",
                principalColumn: "ProveedorBolsaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosDefectosBolsas_LotesProveedorBolsa_LoteProveedorBolsaID",
                table: "RegistrosDefectosBolsas",
                column: "LoteProveedorBolsaID",
                principalTable: "LotesProveedorBolsa",
                principalColumn: "LoteProveedorBolsaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosDefectosBolsas_ProduccionMaterial_ProduccionMaterialID",
                table: "RegistrosDefectosBolsas",
                column: "ProduccionMaterialID",
                principalTable: "ProduccionMaterial",
                principalColumn: "ProduccionMaterialID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosDefectosBolsas_TurnoProduccion_TurnoProduccionID",
                table: "RegistrosDefectosBolsas",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
