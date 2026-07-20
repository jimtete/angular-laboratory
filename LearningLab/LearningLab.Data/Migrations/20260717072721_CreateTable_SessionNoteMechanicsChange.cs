using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_SessionNoteMechanicsChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionNoteMechanicsChanges",
                columns: table => new
                {
                    session_note_mechanics_change_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_note_id = table.Column<int>(type: "int", nullable: false),
                    change_order = table.Column<int>(type: "int", nullable: false),
                    player_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    change_text = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionNoteMechanicsChanges", x => x.session_note_mechanics_change_id);
                    table.ForeignKey(
                        name: "FK_SessionNoteMechanicsChanges_SessionNotes_session_note_id",
                        column: x => x.session_note_id,
                        principalTable: "SessionNotes",
                        principalColumn: "session_note_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionNoteMechanicsChanges_Users_player_id",
                        column: x => x.player_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteMechanicsChanges_player_id",
                table: "SessionNoteMechanicsChanges",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteMechanicsChanges_session_note_id_change_order",
                table: "SessionNoteMechanicsChanges",
                columns: new[] { "session_note_id", "change_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteMechanicsChanges_session_note_id_player_id",
                table: "SessionNoteMechanicsChanges",
                columns: new[] { "session_note_id", "player_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionNoteMechanicsChanges");
        }
    }
}
