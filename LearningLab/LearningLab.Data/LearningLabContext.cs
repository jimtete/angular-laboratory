using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Assets;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Campaign.Quests;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.Character;
using LearningLab.Data.Models.Monsters;
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
    public DbSet<CampaignNpc> CampaignNpcs { get; set; }
    public DbSet<CampaignNpcParticipation> CampaignNpcParticipations { get; set; }
    public DbSet<CampaignQuest> CampaignQuests { get; set; }
    public DbSet<CampaignQuestTask> CampaignQuestTasks { get; set; }
    public DbSet<CampaignSession> CampaignSessions { get; set; }
    public DbSet<SessionNote> SessionNotes { get; set; }
    public DbSet<SessionNoteChoice> SessionNoteChoices { get; set; }
    public DbSet<SessionNoteMechanicsChange> SessionNoteMechanicsChanges { get; set; }
    public DbSet<CampaignMilestone> CampaignMilestones { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<StoryBlock> StoryBlocks { get; set; }
    public DbSet<StoryBeat> StoryBeats { get; set; }
    public DbSet<StoryBlockMilestone> StoryBlockMilestones { get; set; }
    public DbSet<CampaignSettings> CampaignSettings { get; set; }
    public DbSet<PlayerCampaignParticipation> PlayerCampaignParticipations { get; set; }
    public DbSet<CampaignParticipationInvite> CampaignParticipationInvites { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Monster> Monsters { get; set; }
    public DbSet<MonsterAbility> MonsterAbilities { get; set; }
    public DbSet<MonsterProficiency> MonsterProficiencies { get; set; }
    public DbSet<MonsterFeature> MonsterFeatures { get; set; }
    public DbSet<MonsterSpellcasting> MonsterSpellcasting { get; set; }
    public DbSet<MonsterSpellSlot> MonsterSpellSlots { get; set; }

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

            entity.HasMany(campaign => campaign.Milestones)
                .WithOne(milestone => milestone.Campaign)
                .HasForeignKey(milestone => milestone.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(campaign => campaign.Quests)
                .WithOne(quest => quest.Campaign)
                .HasForeignKey(quest => quest.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(campaign => campaign.Npcs)
                .WithOne(npc => npc.Campaign)
                .HasForeignKey(npc => npc.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(campaign => campaign.NpcParticipations)
                .WithOne(participation => participation.Campaign)
                .HasForeignKey(participation => participation.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(campaign => campaign.StoryBlocks)
                .WithOne(block => block.Campaign)
                .HasForeignKey(block => block.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<CampaignNpc>(entity =>
        {
            entity.ToTable("CampaignNpcs");

            entity.HasKey(npc => npc.CampaignNpcId);

            entity.Property(npc => npc.CampaignNpcId)
                .HasColumnName("campaign_npc_id");

            entity.Property(npc => npc.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(npc => npc.Tag)
                .HasColumnName("tag")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(npc => npc.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(npc => npc.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(256)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(npc => npc.Description)
                .HasColumnName("description")
                .HasMaxLength(2048)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(npc => npc.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(npc => npc.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasIndex(npc => new
            {
                npc.CampaignId,
                npc.Tag
            })
            .IsUnique();
        });

        modelBuilder.Entity<CampaignNpcParticipation>(entity =>
        {
            entity.ToTable("CampaignNpcParticipations");

            entity.HasKey(participation => new
            {
                participation.CampaignId,
                participation.MonsterId
            });

            entity.Property(participation => participation.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(participation => participation.MonsterId)
                .HasColumnName("monster_id");

            entity.HasOne(participation => participation.Campaign)
                .WithMany(campaign => campaign.NpcParticipations)
                .HasForeignKey(participation => participation.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(participation => participation.Monster)
                .WithMany(monster => monster.CampaignParticipations)
                .HasForeignKey(participation => participation.MonsterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(participation => participation.MonsterId);
        });

        modelBuilder.Entity<StoryBlock>(entity =>
        {
            entity.ToTable("StoryBlocks");

            entity.HasKey(block => block.StoryBlockId);

            entity.Property(block => block.StoryBlockId)
                .HasColumnName("story_block_id");

            entity.Property(block => block.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(block => block.Title)
                .HasColumnName("title")
                .HasMaxLength(256)
                .HasDefaultValue("")
                .IsRequired();

            entity.HasMany(block => block.Beats)
                .WithOne(beat => beat.StoryBlock)
                .HasForeignKey(beat => beat.StoryBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(block => block.Milestones)
                .WithOne(milestone => milestone.StoryBlock)
                .HasForeignKey(milestone => milestone.StoryBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(block => block.CampaignId);
        });

        modelBuilder.Entity<StoryBeat>(entity =>
        {
            entity.ToTable("StoryBeats");

            entity.HasKey(beat => beat.Id);

            entity.Property(beat => beat.Id)
                .HasColumnName("story_beat_id");

            entity.Property(beat => beat.StoryBlockId)
                .HasColumnName("story_block_id");

            entity.Property(beat => beat.OrderIndex)
                .HasColumnName("order_index")
                .IsRequired();

            entity.Property(beat => beat.StoryBeatType)
                .HasColumnName("story_beat_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(beat => beat.Title)
                .HasColumnName("title")
                .HasMaxLength(256)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(beat => beat.CampaignMilestoneId)
                .HasColumnName("campaign_milestone_id");

            entity.HasOne(beat => beat.Milestone)
                .WithOne(milestone => milestone.StoryBeat)
                .HasForeignKey<StoryBeat>(beat => beat.CampaignMilestoneId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.OwnsOne(beat => beat.Information, information =>
            {
                information.ToJson("information");

                information.Property(content => content.Narrative)
                    .IsRequired();

                information.OwnsMany(content => content.OptionalInformation, optionalInformation =>
                {
                    optionalInformation.Property(optional => optional.Skill)
                        .HasConversion<string>();

                    optionalInformation.Property(optional => optional.DifficultyClass)
                        .IsRequired();

                    optionalInformation.Property(optional => optional.Information)
                        .IsRequired();

                    optionalInformation.Property(optional => optional.Placement)
                        .HasConversion<string>()
                        .IsRequired();
                });
            });

            entity.OwnsOne(beat => beat.Narrative, narrative =>
            {
                narrative.ToJson("narrative");

                narrative.OwnsMany(content => content.Paragraphs, paragraph =>
                {
                    paragraph.Property(item => item.OrderIndex)
                        .IsRequired();

                    paragraph.Property(item => item.Text)
                        .IsRequired();
                });
            });

            entity.OwnsOne(beat => beat.Roleplaying, roleplaying =>
            {
                roleplaying.ToJson("roleplaying");

                roleplaying.Property(content => content.MainDescription)
                    .IsRequired();

                roleplaying.OwnsMany(content => content.NpcReferences, npc =>
                {
                    npc.Property(item => item.NpcTag)
                        .IsRequired();
                });

                roleplaying.OwnsMany(content => content.DiscoverableInformation, information =>
                {
                    information.Property(item => item.NpcTag)
                        .IsRequired();

                    information.Property(item => item.CheckType)
                        .HasConversion<string>()
                        .IsRequired();

                    information.Property(item => item.Skill)
                        .HasConversion<string>();

                    information.Property(item => item.Ability)
                        .HasConversion<string>();

                    information.Property(item => item.Information)
                        .IsRequired();
                });
            });

            entity.OwnsOne(beat => beat.Decision, decision =>
            {
                decision.ToJson("decision");

                decision.Property(content => content.Description)
                    .IsRequired();

                decision.OwnsMany(content => content.Decisions, option =>
                {
                    option.Property(item => item.OrderIndex)
                        .IsRequired();

                    option.Property(item => item.Title)
                        .IsRequired();

                    option.Property(item => item.Description)
                        .IsRequired();

                    option.Property(item => item.IsSelected)
                        .IsRequired();
                });
            });

            entity.HasIndex(beat => beat.StoryBlockId);

            entity.HasIndex(beat => new
            {
                beat.StoryBlockId,
                beat.OrderIndex
            })
            .IsUnique();

            entity.HasIndex(beat => beat.CampaignMilestoneId)
                .IsUnique()
                .HasFilter("[campaign_milestone_id] IS NOT NULL");
        });

        modelBuilder.Entity<StoryBlockMilestone>(entity =>
        {
            entity.ToTable("StoryBlockMilestones");

            entity.HasKey(milestone => new
            {
                milestone.StoryBlockId,
                milestone.CampaignMilestoneId
            });

            entity.Property(milestone => milestone.StoryBlockId)
                .HasColumnName("story_block_id");

            entity.Property(milestone => milestone.CampaignMilestoneId)
                .HasColumnName("campaign_milestone_id");

            entity.Property(milestone => milestone.OrderIndex)
                .HasColumnName("order_index")
                .IsRequired();

            entity.HasOne(milestone => milestone.CampaignMilestone)
                .WithMany(milestone => milestone.StoryBlockMilestones)
                .HasForeignKey(milestone => milestone.CampaignMilestoneId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(milestone => milestone.CampaignMilestoneId)
                .IsUnique();

            entity.HasIndex(milestone => new
            {
                milestone.StoryBlockId,
                milestone.OrderIndex
            })
            .IsUnique();
        });

        modelBuilder.Entity<CampaignSettings>(entity =>
        {
            entity.ToTable("CampaignSettings");

            entity.HasKey(settings => settings.CampaignId);

            entity.Property(settings => settings.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(settings => settings.CampaignDescription)
                .HasColumnName("campaign_description")
                .HasDefaultValue("");

            entity.Property(settings => settings.MaxNumberOfPlayers)
                .HasColumnName("max_number_of_players")
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(settings => settings.PassiveSkillsCheck)
                .HasColumnName("passive_skills_check")
                .HasMaxLength(64)
                .HasConversion<string>()
                .HasDefaultValue(PassiveSkillsCheck.Manual)
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

            entity.PrimitiveCollection(participation => participation.HalfProficientSkills)
                .HasColumnName("half_proficient_skills")
                .HasColumnType("nvarchar(max)");

            entity.PrimitiveCollection(participation => participation.ProficientSkills)
                .HasColumnName("proficient_skills")
                .HasColumnType("nvarchar(max)");

            entity.PrimitiveCollection(participation => participation.ExpertiseSkills)
                .HasColumnName("expertise_skills")
                .HasColumnType("nvarchar(max)");

            entity.OwnsMany(participation => participation.AbilityValues, abilityValues =>
            {
                abilityValues.ToJson("ability_values");

                abilityValues.Property(item => item.Ability)
                    .HasConversion<string>()
                    .IsRequired();

                abilityValues.Property(item => item.Value)
                    .IsRequired();
            });

            entity.OwnsMany(participation => participation.SkillValues, skillValues =>
            {
                skillValues.ToJson("skill_values");

                skillValues.Property(item => item.Skill)
                    .HasConversion<string>()
                    .IsRequired();

                skillValues.Property(item => item.Value)
                    .IsRequired();
            });

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

        modelBuilder.Entity<CampaignSession>(entity =>
        {
            entity.ToTable("CampaignSessions");

            entity.HasKey(session => session.Id);

            entity.Property(session => session.Id)
                .HasColumnName("session_id");

            entity.Property(session => session.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(session => session.SessionNumber)
                .HasColumnName("session_number")
                .IsRequired();

            entity.Property(session => session.Description)
                .HasColumnName("description");

            entity.Property(session => session.SessionDate)
                .HasColumnName("session_date");

            entity.Property(session => session.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(session => session.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(session => session.Campaign)
                .WithMany()
                .HasForeignKey(session => session.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(session => new
            {
                session.CampaignId,
                session.SessionNumber
            })
            .IsUnique();
        });

        modelBuilder.Entity<SessionNote>(entity =>
        {
            entity.ToTable("SessionNotes");

            entity.HasKey(note => note.Id);

            entity.Property(note => note.Id)
                .HasColumnName("session_note_id");

            entity.Property(note => note.SessionId)
                .HasColumnName("session_id");

            entity.Property(note => note.Order)
                .HasColumnName("note_order")
                .IsRequired();

            entity.Property(note => note.Type)
                .HasColumnName("note_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(note => note.Content)
                .HasColumnName("content")
                .IsRequired();

            entity.Property(note => note.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(note => note.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(note => note.Session)
                .WithMany(session => session.Notes)
                .HasForeignKey(note => note.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(note => note.Choices)
                .WithOne(choice => choice.SessionNote)
                .HasForeignKey(choice => choice.SessionNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(note => note.MechanicsChanges)
                .WithOne(change => change.SessionNote)
                .HasForeignKey(change => change.SessionNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(note => new
            {
                note.SessionId,
                note.Order
            })
            .IsUnique();
        });

        modelBuilder.Entity<SessionNoteChoice>(entity =>
        {
            entity.ToTable("SessionNoteChoices");

            entity.HasKey(choice => choice.Id);

            entity.Property(choice => choice.Id)
                .HasColumnName("session_note_choice_id");

            entity.Property(choice => choice.SessionNoteId)
                .HasColumnName("session_note_id");

            entity.Property(choice => choice.Order)
                .HasColumnName("choice_order")
                .IsRequired();

            entity.Property(choice => choice.ChoiceText)
                .HasColumnName("choice_text")
                .IsRequired();

            entity.Property(choice => choice.IsChosen)
                .HasColumnName("is_chosen")
                .IsRequired();

            entity.HasIndex(choice => new
            {
                choice.SessionNoteId,
                choice.Order
            })
            .IsUnique();

        });

        modelBuilder.Entity<SessionNoteMechanicsChange>(entity =>
        {
            entity.ToTable("SessionNoteMechanicsChanges");

            entity.HasKey(change => change.Id);

            entity.Property(change => change.Id)
                .HasColumnName("session_note_mechanics_change_id");

            entity.Property(change => change.SessionNoteId)
                .HasColumnName("session_note_id");

            entity.Property(change => change.Order)
                .HasColumnName("change_order")
                .IsRequired();

            entity.Property(change => change.PlayerId)
                .HasColumnName("player_id");

            entity.Property(change => change.ChangeText)
                .HasColumnName("change_text");

            entity.HasOne(change => change.Player)
                .WithMany()
                .HasForeignKey(change => change.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(change => new
            {
                change.SessionNoteId,
                change.Order
            })
            .IsUnique();

            entity.HasIndex(change => new
            {
                change.SessionNoteId,
                change.PlayerId
            })
            .IsUnique();
        });

        modelBuilder.Entity<CampaignQuest>(entity =>
        {
            entity.ToTable("CampaignQuests");

            entity.HasKey(quest => quest.QuestId);

            entity.Property(quest => quest.QuestId)
                .HasColumnName("quest_id");

            entity.Property(quest => quest.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(quest => quest.Type)
                .HasColumnName("quest_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(quest => quest.Title)
                .HasColumnName("title")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(quest => quest.Description)
                .HasColumnName("description")
                .HasMaxLength(2048)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(quest => quest.GivenBy)
                .HasColumnName("given_by")
                .HasMaxLength(256)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(quest => quest.Reward)
                .HasColumnName("reward")
                .HasMaxLength(2048)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(quest => quest.CompletedAt)
                .HasColumnName("completed_at");

            entity.Property(quest => quest.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(quest => quest.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasMany(quest => quest.Tasks)
                .WithOne(task => task.CampaignQuest)
                .HasForeignKey(task => task.QuestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(quest => new
            {
                quest.CampaignId,
                quest.Type,
                quest.CompletedAt
            });

            entity.HasIndex(quest => new
            {
                quest.CampaignId,
                quest.Title
            });
        });

        modelBuilder.Entity<CampaignQuestTask>(entity =>
        {
            entity.ToTable("CampaignQuestTasks");

            entity.HasKey(task => task.QuestTaskId);

            entity.Property(task => task.QuestTaskId)
                .HasColumnName("quest_task_id");

            entity.Property(task => task.QuestId)
                .HasColumnName("quest_id");

            entity.Property(task => task.Title)
                .HasColumnName("title")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(task => task.Description)
                .HasColumnName("description")
                .HasMaxLength(2048)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(task => task.DateCompleted)
                .HasColumnName("date_completed");

            entity.Property(task => task.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(task => task.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasIndex(task => new
            {
                task.QuestId,
                task.DateCompleted
            });

            entity.HasIndex(task => new
            {
                task.QuestId,
                task.Title
            });
        });

        modelBuilder.Entity<CampaignMilestone>(entity =>
        {
            entity.ToTable("CampaignMilestones");

            entity.HasKey(milestone => milestone.Id);

            entity.Property(milestone => milestone.Id)
                .HasColumnName("campaign_milestone_id");

            entity.Property(milestone => milestone.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(milestone => milestone.Title)
                .HasColumnName("title")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(milestone => milestone.Description)
                .HasColumnName("description")
                .HasMaxLength(2048);

            entity.Property(milestone => milestone.AchievedAt)
                .HasColumnName("achieved_at");

            entity.Property(milestone => milestone.Importance)
                .HasColumnName("importance")
                .HasMaxLength(32)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(milestone => milestone.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(milestone => milestone.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasIndex(milestone => new
            {
                milestone.CampaignId,
                milestone.Title
            });
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("Assets");

            entity.HasKey(asset => asset.Id);

            entity.Property(asset => asset.Id)
                .HasColumnName("asset_id");

            entity.Property(asset => asset.ParentAssetId)
                .HasColumnName("parent_asset_id");

            entity.Property(asset => asset.AssetType)
                .HasColumnName("asset_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(asset => asset.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(asset => asset.Description)
                .HasColumnName("description")
                .HasMaxLength(4096)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(asset => asset.ItemType)
                .HasColumnName("item_type")
                .HasMaxLength(64)
                .HasConversion<string>();

            entity.PrimitiveCollection(asset => asset.CampaignIds)
                .HasColumnName("campaign_ids")
                .HasColumnType("nvarchar(max)");

            entity.Property(asset => asset.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(asset => asset.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(asset => asset.ParentAsset)
                .WithMany(asset => asset.Children)
                .HasForeignKey(asset => asset.ParentAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(asset => new
            {
                asset.ParentAssetId,
                asset.Name
            })
            .IsUnique();
        });

        modelBuilder.Entity<Monster>(entity =>
        {
            entity.ToTable("Monsters");

            entity.HasKey(monster => monster.Id);

            entity.Property(monster => monster.Id)
                .HasColumnName("monster_id");

            entity.Property(monster => monster.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(monster => monster.Size)
                .HasColumnName("size")
                .HasMaxLength(128);

            entity.Property(monster => monster.Race)
                .HasColumnName("race")
                .HasMaxLength(128);

            entity.Property(monster => monster.Class)
                .HasColumnName("class")
                .HasMaxLength(128);

            entity.PrimitiveCollection(monster => monster.Tags)
                .HasColumnName("tags")
                .HasColumnType("nvarchar(max)");

            entity.Property(monster => monster.Notes)
                .HasColumnName("notes");

            entity.HasMany(monster => monster.Abilities)
                .WithOne(ability => ability.Monster)
                .HasForeignKey(ability => ability.MonsterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(monster => monster.Proficiencies)
                .WithOne(proficiency => proficiency.Monster)
                .HasForeignKey(proficiency => proficiency.MonsterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(monster => monster.Features)
                .WithOne(feature => feature.Monster)
                .HasForeignKey(feature => feature.MonsterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(monster => monster.Spellcasting)
                .WithOne(spellcasting => spellcasting.Monster)
                .HasForeignKey<MonsterSpellcasting>(spellcasting => spellcasting.MonsterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(monster => monster.Name);
        });

        modelBuilder.Entity<MonsterAbility>(entity =>
        {
            entity.ToTable("MonsterAbilities");

            entity.HasKey(ability => ability.Id);

            entity.Property(ability => ability.Id)
                .HasColumnName("monster_ability_id");

            entity.Property(ability => ability.MonsterId)
                .HasColumnName("monster_id");

            entity.Property(ability => ability.Name)
                .HasColumnName("name")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(ability => ability.Value)
                .HasColumnName("value");

            entity.Property(ability => ability.Modifier)
                .HasColumnName("modifier");

            entity.Property(ability => ability.Notes)
                .HasColumnName("notes");

            entity.HasIndex(ability => new
            {
                ability.MonsterId,
                ability.Name
            });
        });

        modelBuilder.Entity<MonsterProficiency>(entity =>
        {
            entity.ToTable("MonsterProficiencies");

            entity.HasKey(proficiency => proficiency.Id);

            entity.Property(proficiency => proficiency.Id)
                .HasColumnName("monster_proficiency_id");

            entity.Property(proficiency => proficiency.MonsterId)
                .HasColumnName("monster_id");

            entity.Property(proficiency => proficiency.Name)
                .HasColumnName("name")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(proficiency => proficiency.Bonus)
                .HasColumnName("bonus");

            entity.Property(proficiency => proficiency.Notes)
                .HasColumnName("notes");

            entity.HasIndex(proficiency => new
            {
                proficiency.MonsterId,
                proficiency.Name
            });
        });

        modelBuilder.Entity<MonsterFeature>(entity =>
        {
            entity.ToTable("MonsterFeatures");

            entity.HasKey(feature => feature.Id);

            entity.Property(feature => feature.Id)
                .HasColumnName("monster_feature_id");

            entity.Property(feature => feature.MonsterId)
                .HasColumnName("monster_id");

            entity.Property(feature => feature.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(feature => feature.Description)
                .HasColumnName("description");

            entity.Property(feature => feature.Category)
                .HasColumnName("category")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(feature => feature.UsageNote)
                .HasColumnName("usage_note");

            entity.Property(feature => feature.ResourceCost)
                .HasColumnName("resource_cost");

            entity.Property(feature => feature.IsSpell)
                .HasColumnName("is_spell")
                .IsRequired();

            entity.Property(feature => feature.SpellLevel)
                .HasColumnName("spell_level");

            entity.Property(feature => feature.CastingTime)
                .HasColumnName("casting_time")
                .HasMaxLength(128);

            entity.Property(feature => feature.Range)
                .HasColumnName("range")
                .HasMaxLength(128);

            entity.Property(feature => feature.Duration)
                .HasColumnName("duration")
                .HasMaxLength(128);

            entity.Property(feature => feature.Concentration)
                .HasColumnName("concentration");

            entity.Property(feature => feature.SortOrder)
                .HasColumnName("sort_order")
                .IsRequired();

            entity.HasIndex(feature => new
            {
                feature.MonsterId,
                feature.SortOrder
            });
        });

        modelBuilder.Entity<MonsterSpellcasting>(entity =>
        {
            entity.ToTable("MonsterSpellcasting");

            entity.HasKey(spellcasting => spellcasting.MonsterId);

            entity.Property(spellcasting => spellcasting.MonsterId)
                .HasColumnName("monster_id");

            entity.Property(spellcasting => spellcasting.SpellcastingAbility)
                .HasColumnName("spellcasting_ability")
                .HasMaxLength(128);

            entity.Property(spellcasting => spellcasting.SpellSaveDC)
                .HasColumnName("spell_save_dc");

            entity.Property(spellcasting => spellcasting.SpellAttackBonus)
                .HasColumnName("spell_attack_bonus");

            entity.Property(spellcasting => spellcasting.Notes)
                .HasColumnName("notes");

            entity.HasMany(spellcasting => spellcasting.SpellSlots)
                .WithOne(slot => slot.Spellcasting)
                .HasForeignKey(slot => slot.MonsterSpellcastingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MonsterSpellSlot>(entity =>
        {
            entity.ToTable("MonsterSpellSlots");

            entity.HasKey(slot => slot.Id);

            entity.Property(slot => slot.Id)
                .HasColumnName("monster_spell_slot_id");

            entity.Property(slot => slot.MonsterSpellcastingId)
                .HasColumnName("monster_spellcasting_id");

            entity.Property(slot => slot.SpellLevel)
                .HasColumnName("spell_level")
                .IsRequired();

            entity.Property(slot => slot.MaximumSlots)
                .HasColumnName("maximum_slots");

            entity.Property(slot => slot.RemainingSlots)
                .HasColumnName("remaining_slots");

            entity.HasIndex(slot => new
            {
                slot.MonsterSpellcastingId,
                slot.SpellLevel
            })
            .IsUnique();
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
