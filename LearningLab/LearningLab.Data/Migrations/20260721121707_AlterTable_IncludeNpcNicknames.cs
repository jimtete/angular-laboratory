using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_IncludeNpcNicknames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "CampaignNpcs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE [dbo].[CampaignNpcs]
                SET [display_name] = [name]
                WHERE [display_name] = N'';
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
                        npc.display_name AS DisplayName,
                        npc.description AS Description,
                        npc.created_at AS CreatedAt,
                        npc.updated_at AS UpdatedAt
                    FROM [dbo].[CampaignNpcs] AS npc
                    WHERE npc.campaign_id = @CampaignId
                    ORDER BY
                        npc.display_name ASC,
                        npc.tag ASC;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "display_name",
                table: "CampaignNpcs");
        }
    }
}
