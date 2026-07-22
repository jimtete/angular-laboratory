using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_CampaignNpcs_UpdateNpcProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignNpcs",
                columns: table => new
                {
                    campaign_npc_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tag = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(
                        type: "nvarchar(2048)",
                        maxLength: 2048,
                        nullable: false,
                        defaultValue: ""),
                    created_at = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false,
                        defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false,
                        defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignNpcs", x => x.campaign_npc_id);
                    table.ForeignKey(
                        name: "FK_CampaignNpcs_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignNpcs_campaign_id_tag",
                table: "CampaignNpcs",
                columns: new[] { "campaign_id", "tag" },
                unique: true);

            migrationBuilder.Sql("""
                WITH RawNpcs AS
                (
                    SELECT
                        block.campaign_id AS CampaignId,
                        LEFT(
                            LOWER(COALESCE(
                                NULLIF(JSON_VALUE(npc.[value], '$.Tag'), N''),
                                NULLIF(JSON_VALUE(npc.[value], '$.Name'), N''),
                                NULLIF(JSON_VALUE(npc.[value], '$.Description'), N'')
                            )),
                            128) AS Tag,
                        LEFT(COALESCE(JSON_VALUE(npc.[value], '$.Name'), N'Unknown NPC'), 256) AS Name,
                        LEFT(COALESCE(JSON_VALUE(npc.[value], '$.Description'), N''), 2048) AS Description,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY
                                block.campaign_id,
                                LEFT(
                                    LOWER(COALESCE(
                                        NULLIF(JSON_VALUE(npc.[value], '$.Tag'), N''),
                                        NULLIF(JSON_VALUE(npc.[value], '$.Name'), N''),
                                        NULLIF(JSON_VALUE(npc.[value], '$.Description'), N'')
                                    )),
                                    128)
                            ORDER BY
                                block.story_block_id,
                                beat.order_index,
                                beat.story_beat_id,
                                TRY_CONVERT(int, npc.[key])
                        ) AS RowNumber
                    FROM [dbo].[StoryBeats] AS beat
                    INNER JOIN [dbo].[StoryBlocks] AS block
                        ON block.story_block_id = beat.story_block_id
                    CROSS APPLY OPENJSON(
                        CASE
                            WHEN ISJSON(beat.roleplaying) = 1 THEN beat.roleplaying
                            ELSE N'{}'
                        END,
                        '$.Npcs') AS npc
                    WHERE beat.story_beat_type = N'Roleplaying'
                        AND beat.roleplaying IS NOT NULL
                        AND ISJSON(beat.roleplaying) = 1
                )
                INSERT INTO [dbo].[CampaignNpcs]
                (
                    campaign_npc_id,
                    campaign_id,
                    tag,
                    name,
                    description
                )
                SELECT
                    NEWID(),
                    CampaignId,
                    Tag,
                    Name,
                    Description
                FROM RawNpcs
                WHERE RowNumber = 1
                    AND Tag IS NOT NULL
                    AND NOT EXISTS
                    (
                        SELECT 1
                        FROM [dbo].[CampaignNpcs] AS existing
                        WHERE existing.campaign_id = RawNpcs.CampaignId
                            AND existing.tag = RawNpcs.Tag
                    );
                """);

            migrationBuilder.Sql("""
                WITH RoleplayingBeatReferences AS
                (
                    SELECT
                        beat.story_beat_id AS StoryBeatId,
                        (
                            SELECT
                                LEFT(
                                    LOWER(COALESCE(
                                        NULLIF(JSON_VALUE(npc.[value], '$.Tag'), N''),
                                        NULLIF(JSON_VALUE(npc.[value], '$.Name'), N''),
                                        NULLIF(JSON_VALUE(npc.[value], '$.Description'), N'')
                                    )),
                                    128) AS NpcTag
                            FROM OPENJSON(
                                CASE
                                    WHEN ISJSON(beat.roleplaying) = 1 THEN beat.roleplaying
                                    ELSE N'{}'
                                END,
                                '$.Npcs') AS npc
                            WHERE COALESCE(
                                NULLIF(JSON_VALUE(npc.[value], '$.Tag'), N''),
                                NULLIF(JSON_VALUE(npc.[value], '$.Name'), N''),
                                NULLIF(JSON_VALUE(npc.[value], '$.Description'), N'')
                            ) IS NOT NULL
                            FOR JSON PATH
                        ) AS NpcReferences
                    FROM [dbo].[StoryBeats] AS beat
                    WHERE beat.story_beat_type = N'Roleplaying'
                        AND beat.roleplaying IS NOT NULL
                        AND ISJSON(beat.roleplaying) = 1
                )
                UPDATE beat
                SET roleplaying = JSON_MODIFY(
                    JSON_MODIFY(
                        beat.roleplaying,
                        '$.NpcReferences',
                        JSON_QUERY(COALESCE(reference_data.NpcReferences, N'[]'))),
                    '$.Npcs',
                    NULL)
                FROM [dbo].[StoryBeats] AS beat
                INNER JOIN RoleplayingBeatReferences AS reference_data
                    ON reference_data.StoryBeatId = beat.story_beat_id;
                """);

            migrationBuilder.Sql("""
                DROP PROCEDURE IF EXISTS [gameplay].[GetRoleplayingStoryBeatNpcsByCampaignId];
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE [gameplay].[GetCampaignNpcsByCampaignId]
                    @CampaignId uniqueidentifier
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        npc.campaign_npc_id AS CampaignNpcId,
                        npc.campaign_id AS CampaignId,
                        npc.tag AS Tag,
                        npc.name AS Name,
                        npc.description AS Description,
                        npc.created_at AS CreatedAt,
                        npc.updated_at AS UpdatedAt
                    FROM [dbo].[CampaignNpcs] AS npc
                    WHERE npc.campaign_id = @CampaignId
                    ORDER BY
                        npc.name ASC,
                        npc.tag ASC;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH RoleplayingBeatNpcs AS
                (
                    SELECT
                        beat.story_beat_id AS StoryBeatId,
                        (
                            SELECT
                                npc.tag AS Tag,
                                npc.name AS Name,
                                npc.description AS Description
                            FROM OPENJSON(
                                CASE
                                    WHEN ISJSON(beat.roleplaying) = 1 THEN beat.roleplaying
                                    ELSE N'{}'
                                END,
                                '$.NpcReferences') AS npc_reference
                            INNER JOIN [dbo].[StoryBlocks] AS block
                                ON block.story_block_id = beat.story_block_id
                            INNER JOIN [dbo].[CampaignNpcs] AS npc
                                ON npc.campaign_id = block.campaign_id
                                AND npc.tag = JSON_VALUE(npc_reference.[value], '$.NpcTag')
                            FOR JSON PATH
                        ) AS Npcs
                    FROM [dbo].[StoryBeats] AS beat
                    WHERE beat.story_beat_type = N'Roleplaying'
                        AND beat.roleplaying IS NOT NULL
                        AND ISJSON(beat.roleplaying) = 1
                )
                UPDATE beat
                SET roleplaying = JSON_MODIFY(
                    JSON_MODIFY(
                        beat.roleplaying,
                        '$.Npcs',
                        JSON_QUERY(COALESCE(npcs.Npcs, N'[]'))),
                    '$.NpcReferences',
                    NULL)
                FROM [dbo].[StoryBeats] AS beat
                INNER JOIN RoleplayingBeatNpcs AS npcs
                    ON npcs.StoryBeatId = beat.story_beat_id;
                """);

            migrationBuilder.Sql("""
                DROP PROCEDURE IF EXISTS [gameplay].[GetCampaignNpcsByCampaignId];
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

            migrationBuilder.DropTable(
                name: "CampaignNpcs");
        }
    }
}
