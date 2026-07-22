IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708093326_Initial'
)
BEGIN
    CREATE TABLE [Users] (
        [user_id] uniqueidentifier NOT NULL,
        [first_name] nvarchar(max) NOT NULL,
        [last_name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708093326_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260708093326_Initial', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708112537_Alter_UsersTable_Add_UsernamePassword'
)
BEGIN
    ALTER TABLE [Users] ADD [password] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708112537_Alter_UsersTable_Add_UsernamePassword'
)
BEGIN
    ALTER TABLE [Users] ADD [username] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708112537_Alter_UsersTable_Add_UsernamePassword'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260708112537_Alter_UsersTable_Add_UsernamePassword', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708134839_Alter_UsersTable_Add_PasswordSalt'
)
BEGIN
    ALTER TABLE [Users] ADD [password_salt] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708134839_Alter_UsersTable_Add_PasswordSalt'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260708134839_Alter_UsersTable_Add_PasswordSalt', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709122929_CreateTable_CharacterSheet'
)
BEGIN
    CREATE TABLE [CharacterSheets] (
        [user_id] uniqueidentifier NOT NULL,
        [portrait_url] nvarchar(max) NULL,
        [background] nvarchar(max) NULL,
        [information] nvarchar(max) NULL,
        [first_name] nvarchar(max) NOT NULL,
        [last_name] nvarchar(max) NOT NULL,
        [nationality] nvarchar(max) NULL,
        [height] int NULL,
        [weight] int NULL,
        [traits] nvarchar(max) NOT NULL,
        [equipment] nvarchar(max) NOT NULL,
        [logic_rating] int NOT NULL,
        [psyche_rating] int NOT NULL,
        [physical_rating] int NOT NULL,
        [motorics_rating] int NOT NULL,
        [actions] nvarchar(max) NULL,
        CONSTRAINT [PK_CharacterSheets] PRIMARY KEY ([user_id]),
        CONSTRAINT [CK_CharacterSheets_LogicRating] CHECK ([logic_rating] BETWEEN 0 AND 15),
        CONSTRAINT [CK_CharacterSheets_MotoricsRating] CHECK ([motorics_rating] BETWEEN 0 AND 15),
        CONSTRAINT [CK_CharacterSheets_PhysicalRating] CHECK ([physical_rating] BETWEEN 0 AND 15),
        CONSTRAINT [CK_CharacterSheets_PsycheRating] CHECK ([psyche_rating] BETWEEN 0 AND 15),
        CONSTRAINT [FK_CharacterSheets_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709122929_CreateTable_CharacterSheet'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260709122929_CreateTable_CharacterSheet', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709123739_AlterTable_CharacterSheet_AddColumn_CharacterClass'
)
BEGIN
    ALTER TABLE [CharacterSheets] ADD [character_class] nvarchar(64) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709123739_AlterTable_CharacterSheet_AddColumn_CharacterClass'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260709123739_AlterTable_CharacterSheet_AddColumn_CharacterClass', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    CREATE TABLE [Permissions] (
        [permission_id] uniqueidentifier NOT NULL,
        [name] nvarchar(128) NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([permission_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    CREATE TABLE [Roles] (
        [role_id] uniqueidentifier NOT NULL,
        [name] nvarchar(128) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([role_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [role_id] uniqueidentifier NOT NULL,
        [permission_id] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([role_id], [permission_id]),
        CONSTRAINT [FK_RolePermissions_Permissions_permission_id] FOREIGN KEY ([permission_id]) REFERENCES [Permissions] ([permission_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Roles_role_id] FOREIGN KEY ([role_id]) REFERENCES [Roles] ([role_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [user_id] uniqueidentifier NOT NULL,
        [role_id] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([user_id], [role_id]),
        CONSTRAINT [FK_UserRoles_Roles_role_id] FOREIGN KEY ([role_id]) REFERENCES [Roles] ([role_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_name] ON [Permissions] ([name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_permission_id] ON [RolePermissions] ([permission_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_name] ON [Roles] ([name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    CREATE INDEX [IX_UserRoles_role_id] ON [UserRoles] ([role_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710100915_Create_RBAC_Models'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710100915_Create_RBAC_Models', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710102000_Seed_Master_Player_Roles'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [name] = N'Master')
    BEGIN
        INSERT INTO [Roles] ([role_id], [name])
        VALUES ('1f26a07b-9f08-4c7f-a1c0-3d3d4bf9ef10', N'Master');
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710102000_Seed_Master_Player_Roles'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [name] = N'Player')
    BEGIN
        INSERT INTO [Roles] ([role_id], [name])
        VALUES ('e45f7274-bf3d-4bfb-bd61-02f977fdd911', N'Player');
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710102000_Seed_Master_Player_Roles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710102000_Seed_Master_Player_Roles', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710114907_CreateTable_Campaigns'
)
BEGIN
    CREATE TABLE [Campaigns] (
        [campaign_id] uniqueidentifier NOT NULL,
        [campaign_name] nvarchar(max) NOT NULL,
        [game_master] nvarchar(max) NOT NULL,
        [version] nvarchar(max) NOT NULL,
        [campaign_picture_url] nvarchar(max) NULL,
        CONSTRAINT [PK_Campaigns] PRIMARY KEY ([campaign_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710114907_CreateTable_Campaigns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710114907_CreateTable_Campaigns', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710122548_Alter_CampaignsTable_AddGameMasterReference'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Campaigns]') AND [c].[name] = N'game_master');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Campaigns] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Campaigns] DROP COLUMN [game_master];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710122548_Alter_CampaignsTable_AddGameMasterReference'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [game_master_id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710122548_Alter_CampaignsTable_AddGameMasterReference'
)
BEGIN
    CREATE INDEX [IX_Campaigns_game_master_id] ON [Campaigns] ([game_master_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710122548_Alter_CampaignsTable_AddGameMasterReference'
)
BEGIN
    ALTER TABLE [Campaigns] ADD CONSTRAINT [FK_Campaigns_Users_game_master_id] FOREIGN KEY ([game_master_id]) REFERENCES [Users] ([user_id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710122548_Alter_CampaignsTable_AddGameMasterReference'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710122548_Alter_CampaignsTable_AddGameMasterReference', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713091724_UpdateTable_CampaignsTable_AddCreationDate'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [date_created] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'));
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713091724_UpdateTable_CampaignsTable_AddCreationDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713091724_UpdateTable_CampaignsTable_AddCreationDate', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713095737_AddStoredProcedureSchemasAndGetCampaignsByGameMasterId'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'platform')
        EXEC(N'CREATE SCHEMA [platform]');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713095737_AddStoredProcedureSchemasAndGetCampaignsByGameMasterId'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'gameplay')
        EXEC(N'CREATE SCHEMA [gameplay]');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713095737_AddStoredProcedureSchemasAndGetCampaignsByGameMasterId'
)
BEGIN
    CREATE OR ALTER PROCEDURE [platform].[GetCampaignsByGameMasterId]
        @GameMasterId uniqueidentifier
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            c.campaign_id AS CampaignId,
            c.game_master_id AS GameMasterId,
            u.username AS GameMasterUsername,
            c.campaign_name AS CampaignName,
            c.version AS Version,
            c.campaign_picture_url AS CampaignPictureUrl,
            c.date_created AS DateCreated
        FROM [dbo].[Campaigns] AS c
        INNER JOIN [dbo].[Users] AS u
            ON u.user_id = c.game_master_id
        WHERE c.game_master_id = @GameMasterId
        ORDER BY c.date_created DESC;
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713095737_AddStoredProcedureSchemasAndGetCampaignsByGameMasterId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713095737_AddStoredProcedureSchemasAndGetCampaignsByGameMasterId', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713113239_CreateTable_CampaignSettings'
)
BEGIN
    CREATE TABLE [CampaignSettings] (
        [campaign_id] uniqueidentifier NOT NULL,
        [max_number_of_players] int NOT NULL DEFAULT 1,
        CONSTRAINT [PK_CampaignSettings] PRIMARY KEY ([campaign_id]),
        CONSTRAINT [FK_CampaignSettings_Campaigns_campaign_id] FOREIGN KEY ([campaign_id]) REFERENCES [Campaigns] ([campaign_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713113239_CreateTable_CampaignSettings'
)
BEGIN
    INSERT INTO [dbo].[CampaignSettings] ([campaign_id], [max_number_of_players])
    SELECT
        [campaign_id],
        1
    FROM [dbo].[Campaigns] AS [campaign]
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[CampaignSettings] AS [settings]
        WHERE [settings].[campaign_id] = [campaign].[campaign_id]
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713113239_CreateTable_CampaignSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713113239_CreateTable_CampaignSettings', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713121059_CreateTable_PlayerCampaignParticipation'
)
BEGIN
    CREATE TABLE [PlayerCampaignParticipation] (
        [campaign_id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [date_joined] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        CONSTRAINT [PK_PlayerCampaignParticipation] PRIMARY KEY ([campaign_id], [user_id]),
        CONSTRAINT [FK_PlayerCampaignParticipation_Campaigns_campaign_id] FOREIGN KEY ([campaign_id]) REFERENCES [Campaigns] ([campaign_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PlayerCampaignParticipation_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713121059_CreateTable_PlayerCampaignParticipation'
)
BEGIN
    CREATE INDEX [IX_PlayerCampaignParticipation_user_id] ON [PlayerCampaignParticipation] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713121059_CreateTable_PlayerCampaignParticipation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713121059_CreateTable_PlayerCampaignParticipation', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122915_AlterTable_PlayerCampaignParticipation_AddNickname'
)
BEGIN
    CREATE TABLE [CampaignParticipationInvite] (
        [campaign_id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [date_invited] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        CONSTRAINT [PK_CampaignParticipationInvite] PRIMARY KEY ([campaign_id], [user_id]),
        CONSTRAINT [FK_CampaignParticipationInvite_Campaigns_campaign_id] FOREIGN KEY ([campaign_id]) REFERENCES [Campaigns] ([campaign_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CampaignParticipationInvite_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122915_AlterTable_PlayerCampaignParticipation_AddNickname'
)
BEGIN
    CREATE INDEX [IX_CampaignParticipationInvite_user_id] ON [CampaignParticipationInvite] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122915_AlterTable_PlayerCampaignParticipation_AddNickname'
)
BEGIN
    ALTER TABLE [PlayerCampaignParticipation] ADD [nickname] nvarchar(128) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122915_AlterTable_PlayerCampaignParticipation_AddNickname'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713122915_AlterTable_PlayerCampaignParticipation_AddNickname', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713133000_CreateTable_Notifications'
)
BEGIN
    CREATE TABLE [Notifications] (
        [notification_id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [notification_type] nvarchar(64) NOT NULL,
        [description] nvarchar(512) NOT NULL,
        [date_created] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        [date_read] datetimeoffset NULL,
        [date_deleted] datetimeoffset NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([notification_id]),
        CONSTRAINT [FK_Notifications_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713133000_CreateTable_Notifications'
)
BEGIN
    CREATE INDEX [IX_Notifications_user_id_date_deleted_date_created] ON [Notifications] ([user_id], [date_deleted], [date_created]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713133000_CreateTable_Notifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713133000_CreateTable_Notifications', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713134500_CreateProcedure_CreateNotification'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713134500_CreateProcedure_CreateNotification'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713134500_CreateProcedure_CreateNotification', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714090000_CreateProcedure_GetAvailableNotificationsByUserId'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714090000_CreateProcedure_GetAvailableNotificationsByUserId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714090000_CreateProcedure_GetAvailableNotificationsByUserId', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715075540_AlterTable_CampaignSettings_AddCampaignDescriptionColumn'
)
BEGIN
    ALTER TABLE [CampaignSettings] ADD [campaign_description] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715075540_AlterTable_CampaignSettings_AddCampaignDescriptionColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715075540_AlterTable_CampaignSettings_AddCampaignDescriptionColumn', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715083333_CreateTables_CampaignSession_SessionNotes'
)
BEGIN
    CREATE TABLE [CampaignSessions] (
        [session_id] int NOT NULL IDENTITY,
        [campaign_id] uniqueidentifier NOT NULL,
        [session_number] int NOT NULL,
        [description] nvarchar(max) NULL,
        [session_date] datetimeoffset NOT NULL,
        [created_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        [updated_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        CONSTRAINT [PK_CampaignSessions] PRIMARY KEY ([session_id]),
        CONSTRAINT [FK_CampaignSessions_Campaigns_campaign_id] FOREIGN KEY ([campaign_id]) REFERENCES [Campaigns] ([campaign_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715083333_CreateTables_CampaignSession_SessionNotes'
)
BEGIN
    CREATE TABLE [SessionNotes] (
        [session_note_id] int NOT NULL IDENTITY,
        [session_id] int NOT NULL,
        [note_type] nvarchar(64) NOT NULL,
        [content] nvarchar(max) NOT NULL,
        [created_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        [updated_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        CONSTRAINT [PK_SessionNotes] PRIMARY KEY ([session_note_id]),
        CONSTRAINT [FK_SessionNotes_CampaignSessions_session_id] FOREIGN KEY ([session_id]) REFERENCES [CampaignSessions] ([session_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715083333_CreateTables_CampaignSession_SessionNotes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CampaignSessions_campaign_id_session_number] ON [CampaignSessions] ([campaign_id], [session_number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715083333_CreateTables_CampaignSession_SessionNotes'
)
BEGIN
    CREATE INDEX [IX_SessionNotes_session_id] ON [SessionNotes] ([session_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715083333_CreateTables_CampaignSession_SessionNotes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715083333_CreateTables_CampaignSession_SessionNotes', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715095648_AlterCampaignSessionDateNullable'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CampaignSessions]') AND [c].[name] = N'session_date');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [CampaignSessions] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [CampaignSessions] ALTER COLUMN [session_date] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715095648_AlterCampaignSessionDateNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715095648_AlterCampaignSessionDateNullable', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716071929_AlterTable_AddOrdering_Column'
)
BEGIN
    DROP INDEX [IX_SessionNotes_session_id] ON [SessionNotes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716071929_AlterTable_AddOrdering_Column'
)
BEGIN
    ALTER TABLE [SessionNotes] ADD [note_order] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716071929_AlterTable_AddOrdering_Column'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionNotes_session_id_note_order] ON [SessionNotes] ([session_id], [note_order]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716071929_AlterTable_AddOrdering_Column'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716071929_AlterTable_AddOrdering_Column', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716080412_CreateTable_SessionNoteChoice'
)
BEGIN
    CREATE TABLE [SessionNoteChoices] (
        [session_note_choice_id] int NOT NULL IDENTITY,
        [session_note_id] int NOT NULL,
        [choice_order] int NOT NULL,
        [choice_text] nvarchar(max) NOT NULL,
        [is_chosen] bit NOT NULL,
        CONSTRAINT [PK_SessionNoteChoices] PRIMARY KEY ([session_note_choice_id]),
        CONSTRAINT [FK_SessionNoteChoices_SessionNotes_session_note_id] FOREIGN KEY ([session_note_id]) REFERENCES [SessionNotes] ([session_note_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716080412_CreateTable_SessionNoteChoice'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionNoteChoices_session_note_id_choice_order] ON [SessionNoteChoices] ([session_note_id], [choice_order]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716080412_CreateTable_SessionNoteChoice'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SessionNoteChoices_session_note_id_is_chosen] ON [SessionNoteChoices] ([session_note_id], [is_chosen]) WHERE [is_chosen] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716080412_CreateTable_SessionNoteChoice'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716080412_CreateTable_SessionNoteChoice', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716092129_CreateTable_CampaignMilestones'
)
BEGIN
    CREATE TABLE [CampaignMilestones] (
        [campaign_milestone_id] int NOT NULL IDENTITY,
        [campaign_id] uniqueidentifier NOT NULL,
        [title] nvarchar(256) NOT NULL,
        [description] nvarchar(2048) NULL,
        [achieved_at] datetimeoffset NULL,
        [importance] nvarchar(32) NOT NULL,
        [created_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        [updated_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        CONSTRAINT [PK_CampaignMilestones] PRIMARY KEY ([campaign_milestone_id]),
        CONSTRAINT [FK_CampaignMilestones_Campaigns_campaign_id] FOREIGN KEY ([campaign_id]) REFERENCES [Campaigns] ([campaign_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716092129_CreateTable_CampaignMilestones'
)
BEGIN
    CREATE INDEX [IX_CampaignMilestones_campaign_id_title] ON [CampaignMilestones] ([campaign_id], [title]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716092129_CreateTable_CampaignMilestones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716092129_CreateTable_CampaignMilestones', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716121801_CreateTable_AssetsTable'
)
BEGIN
    CREATE TABLE [Assets] (
        [asset_id] int NOT NULL IDENTITY,
        [parent_asset_id] int NULL,
        [asset_type] nvarchar(64) NOT NULL,
        [name] nvarchar(256) NOT NULL,
        [description] nvarchar(max) NOT NULL DEFAULT N'',
        [item_type] nvarchar(64) NULL,
        [created_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        [updated_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        CONSTRAINT [PK_Assets] PRIMARY KEY ([asset_id]),
        CONSTRAINT [FK_Assets_Assets_parent_asset_id] FOREIGN KEY ([parent_asset_id]) REFERENCES [Assets] ([asset_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716121801_CreateTable_AssetsTable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Assets_parent_asset_id_name] ON [Assets] ([parent_asset_id], [name]) WHERE [parent_asset_id] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716121801_CreateTable_AssetsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716121801_CreateTable_AssetsTable', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716125651_AlterTable_Asset_AddCampaignReferences'
)
BEGIN
    ALTER TABLE [Assets] ADD [campaign_ids] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716125651_AlterTable_Asset_AddCampaignReferences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716125651_AlterTable_Asset_AddCampaignReferences', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716130703_AlterTable_CampaignSettings_AddPassiveSkillCheck'
)
BEGIN
    ALTER TABLE [CampaignSettings] ADD [passive_skills_check] nvarchar(64) NOT NULL DEFAULT N'Manual';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716130703_AlterTable_CampaignSettings_AddPassiveSkillCheck'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716130703_AlterTable_CampaignSettings_AddPassiveSkillCheck', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716134137_AlterTable_PlayerParticipation_AddSkills'
)
BEGIN
    ALTER TABLE [PlayerCampaignParticipation] ADD [expertise_skills] nvarchar(max) NOT NULL DEFAULT N'[]';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716134137_AlterTable_PlayerParticipation_AddSkills'
)
BEGIN
    ALTER TABLE [PlayerCampaignParticipation] ADD [half_proficient_skills] nvarchar(max) NOT NULL DEFAULT N'[]';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716134137_AlterTable_PlayerParticipation_AddSkills'
)
BEGIN
    ALTER TABLE [PlayerCampaignParticipation] ADD [proficient_skills] nvarchar(max) NOT NULL DEFAULT N'[]';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716134137_AlterTable_PlayerParticipation_AddSkills'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716134137_AlterTable_PlayerParticipation_AddSkills', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716140500_CreateProcedure_GetAvailableItemsByCampaignId'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'gameplay')
        EXEC(N'CREATE SCHEMA [gameplay]');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716140500_CreateProcedure_GetAvailableItemsByCampaignId'
)
BEGIN
    CREATE OR ALTER PROCEDURE [gameplay].[GetAvailableItemsByCampaignId]
        @CampaignId uniqueidentifier
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            a.asset_id AS Id,
            a.parent_asset_id AS ParentAssetId,
            a.asset_type AS AssetType,
            a.name AS Name,
            a.description AS Description,
            a.item_type AS ItemType,
            a.campaign_ids AS CampaignIds,
            a.created_at AS CreatedAt,
            a.updated_at AS UpdatedAt
        FROM [dbo].[Assets] AS a
        WHERE a.asset_type = N'Items'
            AND (
                a.campaign_ids IS NULL
                OR EXISTS
                (
                    SELECT 1
                    FROM OPENJSON(a.campaign_ids) AS campaign_id
                    WHERE TRY_CONVERT(uniqueidentifier, campaign_id.[value]) = @CampaignId
                )
            )
        ORDER BY
            a.name ASC,
            a.asset_id ASC;
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716140500_CreateProcedure_GetAvailableItemsByCampaignId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716140500_CreateProcedure_GetAvailableItemsByCampaignId', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717070518_AlterTable_SessionNotesChoices_DropUniqueIsChecked'
)
BEGIN
    DROP INDEX [IX_SessionNoteChoices_session_note_id_is_chosen] ON [SessionNoteChoices];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717070518_AlterTable_SessionNotesChoices_DropUniqueIsChecked'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717070518_AlterTable_SessionNotesChoices_DropUniqueIsChecked', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717072721_CreateTable_SessionNoteMechanicsChange'
)
BEGIN
    CREATE TABLE [SessionNoteMechanicsChanges] (
        [session_note_mechanics_change_id] int NOT NULL IDENTITY,
        [session_note_id] int NOT NULL,
        [change_order] int NOT NULL,
        [player_id] uniqueidentifier NOT NULL,
        [change_text] nvarchar(max) NULL,
        CONSTRAINT [PK_SessionNoteMechanicsChanges] PRIMARY KEY ([session_note_mechanics_change_id]),
        CONSTRAINT [FK_SessionNoteMechanicsChanges_SessionNotes_session_note_id] FOREIGN KEY ([session_note_id]) REFERENCES [SessionNotes] ([session_note_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SessionNoteMechanicsChanges_Users_player_id] FOREIGN KEY ([player_id]) REFERENCES [Users] ([user_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717072721_CreateTable_SessionNoteMechanicsChange'
)
BEGIN
    CREATE INDEX [IX_SessionNoteMechanicsChanges_player_id] ON [SessionNoteMechanicsChanges] ([player_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717072721_CreateTable_SessionNoteMechanicsChange'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionNoteMechanicsChanges_session_note_id_change_order] ON [SessionNoteMechanicsChanges] ([session_note_id], [change_order]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717072721_CreateTable_SessionNoteMechanicsChange'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionNoteMechanicsChanges_session_note_id_player_id] ON [SessionNoteMechanicsChanges] ([session_note_id], [player_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717072721_CreateTable_SessionNoteMechanicsChange'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717072721_CreateTable_SessionNoteMechanicsChange', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717092754_CreateTable_AddCampaignQuests'
)
BEGIN
    CREATE TABLE [CampaignQuests] (
        [quest_id] uniqueidentifier NOT NULL,
        [campaign_id] uniqueidentifier NOT NULL,
        [quest_type] nvarchar(64) NOT NULL,
        [title] nvarchar(256) NOT NULL,
        [description] nvarchar(2048) NOT NULL DEFAULT N'',
        [given_by] nvarchar(256) NOT NULL DEFAULT N'',
        [reward] nvarchar(2048) NOT NULL DEFAULT N'',
        [completed_at] datetimeoffset NULL,
        [created_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        [updated_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        CONSTRAINT [PK_CampaignQuests] PRIMARY KEY ([quest_id]),
        CONSTRAINT [FK_CampaignQuests_Campaigns_campaign_id] FOREIGN KEY ([campaign_id]) REFERENCES [Campaigns] ([campaign_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717092754_CreateTable_AddCampaignQuests'
)
BEGIN
    CREATE TABLE [CampaignQuestTasks] (
        [quest_task_id] uniqueidentifier NOT NULL,
        [title] nvarchar(256) NOT NULL,
        [description] nvarchar(2048) NOT NULL DEFAULT N'',
        [date_completed] datetimeoffset NULL,
        [quest_id] uniqueidentifier NOT NULL,
        [created_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        [updated_at] datetimeoffset NOT NULL DEFAULT (TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')),
        CONSTRAINT [PK_CampaignQuestTasks] PRIMARY KEY ([quest_task_id]),
        CONSTRAINT [FK_CampaignQuestTasks_CampaignQuests_quest_id] FOREIGN KEY ([quest_id]) REFERENCES [CampaignQuests] ([quest_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717092754_CreateTable_AddCampaignQuests'
)
BEGIN
    CREATE INDEX [IX_CampaignQuests_campaign_id_quest_type_completed_at] ON [CampaignQuests] ([campaign_id], [quest_type], [completed_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717092754_CreateTable_AddCampaignQuests'
)
BEGIN
    CREATE INDEX [IX_CampaignQuests_campaign_id_title] ON [CampaignQuests] ([campaign_id], [title]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717092754_CreateTable_AddCampaignQuests'
)
BEGIN
    CREATE INDEX [IX_CampaignQuestTasks_quest_id_date_completed] ON [CampaignQuestTasks] ([quest_id], [date_completed]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717092754_CreateTable_AddCampaignQuests'
)
BEGIN
    CREATE INDEX [IX_CampaignQuestTasks_quest_id_title] ON [CampaignQuestTasks] ([quest_id], [title]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717092754_CreateTable_AddCampaignQuests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717092754_CreateTable_AddCampaignQuests', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720073227_CreateTable_StoryBlock_StoryBeat'
)
BEGIN
    CREATE TABLE [StoryBlocks] (
        [story_block_id] uniqueidentifier NOT NULL,
        [campaign_id] uniqueidentifier NOT NULL,
        [order_index] int NOT NULL,
        CONSTRAINT [PK_StoryBlocks] PRIMARY KEY ([story_block_id]),
        CONSTRAINT [FK_StoryBlocks_Campaigns_campaign_id] FOREIGN KEY ([campaign_id]) REFERENCES [Campaigns] ([campaign_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720073227_CreateTable_StoryBlock_StoryBeat'
)
BEGIN
    CREATE TABLE [StoryBeats] (
        [story_beat_id] uniqueidentifier NOT NULL,
        [story_block_id] uniqueidentifier NOT NULL,
        [story_beat_type] nvarchar(64) NOT NULL,
        [information] nvarchar(max) NULL,
        CONSTRAINT [PK_StoryBeats] PRIMARY KEY ([story_beat_id]),
        CONSTRAINT [FK_StoryBeats_StoryBlocks_story_block_id] FOREIGN KEY ([story_block_id]) REFERENCES [StoryBlocks] ([story_block_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720073227_CreateTable_StoryBlock_StoryBeat'
)
BEGIN
    CREATE INDEX [IX_StoryBeats_story_block_id] ON [StoryBeats] ([story_block_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720073227_CreateTable_StoryBlock_StoryBeat'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StoryBlocks_campaign_id_order_index] ON [StoryBlocks] ([campaign_id], [order_index]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720073227_CreateTable_StoryBlock_StoryBeat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720073227_CreateTable_StoryBlock_StoryBeat', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720082506_AlterTable_StoryBeats_StoryBlocks_AllowNull'
)
BEGIN
    DROP INDEX [IX_StoryBlocks_campaign_id_order_index] ON [StoryBlocks];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720082506_AlterTable_StoryBeats_StoryBlocks_AllowNull'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StoryBlocks]') AND [c].[name] = N'order_index');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [StoryBlocks] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [StoryBlocks] DROP COLUMN [order_index];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720082506_AlterTable_StoryBeats_StoryBlocks_AllowNull'
)
BEGIN
    CREATE INDEX [IX_StoryBlocks_campaign_id] ON [StoryBlocks] ([campaign_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720082506_AlterTable_StoryBeats_StoryBlocks_AllowNull'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720082506_AlterTable_StoryBeats_StoryBlocks_AllowNull', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720083354_AlterTable_StoryBlock_AddTitleColumn'
)
BEGIN
    ALTER TABLE [StoryBlocks] ADD [title] nvarchar(256) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720083354_AlterTable_StoryBlock_AddTitleColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720083354_AlterTable_StoryBlock_AddTitleColumn', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720094957_AlterTable_StoryBeats_StoryBlocks_AddNarrative_IncludeMilestone'
)
BEGIN
    ALTER TABLE [StoryBeats] ADD [narrative] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720094957_AlterTable_StoryBeats_StoryBlocks_AddNarrative_IncludeMilestone'
)
BEGIN
    CREATE TABLE [StoryBlockMilestones] (
        [story_block_id] uniqueidentifier NOT NULL,
        [campaign_milestone_id] int NOT NULL,
        [order_index] int NOT NULL,
        CONSTRAINT [PK_StoryBlockMilestones] PRIMARY KEY ([story_block_id], [campaign_milestone_id]),
        CONSTRAINT [FK_StoryBlockMilestones_CampaignMilestones_campaign_milestone_id] FOREIGN KEY ([campaign_milestone_id]) REFERENCES [CampaignMilestones] ([campaign_milestone_id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StoryBlockMilestones_StoryBlocks_story_block_id] FOREIGN KEY ([story_block_id]) REFERENCES [StoryBlocks] ([story_block_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720094957_AlterTable_StoryBeats_StoryBlocks_AddNarrative_IncludeMilestone'
)
BEGIN
    CREATE INDEX [IX_StoryBlockMilestones_campaign_milestone_id] ON [StoryBlockMilestones] ([campaign_milestone_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720094957_AlterTable_StoryBeats_StoryBlocks_AddNarrative_IncludeMilestone'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StoryBlockMilestones_story_block_id_order_index] ON [StoryBlockMilestones] ([story_block_id], [order_index]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720094957_AlterTable_StoryBeats_StoryBlocks_AddNarrative_IncludeMilestone'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720094957_AlterTable_StoryBeats_StoryBlocks_AddNarrative_IncludeMilestone', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720110358_AlterTable_StoryBeats_AddTitleColumn'
)
BEGIN
    ALTER TABLE [StoryBeats] ADD [title] nvarchar(256) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720110358_AlterTable_StoryBeats_AddTitleColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720110358_AlterTable_StoryBeats_AddTitleColumn', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720132221_AlterTable_StoryBeats_AddIndexing'
)
BEGIN
    DROP INDEX [IX_StoryBlockMilestones_campaign_milestone_id] ON [StoryBlockMilestones];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720132221_AlterTable_StoryBeats_AddIndexing'
)
BEGIN
    ALTER TABLE [StoryBeats] ADD [order_index] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720132221_AlterTable_StoryBeats_AddIndexing'
)
BEGIN
    WITH OrderedStoryBeats AS
    (
        SELECT
            story_beat_id,
            ROW_NUMBER() OVER (
                PARTITION BY story_block_id
                ORDER BY story_beat_id
            ) AS order_index
        FROM StoryBeats
    )
    UPDATE beat
    SET order_index = ordered.order_index
    FROM StoryBeats AS beat
    INNER JOIN OrderedStoryBeats AS ordered
        ON ordered.story_beat_id = beat.story_beat_id;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720132221_AlterTable_StoryBeats_AddIndexing'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StoryBlockMilestones_campaign_milestone_id] ON [StoryBlockMilestones] ([campaign_milestone_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720132221_AlterTable_StoryBeats_AddIndexing'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StoryBeats_story_block_id_order_index] ON [StoryBeats] ([story_block_id], [order_index]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720132221_AlterTable_StoryBeats_AddIndexing'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720132221_AlterTable_StoryBeats_AddIndexing', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721071644_CreateTable_StoryBeatRolePlaying'
)
BEGIN
    ALTER TABLE [StoryBeats] ADD [roleplaying] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721071644_CreateTable_StoryBeatRolePlaying'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260721071644_CreateTable_StoryBeatRolePlaying', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721081249_AlterTable_PlayerCampaignParticipation_IntroduceSkillValues'
)
BEGIN
    ALTER TABLE [PlayerCampaignParticipation] ADD [ability_values] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721081249_AlterTable_PlayerCampaignParticipation_IntroduceSkillValues'
)
BEGIN
    ALTER TABLE [PlayerCampaignParticipation] ADD [skill_values] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721081249_AlterTable_PlayerCampaignParticipation_IntroduceSkillValues'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260721081249_AlterTable_PlayerCampaignParticipation_IntroduceSkillValues', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721082833_AlterTable_RolePlayBeats_IntroduceNpcTags'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260721082833_AlterTable_RolePlayBeats_IntroduceNpcTags', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721090122_AddPlayerParticipationValuesAndGetRoleplayingNpcs'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'gameplay')
        EXEC(N'CREATE SCHEMA [gameplay]');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721090122_AddPlayerParticipationValuesAndGetRoleplayingNpcs'
)
BEGIN
    CREATE OR ALTER PROCEDURE [gameplay].[GetRoleplayingStoryBeatNpcsByCampaignId]
        @CampaignId uniqueidentifier
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            block.campaign_id AS CampaignId,
            block.story_block_id AS StoryBlockId,
            block.title AS StoryBlockTitle,
            beat.story_beat_id AS StoryBeatId,
            beat.title AS StoryBeatTitle,
            beat.order_index AS StoryBeatOrderIndex,
            COALESCE(
                NULLIF(JSON_VALUE(npc.[value], '$.Tag'), N''),
                NULLIF(JSON_VALUE(npc.[value], '$.Name'), N''),
                NULLIF(JSON_VALUE(npc.[value], '$.Description'), N''),
                N''
            ) AS NpcTag,
            COALESCE(JSON_VALUE(npc.[value], '$.Name'), N'') AS NpcName,
            COALESCE(JSON_VALUE(npc.[value], '$.Description'), N'') AS NpcDescription
        FROM [dbo].[StoryBeats] AS beat
        INNER JOIN [dbo].[StoryBlocks] AS block
            ON block.story_block_id = beat.story_block_id
        CROSS APPLY OPENJSON(
            CASE
                WHEN ISJSON(beat.roleplaying) = 1 THEN beat.roleplaying
                ELSE N'{}'
            END,
            '$.Npcs') AS npc
        WHERE block.campaign_id = @CampaignId
            AND beat.story_beat_type = N'Roleplaying'
            AND beat.roleplaying IS NOT NULL
            AND ISJSON(beat.roleplaying) = 1
        ORDER BY
            block.title ASC,
            block.story_block_id ASC,
            beat.order_index ASC,
            beat.story_beat_id ASC,
            npc.[key] ASC;
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721090122_AddPlayerParticipationValuesAndGetRoleplayingNpcs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260721090122_AddPlayerParticipationValuesAndGetRoleplayingNpcs', N'10.0.9');
END;

COMMIT;
GO

