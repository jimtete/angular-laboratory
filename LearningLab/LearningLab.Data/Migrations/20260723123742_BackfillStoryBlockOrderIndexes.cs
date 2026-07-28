using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillStoryBlockOrderIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_StoryBlocks_campaign_id_order_index'
                        AND object_id = OBJECT_ID(N'[dbo].[StoryBlocks]')
                )
                BEGIN
                    DROP INDEX [IX_StoryBlocks_campaign_id_order_index] ON [dbo].[StoryBlocks];
                END;

                IF COL_LENGTH(N'dbo.StoryBlocks', N'order_index') IS NOT NULL
                BEGIN
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
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_StoryBlocks_campaign_id_order_index'
                        AND object_id = OBJECT_ID(N'[dbo].[StoryBlocks]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_StoryBlocks_campaign_id_order_index]
                    ON [dbo].[StoryBlocks] ([campaign_id], [order_index]);
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
