using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_CampaignNpcParticipation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignNpcParticipations",
                columns: table => new
                {
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    monster_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignNpcParticipations", x => new { x.campaign_id, x.monster_id });
                    table.ForeignKey(
                        name: "FK_CampaignNpcParticipations_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignNpcParticipations_Monsters_monster_id",
                        column: x => x.monster_id,
                        principalTable: "Monsters",
                        principalColumn: "monster_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignNpcParticipations_monster_id",
                table: "CampaignNpcParticipations",
                column: "monster_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignNpcParticipations");
        }
    }
}
