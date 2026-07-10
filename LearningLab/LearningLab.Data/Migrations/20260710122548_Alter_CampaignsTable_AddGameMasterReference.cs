using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class Alter_CampaignsTable_AddGameMasterReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "game_master",
                table: "Campaigns");

            migrationBuilder.AddColumn<Guid>(
                name: "game_master_id",
                table: "Campaigns",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_game_master_id",
                table: "Campaigns",
                column: "game_master_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Users_game_master_id",
                table: "Campaigns",
                column: "game_master_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Users_game_master_id",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_game_master_id",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "game_master_id",
                table: "Campaigns");

            migrationBuilder.AddColumn<string>(
                name: "game_master",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
