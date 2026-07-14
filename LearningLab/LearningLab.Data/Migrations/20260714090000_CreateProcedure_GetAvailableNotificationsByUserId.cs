using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateProcedure_GetAvailableNotificationsByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE [platform].[GetAvailableNotificationsByUserId]
                    @UserId uniqueidentifier
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        n.notification_id AS NotificationId,
                        n.user_id AS UserId,
                        n.notification_type AS NotificationType,
                        n.description AS Description,
                        n.date_created AS DateCreated,
                        n.date_read AS DateRead
                    FROM [dbo].[Notifications] AS n
                    WHERE n.user_id = @UserId
                        AND n.date_deleted IS NULL
                    ORDER BY n.date_created DESC;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP PROCEDURE IF EXISTS [platform].[GetAvailableNotificationsByUserId];
                """);
        }
    }
}
