using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateProcedure_CreateNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE [platform].[CreateNotification]
                    @NotificationId uniqueidentifier,
                    @UserId uniqueidentifier,
                    @NotificationType nvarchar(64),
                    @Description nvarchar(512),
                    @DateCreated datetimeoffset
                AS
                BEGIN
                    SET NOCOUNT ON;

                    INSERT INTO [dbo].[Notifications]
                    (
                        [notification_id],
                        [user_id],
                        [notification_type],
                        [description],
                        [date_created],
                        [date_read],
                        [date_deleted]
                    )
                    VALUES
                    (
                        @NotificationId,
                        @UserId,
                        @NotificationType,
                        @Description,
                        @DateCreated,
                        NULL,
                        NULL
                    );
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP PROCEDURE IF EXISTS [platform].[CreateNotification];
                """);
        }
    }
}
