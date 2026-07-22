using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_StoryBeats_AddIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoryBlockMilestones_campaign_milestone_id",
                table: "StoryBlockMilestones");

            migrationBuilder.AddColumn<int>(
                name: "order_index",
                table: "StoryBeats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                WITH OrderedStoryBeats AS
                (
                    SELECT
                        story_beat_id,
                        ROW_NUMBER() OVER (
                            PARTITION BY story_block_id
                            ORDER BY story_beat_id
                        ) AS order_index
                    FROM StoryBeats
                )
                UPDATE beat
                SET order_index = ordered.order_index
                FROM StoryBeats AS beat
                INNER JOIN OrderedStoryBeats AS ordered
                    ON ordered.story_beat_id = beat.story_beat_id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMilestones_campaign_milestone_id",
                table: "StoryBlockMilestones",
                column: "campaign_milestone_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeats_story_block_id_order_index",
                table: "StoryBeats",
                columns: new[] { "story_block_id", "order_index" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoryBlockMilestones_campaign_milestone_id",
                table: "StoryBlockMilestones");

            migrationBuilder.DropIndex(
                name: "IX_StoryBeats_story_block_id_order_index",
                table: "StoryBeats");

            migrationBuilder.DropColumn(
                name: "order_index",
                table: "StoryBeats");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMilestones_campaign_milestone_id",
                table: "StoryBlockMilestones",
                column: "campaign_milestone_id");
        }
    }
}
