using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_Campaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    game_master = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    campaign_picture_url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.campaign_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
