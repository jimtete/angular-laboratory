using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_StoryQuestBeatConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoryBeatQuestTasks",
                columns: table => new
                {
                    story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quest_task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    linked_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryBeatQuestTasks", x => new { x.story_beat_id, x.quest_task_id });
                    table.ForeignKey(
                        name: "FK_StoryBeatQuestTasks_CampaignQuestTasks_quest_task_id",
                        column: x => x.quest_task_id,
                        principalTable: "CampaignQuestTasks",
                        principalColumn: "quest_task_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoryBeatQuestTasks_StoryBeats_story_beat_id",
                        column: x => x.story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryBeatQuestTasks_quest_task_id",
                table: "StoryBeatQuestTasks",
                column: "quest_task_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryBeatQuestTasks");
        }
    }
}
