using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_PlayerCampaignParticipation_IntroduceSkillValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ability_values",
                table: "PlayerCampaignParticipation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "skill_values",
                table: "PlayerCampaignParticipation",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ability_values",
                table: "PlayerCampaignParticipation");

            migrationBuilder.DropColumn(
                name: "skill_values",
                table: "PlayerCampaignParticipation");
        }
    }
}
