using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoApi.Migrations
{
    /// <inheritdoc />
    public partial class TablasProduccionEmanuel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "actores",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", nullable: false),
                    nacionalidad = table.Column<string>(type: "TEXT", nullable: true),
                    fecha_nacimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    biografia = table.Column<string>(type: "TEXT", nullable: true),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    creado_en = table.Column<DateTime>(type: "TEXT", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "directores",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", nullable: false),
                    nacionalidad = table.Column<string>(type: "TEXT", nullable: true),
                    fecha_nacimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    biografia = table.Column<string>(type: "TEXT", nullable: true),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    creado_en = table.Column<DateTime>(type: "TEXT", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_directores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plataformas",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", nullable: false),
                    sitio_web = table.Column<string>(type: "TEXT", nullable: true),
                    precio_mensual = table.Column<decimal>(type: "TEXT", nullable: true),
                    pais_disponible = table.Column<string>(type: "TEXT", nullable: true),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    creado_en = table.Column<DateTime>(type: "TEXT", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plataformas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "productoras",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", nullable: false),
                    pais = table.Column<string>(type: "TEXT", nullable: true),
                    anio_fundacion = table.Column<int>(type: "INTEGER", nullable: true),
                    sitio_web = table.Column<string>(type: "TEXT", nullable: true),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    creado_en = table.Column<DateTime>(type: "TEXT", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productoras", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_plataformas_nombre",
                table: "plataformas",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_productoras_nombre",
                table: "productoras",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "actores");

            migrationBuilder.DropTable(
                name: "directores");

            migrationBuilder.DropTable(
                name: "plataformas");

            migrationBuilder.DropTable(
                name: "productoras");
        }
    }
}
