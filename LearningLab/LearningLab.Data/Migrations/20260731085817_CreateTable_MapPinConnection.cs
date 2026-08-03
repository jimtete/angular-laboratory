using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_MapPinConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MapPinConnections",
                columns: table => new
                {
                    map_pin_connection_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    map_id = table.Column<int>(type: "int", nullable: false),
                    map_pin_a_id = table.Column<int>(type: "int", nullable: false),
                    map_pin_b_id = table.Column<int>(type: "int", nullable: false),
                    distance_value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    distance_unit = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapPinConnections", x => x.map_pin_connection_id);
                    table.ForeignKey(
                        name: "FK_MapPinConnections_MapPins_map_pin_a_id",
                        column: x => x.map_pin_a_id,
                        principalTable: "MapPins",
                        principalColumn: "map_pin_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapPinConnections_MapPins_map_pin_b_id",
                        column: x => x.map_pin_b_id,
                        principalTable: "MapPins",
                        principalColumn: "map_pin_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapPinConnections_Maps_map_id",
                        column: x => x.map_id,
                        principalTable: "Maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapPinConnections_map_id",
                table: "MapPinConnections",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_MapPinConnections_map_id_map_pin_a_id_map_pin_b_id",
                table: "MapPinConnections",
                columns: new[] { "map_id", "map_pin_a_id", "map_pin_b_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapPinConnections_map_pin_a_id",
                table: "MapPinConnections",
                column: "map_pin_a_id");

            migrationBuilder.CreateIndex(
                name: "IX_MapPinConnections_map_pin_b_id",
                table: "MapPinConnections",
                column: "map_pin_b_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapPinConnections");
        }
    }
}
