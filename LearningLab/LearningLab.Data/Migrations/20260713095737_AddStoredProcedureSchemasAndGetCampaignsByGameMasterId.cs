using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedureSchemasAndGetCampaignsByGameMasterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'platform')
                    EXEC(N'CREATE SCHEMA [platform]');
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'gameplay')
                    EXEC(N'CREATE SCHEMA [gameplay]');
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE [platform].[GetCampaignsByGameMasterId]
                    @GameMasterId uniqueidentifier
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        c.campaign_id AS CampaignId,
                        c.game_master_id AS GameMasterId,
                        u.username AS GameMasterUsername,
                        c.campaign_name AS CampaignName,
                        c.version AS Version,
                        c.campaign_picture_url AS CampaignPictureUrl,
                        c.date_created AS DateCreated
                    FROM [dbo].[Campaigns] AS c
                    INNER JOIN [dbo].[Users] AS u
                        ON u.user_id = c.game_master_id
                    WHERE c.game_master_id = @GameMasterId
                    ORDER BY c.date_created DESC;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP PROCEDURE IF EXISTS [platform].[GetCampaignsByGameMasterId];
                """);

            migrationBuilder.Sql("""
                DROP SCHEMA IF EXISTS [gameplay];
                """);

            migrationBuilder.Sql("""
                DROP SCHEMA IF EXISTS [platform];
                """);
        }
    }
}
