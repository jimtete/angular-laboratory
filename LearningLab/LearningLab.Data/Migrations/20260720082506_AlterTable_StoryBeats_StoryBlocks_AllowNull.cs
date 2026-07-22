using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_StoryBeats_StoryBlocks_AllowNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoryBlocks_campaign_id_order_index",
                table: "StoryBlocks");

            migrationBuilder.DropColumn(
                name: "order_index",
                table: "StoryBlocks");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlocks_campaign_id",
                table: "StoryBlocks",
                column: "campaign_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoryBlocks_campaign_id",
                table: "StoryBlocks");

            migrationBuilder.AddColumn<int>(
                name: "order_index",
                table: "StoryBlocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlocks_campaign_id_order_index",
                table: "StoryBlocks",
                columns: new[] { "campaign_id", "order_index" },
                unique: true);
        }
    }
}
