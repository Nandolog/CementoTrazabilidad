using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixMaterialMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "Material",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Material",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                table: "Material",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PersonalID",
                table: "LotesProduccion",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LotesProduccion_PersonalID",
                table: "LotesProduccion",
                column: "PersonalID");

            migrationBuilder.AddForeignKey(
                name: "FK_LotesProduccion_Personal_PersonalID",
                table: "LotesProduccion",
                column: "PersonalID",
                principalTable: "Personal",
                principalColumn: "PersonalID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LotesProduccion_Personal_PersonalID",
                table: "LotesProduccion");

            migrationBuilder.DropIndex(
                name: "IX_LotesProduccion_PersonalID",
                table: "LotesProduccion");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Material");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                table: "Material");

            migrationBuilder.DropColumn(
                name: "PersonalID",
                table: "LotesProduccion");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "Material",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
