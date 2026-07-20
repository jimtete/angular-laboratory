using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_AddCampaignQuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignQuests",
                columns: table => new
                {
                    quest_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quest_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false, defaultValue: ""),
                    given_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: ""),
                    reward = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false, defaultValue: ""),
                    completed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignQuests", x => x.quest_id);
                    table.ForeignKey(
                        name: "FK_CampaignQuests_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignQuestTasks",
                columns: table => new
                {
                    quest_task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false, defaultValue: ""),
                    date_completed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    quest_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignQuestTasks", x => x.quest_task_id);
                    table.ForeignKey(
                        name: "FK_CampaignQuestTasks_CampaignQuests_quest_id",
                        column: x => x.quest_id,
                        principalTable: "CampaignQuests",
                        principalColumn: "quest_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignQuests_campaign_id_quest_type_completed_at",
                table: "CampaignQuests",
                columns: new[] { "campaign_id", "quest_type", "completed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignQuests_campaign_id_title",
                table: "CampaignQuests",
                columns: new[] { "campaign_id", "title" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignQuestTasks_quest_id_date_completed",
                table: "CampaignQuestTasks",
                columns: new[] { "quest_id", "date_completed" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignQuestTasks_quest_id_title",
                table: "CampaignQuestTasks",
                columns: new[] { "quest_id", "title" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignQuestTasks");

            migrationBuilder.DropTable(
                name: "CampaignQuests");
        }
    }
}
