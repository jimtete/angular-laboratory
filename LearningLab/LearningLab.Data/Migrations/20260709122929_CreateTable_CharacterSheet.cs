using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_CharacterSheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CharacterSheets",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    portrait_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    background = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    information = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    height = table.Column<int>(type: "int", nullable: true),
                    weight = table.Column<int>(type: "int", nullable: true),
                    traits = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    equipment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    logic_rating = table.Column<int>(type: "int", nullable: false),
                    psyche_rating = table.Column<int>(type: "int", nullable: false),
                    physical_rating = table.Column<int>(type: "int", nullable: false),
                    motorics_rating = table.Column<int>(type: "int", nullable: false),
                    actions = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSheets", x => x.user_id);
                    table.CheckConstraint("CK_CharacterSheets_LogicRating", "[logic_rating] BETWEEN 0 AND 15");
                    table.CheckConstraint("CK_CharacterSheets_MotoricsRating", "[motorics_rating] BETWEEN 0 AND 15");
                    table.CheckConstraint("CK_CharacterSheets_PhysicalRating", "[physical_rating] BETWEEN 0 AND 15");
                    table.CheckConstraint("CK_CharacterSheets_PsycheRating", "[psyche_rating] BETWEEN 0 AND 15");
                    table.ForeignKey(
                        name: "FK_CharacterSheets_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterSheets");
        }
    }
}
