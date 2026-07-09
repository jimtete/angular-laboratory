using LearningLab.Data.Models;
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
