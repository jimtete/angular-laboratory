using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_StoryBlocks_IndexOrderThem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "order_index",
                table: "StoryBlocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                WITH OrderedStoryBlocks AS
                (
                    SELECT
                        story_block_id,
                        ROW_NUMBER() OVER (
                            PARTITION BY campaign_id
                            ORDER BY story_block_id
                        ) AS order_index
                    FROM [dbo].[StoryBlocks]
                )
                UPDATE storyBlock
                SET order_index = ordered.order_index
                FROM [dbo].[StoryBlocks] storyBlock
                INNER JOIN OrderedStoryBlocks ordered
                    ON ordered.story_block_id = storyBlock.story_block_id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlocks_campaign_id_order_index",
                table: "StoryBlocks",
                columns: new[] { "campaign_id", "order_index" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoryBlocks_campaign_id_order_index",
                table: "StoryBlocks");

            migrationBuilder.DropColumn(
                name: "order_index",
                table: "StoryBlocks");
        }
    }
}
