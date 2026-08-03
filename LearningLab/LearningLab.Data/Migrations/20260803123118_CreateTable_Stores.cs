using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_Stores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoreEntries",
                columns: table => new
                {
                    store_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    store_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    store_location = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    store_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    store_description = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreEntries", x => x.store_id);
                    table.ForeignKey(
                        name: "FK_StoreEntries_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreItems",
                columns: table => new
                {
                    store_item_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    store_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    times_sold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    item_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    item_description = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    item_price = table.Column<int>(type: "int", nullable: false),
                    item_price_discount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    item_price_percentage_discount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreItems", x => x.store_item_id);
                    table.ForeignKey(
                        name: "FK_StoreItems_StoreEntries_store_id",
                        column: x => x.store_id,
                        principalTable: "StoreEntries",
                        principalColumn: "store_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreEntries_campaign_id",
                table: "StoreEntries",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoreItems_store_id",
                table: "StoreItems",
                column: "store_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreItems");

            migrationBuilder.DropTable(
                name: "StoreEntries");
        }
    }
}
