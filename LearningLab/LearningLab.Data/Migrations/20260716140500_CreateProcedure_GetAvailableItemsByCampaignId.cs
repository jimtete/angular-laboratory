using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LearningLabContext))]
    [Migration("20260716140500_CreateProcedure_GetAvailableItemsByCampaignId")]
    public partial class CreateProcedure_GetAvailableItemsByCampaignId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'gameplay')
                    EXEC(N'CREATE SCHEMA [gameplay]');
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE [gameplay].[GetAvailableItemsByCampaignId]
                    @CampaignId uniqueidentifier
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        a.asset_id AS Id,
                        a.parent_asset_id AS ParentAssetId,
                        a.asset_type AS AssetType,
                        a.name AS Name,
                        a.description AS Description,
                        a.item_type AS ItemType,
                        a.campaign_ids AS CampaignIds,
                        a.created_at AS CreatedAt,
                        a.updated_at AS UpdatedAt
                    FROM [dbo].[Assets] AS a
                    WHERE a.asset_type = N'Items'
                        AND (
                            a.campaign_ids IS NULL
                            OR EXISTS
                            (
                                SELECT 1
                                FROM OPENJSON(a.campaign_ids) AS campaign_id
                                WHERE TRY_CONVERT(uniqueidentifier, campaign_id.[value]) = @CampaignId
                            )
                        )
                    ORDER BY
                        a.name ASC,
                        a.asset_id ASC;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP PROCEDURE IF EXISTS [gameplay].[GetAvailableItemsByCampaignId];
                """);
        }
    }
}
