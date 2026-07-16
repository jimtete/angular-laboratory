using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_AddOrdering_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionNotes_session_id",
                table: "SessionNotes");

            migrationBuilder.AddColumn<int>(
                name: "note_order",
                table: "SessionNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotes_session_id_note_order",
                table: "SessionNotes",
                columns: new[] { "session_id", "note_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionNotes_session_id_note_order",
                table: "SessionNotes");

            migrationBuilder.DropColumn(
                name: "note_order",
                table: "SessionNotes");

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotes_session_id",
                table: "SessionNotes",
                column: "session_id");
        }
    }
}
