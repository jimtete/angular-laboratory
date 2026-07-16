using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_SessionNoteChoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionNoteChoices",
                columns: table => new
                {
                    session_note_choice_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_note_id = table.Column<int>(type: "int", nullable: false),
                    choice_order = table.Column<int>(type: "int", nullable: false),
                    choice_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_chosen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionNoteChoices", x => x.session_note_choice_id);
                    table.ForeignKey(
                        name: "FK_SessionNoteChoices_SessionNotes_session_note_id",
                        column: x => x.session_note_id,
                        principalTable: "SessionNotes",
                        principalColumn: "session_note_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteChoices_session_note_id_choice_order",
                table: "SessionNoteChoices",
                columns: new[] { "session_note_id", "choice_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteChoices_session_note_id_is_chosen",
                table: "SessionNoteChoices",
                columns: new[] { "session_note_id", "is_chosen" },
                unique: true,
                filter: "[is_chosen] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionNoteChoices");
        }
    }
}
