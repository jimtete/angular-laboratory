using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_SessionNotes_PresentationNotes_DecisionNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reference_outcome",
                table: "SessionNoteStoryBeatReferences",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Presented");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reference_outcome",
                table: "SessionNoteStoryBeatReferences");
        }
    }
}
