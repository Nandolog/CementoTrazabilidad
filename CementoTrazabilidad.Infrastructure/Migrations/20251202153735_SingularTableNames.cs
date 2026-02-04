using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SingularTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_ProveedoresBolsa_ProveedorBolsaID",
                table: "ConsumoBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_TurnosProduccion_TurnoProduccionID",
                table: "ConsumoBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_Despachos_Materiales_MaterialID",
                table: "Despachos");

            migrationBuilder.DropForeignKey(
                name: "FK_Despachos_TurnosProduccion_TurnoProduccionID",
                table: "Despachos");

            migrationBuilder.DropForeignKey(
                name: "FK_Paradas_Personal_PersonalResponsableID",
                table: "Paradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Paradas_TurnosProduccion_TurnoProduccionID",
                table: "Paradas");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonalTurno_TurnosProduccion_TurnoProduccionID",
                table: "PersonalTurno");

            migrationBuilder.DropForeignKey(
                name: "FK_ProduccionMaterial_Materiales_MaterialID",
                table: "ProduccionMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_ProduccionMaterial_TurnosProduccion_TurnoProduccionID",
                table: "ProduccionMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Personal_PersonalID",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurnosProduccion",
                table: "TurnosProduccion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProveedoresBolsa",
                table: "ProveedoresBolsa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Paradas",
                table: "Paradas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Materiales",
                table: "Materiales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Despachos",
                table: "Despachos");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Usuario");

            migrationBuilder.RenameTable(
                name: "TurnosProduccion",
                newName: "TurnoProduccion");

            migrationBuilder.RenameTable(
                name: "ProveedoresBolsa",
                newName: "ProveedorBolsa");

            migrationBuilder.RenameTable(
                name: "Paradas",
                newName: "Parada");

            migrationBuilder.RenameTable(
                name: "Materiales",
                newName: "Material");

            migrationBuilder.RenameTable(
                name: "Despachos",
                newName: "Despacho");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_PersonalID",
                table: "Usuario",
                newName: "IX_Usuario_PersonalID");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_Legajo",
                table: "Usuario",
                newName: "IX_Usuario_Legajo");

            migrationBuilder.RenameIndex(
                name: "IX_TurnosProduccion_Fecha_TurnoNumero",
                table: "TurnoProduccion",
                newName: "IX_TurnoProduccion_Fecha_TurnoNumero");

            migrationBuilder.RenameIndex(
                name: "IX_Paradas_TurnoProduccionID",
                table: "Parada",
                newName: "IX_Parada_TurnoProduccionID");

            migrationBuilder.RenameIndex(
                name: "IX_Paradas_PersonalResponsableID",
                table: "Parada",
                newName: "IX_Parada_PersonalResponsableID");

            migrationBuilder.RenameIndex(
                name: "IX_Despachos_TurnoProduccionID",
                table: "Despacho",
                newName: "IX_Despacho_TurnoProduccionID");

            migrationBuilder.RenameIndex(
                name: "IX_Despachos_MaterialID",
                table: "Despacho",
                newName: "IX_Despacho_MaterialID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuario",
                table: "Usuario",
                column: "UsuarioID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurnoProduccion",
                table: "TurnoProduccion",
                column: "TurnoProduccionID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProveedorBolsa",
                table: "ProveedorBolsa",
                column: "ProveedorBolsaID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Parada",
                table: "Parada",
                column: "ParadaID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Material",
                table: "Material",
                column: "MaterialID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Despacho",
                table: "Despacho",
                column: "DespachoID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalTurno_TurnoProduccion_TurnoProduccionID",
                table: "PersonalTurno",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProduccionMaterial_Material_MaterialID",
                table: "ProduccionMaterial",
                column: "MaterialID",
                principalTable: "Material",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProduccionMaterial_TurnoProduccion_TurnoProduccionID",
                table: "ProduccionMaterial",
                column: "TurnoProduccionID",
                principalTable: "TurnoProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Personal_PersonalID",
                table: "Usuario",
                column: "PersonalID",
                principalTable: "Personal",
                principalColumn: "PersonalID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_ProveedorBolsa_ProveedorBolsaID",
                table: "ConsumoBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsumoBolsas_TurnoProduccion_TurnoProduccionID",
                table: "ConsumoBolsas");

            migrationBuilder.DropForeignKey(
                name: "FK_Despacho_Material_MaterialID",
                table: "Despacho");

            migrationBuilder.DropForeignKey(
                name: "FK_Despacho_TurnoProduccion_TurnoProduccionID",
                table: "Despacho");

            migrationBuilder.DropForeignKey(
                name: "FK_Parada_Personal_PersonalResponsableID",
                table: "Parada");

            migrationBuilder.DropForeignKey(
                name: "FK_Parada_TurnoProduccion_TurnoProduccionID",
                table: "Parada");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonalTurno_TurnoProduccion_TurnoProduccionID",
                table: "PersonalTurno");

            migrationBuilder.DropForeignKey(
                name: "FK_ProduccionMaterial_Material_MaterialID",
                table: "ProduccionMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_ProduccionMaterial_TurnoProduccion_TurnoProduccionID",
                table: "ProduccionMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Personal_PersonalID",
                table: "Usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuario",
                table: "Usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurnoProduccion",
                table: "TurnoProduccion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProveedorBolsa",
                table: "ProveedorBolsa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Parada",
                table: "Parada");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Material",
                table: "Material");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Despacho",
                table: "Despacho");

            migrationBuilder.RenameTable(
                name: "Usuario",
                newName: "Usuarios");

            migrationBuilder.RenameTable(
                name: "TurnoProduccion",
                newName: "TurnosProduccion");

            migrationBuilder.RenameTable(
                name: "ProveedorBolsa",
                newName: "ProveedoresBolsa");

            migrationBuilder.RenameTable(
                name: "Parada",
                newName: "Paradas");

            migrationBuilder.RenameTable(
                name: "Material",
                newName: "Materiales");

            migrationBuilder.RenameTable(
                name: "Despacho",
                newName: "Despachos");

            migrationBuilder.RenameIndex(
                name: "IX_Usuario_PersonalID",
                table: "Usuarios",
                newName: "IX_Usuarios_PersonalID");

            migrationBuilder.RenameIndex(
                name: "IX_Usuario_Legajo",
                table: "Usuarios",
                newName: "IX_Usuarios_Legajo");

            migrationBuilder.RenameIndex(
                name: "IX_TurnoProduccion_Fecha_TurnoNumero",
                table: "TurnosProduccion",
                newName: "IX_TurnosProduccion_Fecha_TurnoNumero");

            migrationBuilder.RenameIndex(
                name: "IX_Parada_TurnoProduccionID",
                table: "Paradas",
                newName: "IX_Paradas_TurnoProduccionID");

            migrationBuilder.RenameIndex(
                name: "IX_Parada_PersonalResponsableID",
                table: "Paradas",
                newName: "IX_Paradas_PersonalResponsableID");

            migrationBuilder.RenameIndex(
                name: "IX_Despacho_TurnoProduccionID",
                table: "Despachos",
                newName: "IX_Despachos_TurnoProduccionID");

            migrationBuilder.RenameIndex(
                name: "IX_Despacho_MaterialID",
                table: "Despachos",
                newName: "IX_Despachos_MaterialID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "UsuarioID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurnosProduccion",
                table: "TurnosProduccion",
                column: "TurnoProduccionID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProveedoresBolsa",
                table: "ProveedoresBolsa",
                column: "ProveedorBolsaID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Paradas",
                table: "Paradas",
                column: "ParadaID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Materiales",
                table: "Materiales",
                column: "MaterialID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Despachos",
                table: "Despachos",
                column: "DespachoID");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumoBolsas_ProveedoresBolsa_ProveedorBolsaID",
                table: "ConsumoBolsas",
                column: "ProveedorBolsaID",
                principalTable: "ProveedoresBolsa",
                principalColumn: "ProveedorBolsaID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumoBolsas_TurnosProduccion_TurnoProduccionID",
                table: "ConsumoBolsas",
                column: "TurnoProduccionID",
                principalTable: "TurnosProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Despachos_Materiales_MaterialID",
                table: "Despachos",
                column: "MaterialID",
                principalTable: "Materiales",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Despachos_TurnosProduccion_TurnoProduccionID",
                table: "Despachos",
                column: "TurnoProduccionID",
                principalTable: "TurnosProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Paradas_Personal_PersonalResponsableID",
                table: "Paradas",
                column: "PersonalResponsableID",
                principalTable: "Personal",
                principalColumn: "PersonalID");

            migrationBuilder.AddForeignKey(
                name: "FK_Paradas_TurnosProduccion_TurnoProduccionID",
                table: "Paradas",
                column: "TurnoProduccionID",
                principalTable: "TurnosProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalTurno_TurnosProduccion_TurnoProduccionID",
                table: "PersonalTurno",
                column: "TurnoProduccionID",
                principalTable: "TurnosProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProduccionMaterial_Materiales_MaterialID",
                table: "ProduccionMaterial",
                column: "MaterialID",
                principalTable: "Materiales",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProduccionMaterial_TurnosProduccion_TurnoProduccionID",
                table: "ProduccionMaterial",
                column: "TurnoProduccionID",
                principalTable: "TurnosProduccion",
                principalColumn: "TurnoProduccionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Personal_PersonalID",
                table: "Usuarios",
                column: "PersonalID",
                principalTable: "Personal",
                principalColumn: "PersonalID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
