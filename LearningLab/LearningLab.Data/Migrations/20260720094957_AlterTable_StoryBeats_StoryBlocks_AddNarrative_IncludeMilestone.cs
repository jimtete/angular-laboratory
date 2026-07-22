using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_StoryBeats_StoryBlocks_AddNarrative_IncludeMilestone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "narrative",
                table: "StoryBeats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StoryBlockMilestones",
                columns: table => new
                {
                    story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_milestone_id = table.Column<int>(type: "int", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryBlockMilestones", x => new { x.story_block_id, x.campaign_milestone_id });
                    table.ForeignKey(
                        name: "FK_StoryBlockMilestones_CampaignMilestones_campaign_milestone_id",
                        column: x => x.campaign_milestone_id,
                        principalTable: "CampaignMilestones",
                        principalColumn: "campaign_milestone_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoryBlockMilestones_StoryBlocks_story_block_id",
                        column: x => x.story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMilestones_campaign_milestone_id",
                table: "StoryBlockMilestones",
                column: "campaign_milestone_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMilestones_story_block_id_order_index",
                table: "StoryBlockMilestones",
                columns: new[] { "story_block_id", "order_index" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryBlockMilestones");

            migrationBuilder.DropColumn(
                name: "narrative",
                table: "StoryBeats");
        }
    }
}
