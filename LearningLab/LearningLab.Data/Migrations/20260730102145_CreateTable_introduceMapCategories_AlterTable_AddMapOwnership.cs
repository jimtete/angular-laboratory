using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_introduceMapCategories_AlterTable_AddMapOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "Maps",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "parent_map_id",
                table: "Maps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "asset_url",
                table: "Assets",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "Assets",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "file_size_bytes",
                table: "Assets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maps_parent_map_id",
                table: "Maps",
                column: "parent_map_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maps_Maps_parent_map_id",
                table: "Maps",
                column: "parent_map_id",
                principalTable: "Maps",
                principalColumn: "map_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maps_Maps_parent_map_id",
                table: "Maps");

            migrationBuilder.DropIndex(
                name: "IX_Maps_parent_map_id",
                table: "Maps");

            migrationBuilder.DropColumn(
                name: "category",
                table: "Maps");

            migrationBuilder.DropColumn(
                name: "parent_map_id",
                table: "Maps");

            migrationBuilder.DropColumn(
                name: "asset_url",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "content_type",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "file_size_bytes",
                table: "Assets");
        }
    }
}
