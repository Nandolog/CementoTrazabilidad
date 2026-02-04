using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CementoTrazabilidad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materiales",
                columns: table => new
                {
                    MaterialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PesoBolsa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materiales", x => x.MaterialID);
                });

            migrationBuilder.CreateTable(
                name: "Personal",
                columns: table => new
                {
                    PersonalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Legajo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personal", x => x.PersonalID);
                });

            migrationBuilder.CreateTable(
                name: "ProveedoresBolsa",
                columns: table => new
                {
                    ProveedorBolsaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contacto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProveedoresBolsa", x => x.ProveedorBolsaID);
                });

            migrationBuilder.CreateTable(
                name: "TurnosProduccion",
                columns: table => new
                {
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    TurnoNumero = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnosProduccion", x => x.TurnoProduccionID);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalID = table.Column<int>(type: "int", nullable: false),
                    Legajo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RolSistema = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoLogin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioID);
                    table.ForeignKey(
                        name: "FK_Usuarios_Personal_PersonalID",
                        column: x => x.PersonalID,
                        principalTable: "Personal",
                        principalColumn: "PersonalID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsumoBolsas",
                columns: table => new
                {
                    ConsumoBolsasID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorBolsaID = table.Column<int>(type: "int", nullable: false),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    CantidadBolsas = table.Column<int>(type: "int", nullable: false),
                    FechaConsumo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoteBolsa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumoBolsas", x => x.ConsumoBolsasID);
                    table.ForeignKey(
                        name: "FK_ConsumoBolsas_ProveedoresBolsa_ProveedorBolsaID",
                        column: x => x.ProveedorBolsaID,
                        principalTable: "ProveedoresBolsa",
                        principalColumn: "ProveedorBolsaID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsumoBolsas_TurnosProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnosProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Despachos",
                columns: table => new
                {
                    DespachoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    MaterialID = table.Column<int>(type: "int", nullable: false),
                    NumeroRemito = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadBolsas = table.Column<int>(type: "int", nullable: false),
                    PesoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PatenteCamion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHoraDespacho = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despachos", x => x.DespachoID);
                    table.ForeignKey(
                        name: "FK_Despachos_Materiales_MaterialID",
                        column: x => x.MaterialID,
                        principalTable: "Materiales",
                        principalColumn: "MaterialID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Despachos_TurnosProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnosProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paradas",
                columns: table => new
                {
                    ParadaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    TipoParada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonalResponsableID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paradas", x => x.ParadaID);
                    table.ForeignKey(
                        name: "FK_Paradas_Personal_PersonalResponsableID",
                        column: x => x.PersonalResponsableID,
                        principalTable: "Personal",
                        principalColumn: "PersonalID");
                    table.ForeignKey(
                        name: "FK_Paradas_TurnosProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnosProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalTurno",
                columns: table => new
                {
                    PersonalTurnoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    PersonalID = table.Column<int>(type: "int", nullable: false),
                    RolTurno = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalTurno", x => x.PersonalTurnoID);
                    table.ForeignKey(
                        name: "FK_PersonalTurno_Personal_PersonalID",
                        column: x => x.PersonalID,
                        principalTable: "Personal",
                        principalColumn: "PersonalID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalTurno_TurnosProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnosProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProduccionMaterial",
                columns: table => new
                {
                    ProduccionMaterialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoProduccionID = table.Column<int>(type: "int", nullable: false),
                    MaterialID = table.Column<int>(type: "int", nullable: false),
                    BolsasElaboradas = table.Column<int>(type: "int", nullable: false),
                    BolsasRotas = table.Column<int>(type: "int", nullable: false),
                    HorasMarcha = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduccionMaterial", x => x.ProduccionMaterialID);
                    table.ForeignKey(
                        name: "FK_ProduccionMaterial_Materiales_MaterialID",
                        column: x => x.MaterialID,
                        principalTable: "Materiales",
                        principalColumn: "MaterialID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProduccionMaterial_TurnosProduccion_TurnoProduccionID",
                        column: x => x.TurnoProduccionID,
                        principalTable: "TurnosProduccion",
                        principalColumn: "TurnoProduccionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumoBolsas_ProveedorBolsaID",
                table: "ConsumoBolsas",
                column: "ProveedorBolsaID");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumoBolsas_TurnoProduccionID",
                table: "ConsumoBolsas",
                column: "TurnoProduccionID");

            migrationBuilder.CreateIndex(
                name: "IX_Despachos_MaterialID",
                table: "Despachos",
                column: "MaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_Despachos_TurnoProduccionID",
                table: "Despachos",
                column: "TurnoProduccionID");

            migrationBuilder.CreateIndex(
                name: "IX_Paradas_PersonalResponsableID",
                table: "Paradas",
                column: "PersonalResponsableID");

            migrationBuilder.CreateIndex(
                name: "IX_Paradas_TurnoProduccionID",
                table: "Paradas",
                column: "TurnoProduccionID");

            migrationBuilder.CreateIndex(
                name: "IX_Personal_Legajo",
                table: "Personal",
                column: "Legajo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTurno_PersonalID",
                table: "PersonalTurno",
                column: "PersonalID");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTurno_TurnoProduccionID",
                table: "PersonalTurno",
                column: "TurnoProduccionID");

            migrationBuilder.CreateIndex(
                name: "IX_ProduccionMaterial_MaterialID",
                table: "ProduccionMaterial",
                column: "MaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_ProduccionMaterial_TurnoProduccionID",
                table: "ProduccionMaterial",
                column: "TurnoProduccionID");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosProduccion_Fecha_TurnoNumero",
                table: "TurnosProduccion",
                columns: new[] { "Fecha", "TurnoNumero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Legajo",
                table: "Usuarios",
                column: "Legajo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PersonalID",
                table: "Usuarios",
                column: "PersonalID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumoBolsas");

            migrationBuilder.DropTable(
                name: "Despachos");

            migrationBuilder.DropTable(
                name: "Paradas");

            migrationBuilder.DropTable(
                name: "PersonalTurno");

            migrationBuilder.DropTable(
                name: "ProduccionMaterial");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "ProveedoresBolsa");

            migrationBuilder.DropTable(
                name: "Materiales");

            migrationBuilder.DropTable(
                name: "TurnosProduccion");

            migrationBuilder.DropTable(
                name: "Personal");
        }
    }
}
