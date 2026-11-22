using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablasCatalogoAbraham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", nullable: false),
                    descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    creado_en = table.Column<DateTime>(type: "TEXT", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "peliculas",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    titulo = table.Column<string>(type: "TEXT", nullable: false),
                    sinopsis = table.Column<string>(type: "TEXT", nullable: true),
                    anio_estreno = table.Column<int>(type: "INTEGER", nullable: false),
                    duracion_min = table.Column<int>(type: "INTEGER", nullable: false),
                    clasificacion = table.Column<string>(type: "TEXT", nullable: true),
                    poster_url = table.Column<string>(type: "TEXT", nullable: true),
                    categoria_id = table.Column<int>(type: "INTEGER", nullable: true),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    creado_en = table.Column<DateTime>(type: "TEXT", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_peliculas", x => x.id);
                    table.ForeignKey(
                        name: "FK_peliculas_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "series",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    titulo = table.Column<string>(type: "TEXT", nullable: false),
                    sinopsis = table.Column<string>(type: "TEXT", nullable: true),
                    anio_inicio = table.Column<int>(type: "INTEGER", nullable: false),
                    en_emision = table.Column<bool>(type: "INTEGER", nullable: false),
                    poster_url = table.Column<string>(type: "TEXT", nullable: true),
                    categoria_id = table.Column<int>(type: "INTEGER", nullable: true),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    creado_en = table.Column<DateTime>(type: "TEXT", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_series", x => x.id);
                    table.ForeignKey(
                        name: "FK_series_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "temporadas",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    numero = table.Column<int>(type: "INTEGER", nullable: false),
                    titulo = table.Column<string>(type: "TEXT", nullable: false),
                    anio_estreno = table.Column<int>(type: "INTEGER", nullable: false),
                    cantidad_episodios = table.Column<int>(type: "INTEGER", nullable: false),
                    serie_id = table.Column<int>(type: "INTEGER", nullable: false),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    creado_en = table.Column<DateTime>(type: "TEXT", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temporadas", x => x.id);
                    table.ForeignKey(
                        name: "FK_temporadas_series_serie_id",
                        column: x => x.serie_id,
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                table: "categorias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_peliculas_categoria_id",
                table: "peliculas",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_series_categoria_id",
                table: "series",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_temporadas_serie_id",
                table: "temporadas",
                column: "serie_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "peliculas");

            migrationBuilder.DropTable(
                name: "temporadas");

            migrationBuilder.DropTable(
                name: "series");

            migrationBuilder.DropTable(
                name: "categorias");
        }
    }
}
