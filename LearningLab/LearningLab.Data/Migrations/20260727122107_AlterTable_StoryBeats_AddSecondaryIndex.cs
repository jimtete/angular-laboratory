using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_StoryBeats_AddSecondaryIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoryBeats_story_block_id_order_index",
                table: "StoryBeats");

            migrationBuilder.AddColumn<int>(
                name: "secondary_order_index",
                table: "StoryBeats",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeats_story_block_id_order_index_secondary_order_index",
                table: "StoryBeats",
                columns: new[] { "story_block_id", "order_index", "secondary_order_index" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoryBeats_story_block_id_order_index_secondary_order_index",
                table: "StoryBeats");

            migrationBuilder.DropColumn(
                name: "secondary_order_index",
                table: "StoryBeats");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeats_story_block_id_order_index",
                table: "StoryBeats",
                columns: new[] { "story_block_id", "order_index" },
                unique: true);
        }
    }
}
