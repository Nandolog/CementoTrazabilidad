using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEventoCarga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parada_Personal_PersonalResponsableID",
                table: "Parada");

            migrationBuilder.DropForeignKey(
                name: "FK_Parada_TurnoProduccion_TurnoProduccionID",
                table: "Parada");

            migrationBuilder.DropIndex(
                name: "IX_Parada_PersonalResponsableID",
                table: "Parada");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Parada");

            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "Parada");

            migrationBuilder.DropColumn(
                name: "PersonalResponsableID",
                table: "Parada");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Material");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Material");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                table: "Material");

            migrationBuilder.RenameColumn(
                name: "TipoParada",
                table: "Parada",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Parada",
                newName: "Decripcion");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Material",
                newName: "descripcion");

            migrationBuilder.RenameColumn(
                name: "PesoBolsa",
                table: "Material",
                newName: "PesoPorBolsa");

            migrationBuilder.AddColumn<decimal>(
                name: "DensildadKGm3",
                table: "Material",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "EventosCarga",
                columns: table => new
                {
                    EventoCargaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    ZonaCarga = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosCarga", x => x.EventoCargaID);
                    table.ForeignKey(
                        name: "FK_EventosCarga_TurnoProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnoProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosCarga_TurnoProduccionID",
                table: "EventosCarga",
                column: "TurnoProduccionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Parada_TurnoProduccion_TurnoProduccionID",
                table: "Parada",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parada_TurnoProduccion_TurnoProduccionID",
                table: "Parada");

            migrationBuilder.DropTable(
                name: "EventosCarga");

            migrationBuilder.DropColumn(
                name: "DensildadKGm3",
                table: "Material");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Parada",
                newName: "TipoParada");

            migrationBuilder.RenameColumn(
                name: "Decripcion",
                table: "Parada",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "descripcion",
                table: "Material",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "PesoPorBolsa",
                table: "Material",
                newName: "PesoBolsa");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Parada",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Motivo",
                table: "Parada",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PersonalResponsableID",
                table: "Parada",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Material",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Material",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                table: "Material",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Parada_PersonalResponsableID",
                table: "Parada",
                column: "PersonalResponsableID");

            migrationBuilder.AddForeignKey(
                name: "FK_Parada_Personal_PersonalResponsableID",
                table: "Parada",
                column: "PersonalResponsableID",
                principalTable: "Personal",
                principalColumn: "PersonalID");

            migrationBuilder.AddForeignKey(
                name: "FK_Parada_TurnoProduccion_TurnoProduccionID",
                table: "Parada",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
