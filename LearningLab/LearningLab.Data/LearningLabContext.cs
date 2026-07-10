using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Character;
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
