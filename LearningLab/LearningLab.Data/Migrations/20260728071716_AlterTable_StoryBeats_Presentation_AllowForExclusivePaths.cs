using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_StoryBeats_Presentation_AllowForExclusivePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignPresentationStoryBeatSelections",
                columns: table => new
                {
                    campaign_presentation_story_beat_selection_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    campaign_presentation_id = table.Column<int>(type: "int", nullable: false),
                    story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    selected_secondary_order_index = table.Column<int>(type: "int", nullable: false),
                    selected_story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    selected_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPresentationStoryBeatSelections", x => x.campaign_presentation_story_beat_selection_id);
                    table.ForeignKey(
                        name: "FK_CampaignPresentationStoryBeatSelections_CampaignPresentations_campaign_presentation_id",
                        column: x => x.campaign_presentation_id,
                        principalTable: "CampaignPresentations",
                        principalColumn: "campaign_presentation_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignPresentationStoryBeatSelections_StoryBeats_selected_story_beat_id",
                        column: x => x.selected_story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignPresentationStoryBeatSelections_StoryBlocks_story_block_id",
                        column: x => x.story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentationStoryBeatSelections_campaign_presentation_id_story_block_id_order_index",
                table: "CampaignPresentationStoryBeatSelections",
                columns: new[] { "campaign_presentation_id", "story_block_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentationStoryBeatSelections_selected_story_beat_id",
                table: "CampaignPresentationStoryBeatSelections",
                column: "selected_story_beat_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentationStoryBeatSelections_story_block_id",
                table: "CampaignPresentationStoryBeatSelections",
                column: "story_block_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignPresentationStoryBeatSelections");
        }
    }
}
