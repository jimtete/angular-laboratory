using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_MusicFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicFiles",
                columns: table => new
                {
                    music_file_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    uploaded_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    original_file_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    stored_file_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    storage_path = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    duration_milliseconds = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicFiles", x => x.music_file_id);
                    table.ForeignKey(
                        name: "FK_MusicFiles_Users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicFiles_storage_path",
                table: "MusicFiles",
                column: "storage_path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicFiles_uploaded_by_user_id",
                table: "MusicFiles",
                column: "uploaded_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicFiles");
        }
    }
}
