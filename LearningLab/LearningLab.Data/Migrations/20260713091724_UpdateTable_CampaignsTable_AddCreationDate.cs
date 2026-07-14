using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_CampaignsTable_AddCreationDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_created",
                table: "Campaigns",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_created",
                table: "Campaigns");
        }
    }
}
