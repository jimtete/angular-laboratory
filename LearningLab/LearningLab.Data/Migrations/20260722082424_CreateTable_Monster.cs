using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_Monster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Monsters",
                columns: table => new
                {
                    monster_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    size = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    race = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    @class = table.Column<string>(name: "class", type: "nvarchar(128)", maxLength: 128, nullable: true),
                    tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monsters", x => x.monster_id);
                });

            migrationBuilder.CreateTable(
                name: "MonsterAbilities",
                columns: table => new
                {
                    monster_ability_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    monster_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    value = table.Column<int>(type: "int", nullable: true),
                    modifier = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonsterAbilities", x => x.monster_ability_id);
                    table.ForeignKey(
                        name: "FK_MonsterAbilities_Monsters_monster_id",
                        column: x => x.monster_id,
                        principalTable: "Monsters",
                        principalColumn: "monster_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonsterFeatures",
                columns: table => new
                {
                    monster_feature_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    monster_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    usage_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resource_cost = table.Column<int>(type: "int", nullable: true),
                    is_spell = table.Column<bool>(type: "bit", nullable: false),
                    spell_level = table.Column<int>(type: "int", nullable: true),
                    casting_time = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    range = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    duration = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    concentration = table.Column<bool>(type: "bit", nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonsterFeatures", x => x.monster_feature_id);
                    table.ForeignKey(
                        name: "FK_MonsterFeatures_Monsters_monster_id",
                        column: x => x.monster_id,
                        principalTable: "Monsters",
                        principalColumn: "monster_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonsterProficiencies",
                columns: table => new
                {
                    monster_proficiency_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    monster_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    bonus = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonsterProficiencies", x => x.monster_proficiency_id);
                    table.ForeignKey(
                        name: "FK_MonsterProficiencies_Monsters_monster_id",
                        column: x => x.monster_id,
                        principalTable: "Monsters",
                        principalColumn: "monster_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonsterSpellcasting",
                columns: table => new
                {
                    monster_id = table.Column<int>(type: "int", nullable: false),
                    spellcasting_ability = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    spell_save_dc = table.Column<int>(type: "int", nullable: true),
                    spell_attack_bonus = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonsterSpellcasting", x => x.monster_id);
                    table.ForeignKey(
                        name: "FK_MonsterSpellcasting_Monsters_monster_id",
                        column: x => x.monster_id,
                        principalTable: "Monsters",
                        principalColumn: "monster_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonsterSpellSlots",
                columns: table => new
                {
                    monster_spell_slot_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    monster_spellcasting_id = table.Column<int>(type: "int", nullable: false),
                    spell_level = table.Column<int>(type: "int", nullable: false),
                    maximum_slots = table.Column<int>(type: "int", nullable: true),
                    remaining_slots = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonsterSpellSlots", x => x.monster_spell_slot_id);
                    table.ForeignKey(
                        name: "FK_MonsterSpellSlots_MonsterSpellcasting_monster_spellcasting_id",
                        column: x => x.monster_spellcasting_id,
                        principalTable: "MonsterSpellcasting",
                        principalColumn: "monster_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonsterAbilities_monster_id_name",
                table: "MonsterAbilities",
                columns: new[] { "monster_id", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_MonsterFeatures_monster_id_sort_order",
                table: "MonsterFeatures",
                columns: new[] { "monster_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_MonsterProficiencies_monster_id_name",
                table: "MonsterProficiencies",
                columns: new[] { "monster_id", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_Monsters_name",
                table: "Monsters",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_MonsterSpellSlots_monster_spellcasting_id_spell_level",
                table: "MonsterSpellSlots",
                columns: new[] { "monster_spellcasting_id", "spell_level" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonsterAbilities");

            migrationBuilder.DropTable(
                name: "MonsterFeatures");

            migrationBuilder.DropTable(
                name: "MonsterProficiencies");

            migrationBuilder.DropTable(
                name: "MonsterSpellSlots");

            migrationBuilder.DropTable(
                name: "MonsterSpellcasting");

            migrationBuilder.DropTable(
                name: "Monsters");
        }
    }
}
