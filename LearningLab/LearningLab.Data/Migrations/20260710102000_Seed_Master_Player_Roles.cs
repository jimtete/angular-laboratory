using LearningLab.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LearningLabContext))]
    [Migration("20260710102000_Seed_Master_Player_Roles")]
    public partial class Seed_Master_Player_Roles : Migration
    {
        private const string MasterRoleId = "1f26a07b-9f08-4c7f-a1c0-3d3d4bf9ef10";
        private const string PlayerRoleId = "e45f7274-bf3d-4bfb-bd61-02f977fdd911";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [name] = N'Master')
                BEGIN
                    INSERT INTO [Roles] ([role_id], [name])
                    VALUES ('{MasterRoleId}', N'Master');
                END
                """);

            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [name] = N'Player')
                BEGIN
                    INSERT INTO [Roles] ([role_id], [name])
                    VALUES ('{PlayerRoleId}', N'Player');
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                DELETE FROM [Roles]
                WHERE [role_id] IN ('{MasterRoleId}', '{PlayerRoleId}');
                """);
        }
    }
}
