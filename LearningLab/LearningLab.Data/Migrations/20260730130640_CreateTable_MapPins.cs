using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_MapPins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "image_height_pixels",
                table: "Maps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "image_width_pixels",
                table: "Maps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MapPins",
                columns: table => new
                {
                    map_pin_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    map_id = table.Column<int>(type: "int", nullable: false),
                    x_coordinate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    y_coordinate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: false, defaultValue: ""),
                    target_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    target_id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapPins", x => x.map_pin_id);
                    table.ForeignKey(
                        name: "FK_MapPins_Maps_map_id",
                        column: x => x.map_id,
                        principalTable: "Maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapPins_map_id",
                table: "MapPins",
                column: "map_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapPins");

            migrationBuilder.DropColumn(
                name: "image_height_pixels",
                table: "Maps");

            migrationBuilder.DropColumn(
                name: "image_width_pixels",
                table: "Maps");
        }
    }
}
