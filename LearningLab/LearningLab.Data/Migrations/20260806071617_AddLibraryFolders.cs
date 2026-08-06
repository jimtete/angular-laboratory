using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_folder_id",
                table: "MusicFiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LibraryFolders",
                columns: table => new
                {
                    library_folder_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_folder_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryFolders", x => x.library_folder_id);
                    table.ForeignKey(
                        name: "FK_LibraryFolders_LibraryFolders_parent_folder_id",
                        column: x => x.parent_folder_id,
                        principalTable: "LibraryFolders",
                        principalColumn: "library_folder_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LibraryFolders_Users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicFiles_parent_folder_id",
                table: "MusicFiles",
                column: "parent_folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFolders_created_by_user_id_parent_folder_id_name",
                table: "LibraryFolders",
                columns: new[] { "created_by_user_id", "parent_folder_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFolders_parent_folder_id",
                table: "LibraryFolders",
                column: "parent_folder_id");

            migrationBuilder.AddForeignKey(
                name: "FK_MusicFiles_LibraryFolders_parent_folder_id",
                table: "MusicFiles",
                column: "parent_folder_id",
                principalTable: "LibraryFolders",
                principalColumn: "library_folder_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicFiles_LibraryFolders_parent_folder_id",
                table: "MusicFiles");

            migrationBuilder.DropTable(
                name: "LibraryFolders");

            migrationBuilder.DropIndex(
                name: "IX_MusicFiles_parent_folder_id",
                table: "MusicFiles");

            migrationBuilder.DropColumn(
                name: "parent_folder_id",
                table: "MusicFiles");
        }
    }
}
