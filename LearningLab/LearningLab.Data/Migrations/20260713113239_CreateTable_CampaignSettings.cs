using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_CampaignSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignSettings",
                columns: table => new
                {
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    max_number_of_players = table.Column<int>(
                        type: "int",
                        nullable: false,
                        defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignSettings", x => x.campaign_id);
                    table.ForeignKey(
                        name: "FK_CampaignSettings_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO [dbo].[CampaignSettings] ([campaign_id], [max_number_of_players])
                SELECT
                    [campaign_id],
                    1
                FROM [dbo].[Campaigns] AS [campaign]
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [dbo].[CampaignSettings] AS [settings]
                    WHERE [settings].[campaign_id] = [campaign].[campaign_id]
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignSettings");
        }
    }
}
