using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_StoryBeats_AddMilestoneLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "campaign_milestone_id",
                table: "StoryBeats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeats_campaign_milestone_id",
                table: "StoryBeats",
                column: "campaign_milestone_id",
                unique: true,
                filter: "[campaign_milestone_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_StoryBeats_CampaignMilestones_campaign_milestone_id",
                table: "StoryBeats",
                column: "campaign_milestone_id",
                principalTable: "CampaignMilestones",
                principalColumn: "campaign_milestone_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoryBeats_CampaignMilestones_campaign_milestone_id",
                table: "StoryBeats");

            migrationBuilder.DropIndex(
                name: "IX_StoryBeats_campaign_milestone_id",
                table: "StoryBeats");

            migrationBuilder.DropColumn(
                name: "campaign_milestone_id",
                table: "StoryBeats");
        }
    }
}
