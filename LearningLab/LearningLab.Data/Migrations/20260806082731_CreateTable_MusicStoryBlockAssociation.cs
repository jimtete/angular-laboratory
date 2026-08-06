using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_MusicStoryBlockAssociation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoryBlockMusicFiles",
                columns: table => new
                {
                    story_block_music_file_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    music_file_id = table.Column<int>(type: "int", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    loop = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    continue_across_story_blocks = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryBlockMusicFiles", x => x.story_block_music_file_id);
                    table.ForeignKey(
                        name: "FK_StoryBlockMusicFiles_MusicFiles_music_file_id",
                        column: x => x.music_file_id,
                        principalTable: "MusicFiles",
                        principalColumn: "music_file_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoryBlockMusicFiles_StoryBeats_story_beat_id",
                        column: x => x.story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id");
                    table.ForeignKey(
                        name: "FK_StoryBlockMusicFiles_StoryBlocks_story_block_id",
                        column: x => x.story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMusicFiles_music_file_id",
                table: "StoryBlockMusicFiles",
                column: "music_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMusicFiles_story_beat_id",
                table: "StoryBlockMusicFiles",
                column: "story_beat_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMusicFiles_story_block_id",
                table: "StoryBlockMusicFiles",
                column: "story_block_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMusicFiles_story_block_id_music_file_id",
                table: "StoryBlockMusicFiles",
                columns: new[] { "story_block_id", "music_file_id" },
                unique: true,
                filter: "[story_beat_id] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlockMusicFiles_story_block_id_story_beat_id_music_file_id",
                table: "StoryBlockMusicFiles",
                columns: new[] { "story_block_id", "story_beat_id", "music_file_id" },
                unique: true,
                filter: "[story_beat_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryBlockMusicFiles");
        }
    }
}
