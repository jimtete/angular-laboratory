using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_StoryBeatIndexPathRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoryBeatIndexPathRules",
                columns: table => new
                {
                    story_beat_index_path_rule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    relation_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    is_required = table.Column<bool>(type: "bit", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryBeatIndexPathRules", x => x.story_beat_index_path_rule_id);
                    table.ForeignKey(
                        name: "FK_StoryBeatIndexPathRules_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoryBeatIndexPathRules_StoryBlocks_story_block_id",
                        column: x => x.story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeatIndexPathRules_campaign_id",
                table: "StoryBeatIndexPathRules",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeatIndexPathRules_campaign_id_story_block_id_order_index",
                table: "StoryBeatIndexPathRules",
                columns: new[] { "campaign_id", "story_block_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeatIndexPathRules_story_block_id",
                table: "StoryBeatIndexPathRules",
                column: "story_block_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryBeatIndexPathRules");
        }
    }
}
