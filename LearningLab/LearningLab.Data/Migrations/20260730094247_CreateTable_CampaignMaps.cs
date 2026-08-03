using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_CampaignMaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Maps",
                columns: table => new
                {
                    map_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    asset_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: false, defaultValue: ""),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maps", x => x.map_id);
                    table.ForeignKey(
                        name: "FK_Maps_Assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "Assets",
                        principalColumn: "asset_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MapCampaigns",
                columns: table => new
                {
                    map_id = table.Column<int>(type: "int", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_added = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapCampaigns", x => new { x.map_id, x.campaign_id });
                    table.ForeignKey(
                        name: "FK_MapCampaigns_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MapCampaigns_Maps_map_id",
                        column: x => x.map_id,
                        principalTable: "Maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapCampaigns_campaign_id",
                table: "MapCampaigns",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_Maps_asset_id",
                table: "Maps",
                column: "asset_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maps_name",
                table: "Maps",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapCampaigns");

            migrationBuilder.DropTable(
                name: "Maps");
        }
    }
}
