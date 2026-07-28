using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTables_PresentationRelatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignPresentations",
                columns: table => new
                {
                    campaign_presentation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    campaign_session_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    active_story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    current_story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    ended_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPresentations", x => x.campaign_presentation_id);
                    table.ForeignKey(
                        name: "FK_CampaignPresentations_CampaignSessions_campaign_session_id",
                        column: x => x.campaign_session_id,
                        principalTable: "CampaignSessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignPresentations_StoryBeats_current_story_beat_id",
                        column: x => x.current_story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignPresentations_StoryBlocks_active_story_block_id",
                        column: x => x.active_story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignPresentationEntries",
                columns: table => new
                {
                    campaign_presentation_entry_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    campaign_presentation_id = table.Column<int>(type: "int", nullable: false),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    entry_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPresentationEntries", x => x.campaign_presentation_entry_id);
                    table.ForeignKey(
                        name: "FK_CampaignPresentationEntries_CampaignPresentations_campaign_presentation_id",
                        column: x => x.campaign_presentation_id,
                        principalTable: "CampaignPresentations",
                        principalColumn: "campaign_presentation_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignPresentationEntries_StoryBeats_story_beat_id",
                        column: x => x.story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignPresentationEntries_StoryBlocks_story_block_id",
                        column: x => x.story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentationEntries_campaign_presentation_id_created_at",
                table: "CampaignPresentationEntries",
                columns: new[] { "campaign_presentation_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentationEntries_campaign_presentation_id_sequence",
                table: "CampaignPresentationEntries",
                columns: new[] { "campaign_presentation_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentationEntries_story_beat_id",
                table: "CampaignPresentationEntries",
                column: "story_beat_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentationEntries_story_block_id",
                table: "CampaignPresentationEntries",
                column: "story_block_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentations_active_story_block_id",
                table: "CampaignPresentations",
                column: "active_story_block_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentations_campaign_session_id",
                table: "CampaignPresentations",
                column: "campaign_session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentations_current_story_beat_id",
                table: "CampaignPresentations",
                column: "current_story_beat_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPresentations_status_updated_at",
                table: "CampaignPresentations",
                columns: new[] { "status", "updated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignPresentationEntries");

            migrationBuilder.DropTable(
                name: "CampaignPresentations");
        }
    }
}
