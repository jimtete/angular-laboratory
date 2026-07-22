using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_StoryBlock_StoryBeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoryBlocks",
                columns: table => new
                {
                    story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryBlocks", x => x.story_block_id);
                    table.ForeignKey(
                        name: "FK_StoryBlocks_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryBeats",
                columns: table => new
                {
                    story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_beat_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    information = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryBeats", x => x.story_beat_id);
                    table.ForeignKey(
                        name: "FK_StoryBeats_StoryBlocks_story_block_id",
                        column: x => x.story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeats_story_block_id",
                table: "StoryBeats",
                column: "story_block_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlocks_campaign_id_order_index",
                table: "StoryBlocks",
                columns: new[] { "campaign_id", "order_index" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryBeats");

            migrationBuilder.DropTable(
                name: "StoryBlocks");
        }
    }
}
