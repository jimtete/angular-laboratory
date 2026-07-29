using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_SessionNotes_AllowStoryBeatNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "story_beat_id",
                table: "SessionNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotes_story_beat_id",
                table: "SessionNotes",
                column: "story_beat_id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionNotes_StoryBeats_story_beat_id",
                table: "SessionNotes",
                column: "story_beat_id",
                principalTable: "StoryBeats",
                principalColumn: "story_beat_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionNotes_StoryBeats_story_beat_id",
                table: "SessionNotes");

            migrationBuilder.DropIndex(
                name: "IX_SessionNotes_story_beat_id",
                table: "SessionNotes");

            migrationBuilder.DropColumn(
                name: "story_beat_id",
                table: "SessionNotes");
        }
    }
}
