using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerParticipationValuesAndGetRoleplayingNpcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'gameplay')
                    EXEC(N'CREATE SCHEMA [gameplay]');
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE [gameplay].[GetRoleplayingStoryBeatNpcsByCampaignId]
                    @CampaignId uniqueidentifier
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        block.campaign_id AS CampaignId,
                        block.story_block_id AS StoryBlockId,
                        block.title AS StoryBlockTitle,
                        beat.story_beat_id AS StoryBeatId,
                        beat.title AS StoryBeatTitle,
                        beat.order_index AS StoryBeatOrderIndex,
                        COALESCE(
                            NULLIF(JSON_VALUE(npc.[value], '$.Tag'), N''),
                            NULLIF(JSON_VALUE(npc.[value], '$.Name'), N''),
                            NULLIF(JSON_VALUE(npc.[value], '$.Description'), N''),
                            N''
                        ) AS NpcTag,
                        COALESCE(JSON_VALUE(npc.[value], '$.Name'), N'') AS NpcName,
                        COALESCE(JSON_VALUE(npc.[value], '$.Description'), N'') AS NpcDescription
                    FROM [dbo].[StoryBeats] AS beat
                    INNER JOIN [dbo].[StoryBlocks] AS block
                        ON block.story_block_id = beat.story_block_id
                    CROSS APPLY OPENJSON(
                        CASE
                            WHEN ISJSON(beat.roleplaying) = 1 THEN beat.roleplaying
                            ELSE N'{}'
                        END,
                        '$.Npcs') AS npc
                    WHERE block.campaign_id = @CampaignId
                        AND beat.story_beat_type = N'Roleplaying'
                        AND beat.roleplaying IS NOT NULL
                        AND ISJSON(beat.roleplaying) = 1
                    ORDER BY
                        block.title ASC,
                        block.story_block_id ASC,
                        beat.order_index ASC,
                        beat.story_beat_id ASC,
                        npc.[key] ASC;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP PROCEDURE IF EXISTS [gameplay].[GetRoleplayingStoryBeatNpcsByCampaignId];
                """);
        }
    }
}
