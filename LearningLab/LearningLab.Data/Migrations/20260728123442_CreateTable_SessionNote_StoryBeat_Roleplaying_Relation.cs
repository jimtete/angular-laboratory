using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_SessionNote_StoryBeat_Roleplaying_Relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionNoteStoryBeatReferences",
                columns: table => new
                {
                    session_note_story_beat_reference_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_note_id = table.Column<int>(type: "int", nullable: false),
                    story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reference_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    reference_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    npc_tag = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    content_snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionNoteStoryBeatReferences", x => x.session_note_story_beat_reference_id);
                    table.ForeignKey(
                        name: "FK_SessionNoteStoryBeatReferences_SessionNotes_session_note_id",
                        column: x => x.session_note_id,
                        principalTable: "SessionNotes",
                        principalColumn: "session_note_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionNoteStoryBeatReferences_StoryBeats_story_beat_id",
                        column: x => x.story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteStoryBeatReferences_session_note_id",
                table: "SessionNoteStoryBeatReferences",
                column: "session_note_id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteStoryBeatReferences_story_beat_id",
                table: "SessionNoteStoryBeatReferences",
                column: "story_beat_id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteStoryBeatReferences_story_beat_id_reference_type_reference_id",
                table: "SessionNoteStoryBeatReferences",
                columns: new[] { "story_beat_id", "reference_type", "reference_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionNoteStoryBeatReferences");
        }
    }
}
