using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_PlayerParticipation_AddSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "expertise_skills",
                table: "PlayerCampaignParticipation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "half_proficient_skills",
                table: "PlayerCampaignParticipation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "proficient_skills",
                table: "PlayerCampaignParticipation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expertise_skills",
                table: "PlayerCampaignParticipation");

            migrationBuilder.DropColumn(
                name: "half_proficient_skills",
                table: "PlayerCampaignParticipation");

            migrationBuilder.DropColumn(
                name: "proficient_skills",
                table: "PlayerCampaignParticipation");
        }
    }
}
