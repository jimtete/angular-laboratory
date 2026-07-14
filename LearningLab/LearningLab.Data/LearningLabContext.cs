using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Character;
using LearningLab.Data.Models.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data;

public class LearningLabContext : DbContext
{
    public LearningLabContext(DbContextOptions<LearningLabContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<CharacterSheet> CharacterSheets { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CampaignSettings> CampaignSettings { get; set; }
    public DbSet<PlayerCampaignParticipation> PlayerCampaignParticipations { get; set; }
    public DbSet<CampaignParticipationInvite> CampaignParticipationInvites { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(user => user.UserId);

            entity.Property(user => user.UserId)
                .HasColumnName("user_id");

            entity.Property(user => user.Username)
                .HasColumnName("username")
                .IsRequired();

            entity.Property(user => user.Password)
                .HasColumnName("password")
                .IsRequired();

            entity.Property(user => user.PasswordSalt)
                .HasColumnName("password_salt")
                .IsRequired();

            entity.Property(user => user.FirstName)
                .HasColumnName("first_name")
                .IsRequired();

            entity.Property(user => user.LastName)
                .HasColumnName("last_name")
                .IsRequired();

            entity.HasMany(user => user.UserRoles)
                .WithOne(userRole => userRole.User)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.OwnedCampaigns)
                .WithOne(campaign => campaign.GameMaster)
                .HasForeignKey(campaign => campaign.GameMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(user => user.CampaignParticipations)
                .WithOne(participation => participation.User)
                .HasForeignKey(participation => participation.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.CampaignParticipationInvites)
                .WithOne(invite => invite.User)
                .HasForeignKey(invite => invite.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.Notifications)
                .WithOne(notification => notification.User)
                .HasForeignKey(notification => notification.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");

            entity.HasKey(notification => notification.NotificationId);

            entity.Property(notification => notification.NotificationId)
                .HasColumnName("notification_id");

            entity.Property(notification => notification.UserId)
                .HasColumnName("user_id");

            entity.Property(notification => notification.NotificationType)
                .HasColumnName("notification_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(notification => notification.Description)
                .HasColumnName("description")
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(notification => notification.DateCreated)
                .HasColumnName("date_created")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(notification => notification.DateRead)
                .HasColumnName("date_read");

            entity.Property(notification => notification.DateDeleted)
                .HasColumnName("date_deleted");

            entity.HasIndex(notification => new
            {
                notification.UserId,
                notification.DateDeleted,
                notification.DateCreated
            });
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.ToTable("Campaigns");

            entity.HasKey(campaign => campaign.CampaignId);

            entity.Property(campaign => campaign.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(campaign => campaign.GameMasterId)
                .HasColumnName("game_master_id");

            entity.Property(campaign => campaign.CampaignName)
                .HasColumnName("campaign_name")
                .IsRequired();

            entity.Property(campaign => campaign.Version)
                .HasColumnName("version")
                .IsRequired();

            entity.Property(campaign => campaign.CampaignPictureUrl)
                .HasColumnName("campaign_picture_url");

            entity.Property(campaign => campaign.DateCreated)
                .HasColumnName("date_created")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(campaign => campaign.Settings)
                .WithOne(settings => settings.Campaign)
                .HasForeignKey<CampaignSettings>(settings => settings.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(campaign => campaign.PlayerParticipations)
                .WithOne(participation => participation.Campaign)
                .HasForeignKey(participation => participation.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(campaign => campaign.ParticipationInvites)
                .WithOne(invite => invite.Campaign)
                .HasForeignKey(invite => invite.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CampaignSettings>(entity =>
        {
            entity.ToTable("CampaignSettings");

            entity.HasKey(settings => settings.CampaignId);

            entity.Property(settings => settings.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(settings => settings.MaxNumberOfPlayers)
                .HasColumnName("max_number_of_players")
                .HasDefaultValue(1)
                .IsRequired();
        });

        modelBuilder.Entity<PlayerCampaignParticipation>(entity =>
        {
            entity.ToTable("PlayerCampaignParticipation");

            entity.HasKey(participation => new
            {
                participation.CampaignId,
                participation.UserId
            });

            entity.Property(participation => participation.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(participation => participation.UserId)
                .HasColumnName("user_id");

            entity.Property(participation => participation.Nickname)
                .HasColumnName("nickname")
                .HasMaxLength(128);

            entity.Property(participation => participation.DateJoined)
                .HasColumnName("date_joined")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();
        });

        modelBuilder.Entity<CampaignParticipationInvite>(entity =>
        {
            entity.ToTable("CampaignParticipationInvite");

            entity.HasKey(invite => new
            {
                invite.CampaignId,
                invite.UserId
            });

            entity.Property(invite => invite.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(invite => invite.UserId)
                .HasColumnName("user_id");

            entity.Property(invite => invite.DateInvited)
                .HasColumnName("date_invited")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(role => role.RoleId);

            entity.Property(role => role.RoleId)
                .HasColumnName("role_id");

            entity.Property(role => role.Name)
                .HasColumnName("name")
                .HasMaxLength(128)
                .IsRequired();

            entity.HasIndex(role => role.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");

            entity.HasKey(permission => permission.PermissionId);

            entity.Property(permission => permission.PermissionId)
                .HasColumnName("permission_id");

            entity.Property(permission => permission.Name)
                .HasColumnName("name")
                .HasMaxLength(128)
                .IsRequired();

            entity.HasIndex(permission => permission.Name)
                .IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");

            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            entity.Property(userRole => userRole.UserId)
                .HasColumnName("user_id");

            entity.Property(userRole => userRole.RoleId)
                .HasColumnName("role_id");

            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");

            entity.HasKey(rolePermission => new
            {
                rolePermission.RoleId,
                rolePermission.PermissionId
            });

            entity.Property(rolePermission => rolePermission.RoleId)
                .HasColumnName("role_id");

            entity.Property(rolePermission => rolePermission.PermissionId)
                .HasColumnName("permission_id");

            entity.HasOne(rolePermission => rolePermission.Role)
                .WithMany(role => role.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rolePermission => rolePermission.Permission)
                .WithMany(permission => permission.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CharacterSheet>(entity =>
        {
            entity.ToTable("CharacterSheets", table =>
            {
                table.HasCheckConstraint(
                    "CK_CharacterSheets_LogicRating",
                    "[logic_rating] BETWEEN 0 AND 15");
                table.HasCheckConstraint(
                    "CK_CharacterSheets_PsycheRating",
                    "[psyche_rating] BETWEEN 0 AND 15");
                table.HasCheckConstraint(
                    "CK_CharacterSheets_PhysicalRating",
                    "[physical_rating] BETWEEN 0 AND 15");
                table.HasCheckConstraint(
                    "CK_CharacterSheets_MotoricsRating",
                    "[motorics_rating] BETWEEN 0 AND 15");
            });

            entity.HasKey(character => character.UserId);

            entity.Property(character => character.UserId)
                .HasColumnName("user_id");

            entity.HasOne(character => character.User)
                .WithOne(user => user.CharacterSheet)
                .HasForeignKey<CharacterSheet>(character => character.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(character => character.PortraitUrl)
                .HasColumnName("portrait_url");

            entity.Property(character => character.Background)
                .HasColumnName("background");

            entity.Property(character => character.Information)
                .HasColumnName("information");

            entity.Property(character => character.FirstName)
                .HasColumnName("first_name")
                .IsRequired();

            entity.Property(character => character.LastName)
                .HasColumnName("last_name")
                .IsRequired();

            entity.Property(character => character.CharacterClass)
                .HasColumnName("character_class")
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(character => character.Nationality)
                .HasColumnName("nationality");

            entity.Property(character => character.Height)
                .HasColumnName("height");

            entity.Property(character => character.Weight)
                .HasColumnName("weight");

            entity.OwnsMany(character => character.Actions, actions =>
            {
                actions.ToJson("actions");

                actions.Property(action => action.ActionType)
                    .HasConversion<string>();
            });

            entity.PrimitiveCollection(character => character.Traits)
                .HasColumnName("traits")
                .HasColumnType("nvarchar(max)");

            entity.PrimitiveCollection(character => character.Equipment)
                .HasColumnName("equipment")
                .HasColumnType("nvarchar(max)");

            entity.Property(character => character.LogicRating)
                .HasColumnName("logic_rating");

            entity.Property(character => character.PsycheRating)
                .HasColumnName("psyche_rating");

            entity.Property(character => character.PhysicalRating)
                .HasColumnName("physical_rating");

            entity.Property(character => character.MotoricsRating)
                .HasColumnName("motorics_rating");
        });
    }
}
