using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_SessionNotesChoices_DropUniqueIsChecked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionNoteChoices_session_note_id_is_chosen",
                table: "SessionNoteChoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteChoices_session_note_id_is_chosen",
                table: "SessionNoteChoices",
                columns: new[] { "session_note_id", "is_chosen" },
                unique: true,
                filter: "[is_chosen] = 1");
        }
    }
}
