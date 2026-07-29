using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropCampaignSettingsPassiveSkillsCheckDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "passive_skills_check",
                table: "CampaignSettings",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldDefaultValue: "Manual");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "passive_skills_check",
                table: "CampaignSettings",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Manual",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
