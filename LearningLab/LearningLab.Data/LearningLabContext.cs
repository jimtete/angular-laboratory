using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Assets;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Campaign.Maps;
using LearningLab.Data.Models.Campaign.Presentation;
using LearningLab.Data.Models.Campaign.Quests;
using LearningLab.Data.Models.Campaign.Rules;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.Campaign.Stores;
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
    public DbSet<StoryBeatQuestTask> StoryBeatQuestTasks { get; set; }
    public DbSet<CampaignSession> CampaignSessions { get; set; }
    public DbSet<SessionNote> SessionNotes { get; set; }
    public DbSet<SessionNoteChoice> SessionNoteChoices { get; set; }
    public DbSet<SessionNoteMechanicsChange> SessionNoteMechanicsChanges { get; set; }
    public DbSet<SessionNoteStoryBeatReference> SessionNoteStoryBeatReferences { get; set; }
    public DbSet<CampaignMilestone> CampaignMilestones { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Map> Maps { get; set; }
    public DbSet<MapCampaign> MapCampaigns { get; set; }
    public DbSet<MapPin> MapPins { get; set; }
    public DbSet<MapPinConnection> MapPinConnections { get; set; }
    public DbSet<StoryBlock> StoryBlocks { get; set; }
    public DbSet<StoryBeat> StoryBeats { get; set; }
    public DbSet<StoryBeatIndexPathRule> StoryBeatIndexPathRules { get; set; }
    public DbSet<StoryBlockMilestone> StoryBlockMilestones { get; set; }
    public DbSet<CampaignPresentation> CampaignPresentations { get; set; }
    public DbSet<CampaignPresentationEntry> CampaignPresentationEntries { get; set; }
    public DbSet<CampaignPresentationStoryBeatSelection> CampaignPresentationStoryBeatSelections { get; set; }
    public DbSet<CampaignSettings> CampaignSettings { get; set; }
    public DbSet<CampaignEventDefinition> CampaignEventDefinitions { get; set; }
    public DbSet<CampaignEventOption> CampaignEventOptions { get; set; }
    public DbSet<CampaignEventState> CampaignEventStates { get; set; }
    public DbSet<ConditionalRule> ConditionalRules { get; set; }
    public DbSet<ConditionGroup> ConditionGroups { get; set; }
    public DbSet<ConditionClause> ConditionClauses { get; set; }
    public DbSet<StoryOutcomeEffect> StoryOutcomeEffects { get; set; }
    public DbSet<CampaignChoiceDefinition> CampaignChoiceDefinitions { get; set; }
    public DbSet<CampaignChoiceOption> CampaignChoiceOptions { get; set; }
    public DbSet<CampaignChoiceSelection> CampaignChoiceSelections { get; set; }
    public DbSet<StoreEntry> StoreEntries { get; set; }
    public DbSet<StoreItem> StoreItems { get; set; }
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

            entity.HasMany(campaign => campaign.EventDefinitions)
                .WithOne(definition => definition.Campaign)
                .HasForeignKey(definition => definition.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(campaign => campaign.ConditionalRules)
                .WithOne(rule => rule.Campaign)
                .HasForeignKey(rule => rule.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(campaign => campaign.OutcomeEffects)
                .WithOne(effect => effect.Campaign)
                .HasForeignKey(effect => effect.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(campaign => campaign.ChoiceDefinitions)
                .WithOne(choice => choice.Campaign)
                .HasForeignKey(choice => choice.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

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

            entity.Property(block => block.OrderIndex)
                .HasColumnName("order_index")
                .IsRequired();

            entity.HasMany(block => block.Beats)
                .WithOne(beat => beat.StoryBlock)
                .HasForeignKey(beat => beat.StoryBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(block => block.Milestones)
                .WithOne(milestone => milestone.StoryBlock)
                .HasForeignKey(milestone => milestone.StoryBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(block => block.IndexPathRules)
                .WithOne(rule => rule.StoryBlock)
                .HasForeignKey(rule => rule.StoryBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(block => block.CampaignId);

            entity.HasIndex(block => new
            {
                block.CampaignId,
                block.OrderIndex
            })
            .IsUnique();
        });

        modelBuilder.Entity<StoryBeatIndexPathRule>(entity =>
        {
            entity.ToTable("StoryBeatIndexPathRules");

            entity.HasKey(rule => rule.Id);

            entity.Property(rule => rule.Id)
                .HasColumnName("story_beat_index_path_rule_id");

            entity.Property(rule => rule.CampaignId)
                .HasColumnName("campaign_id")
                .IsRequired();

            entity.Property(rule => rule.StoryBlockId)
                .HasColumnName("story_block_id")
                .IsRequired();

            entity.Property(rule => rule.OrderIndex)
                .HasColumnName("order_index")
                .IsRequired();

            entity.Property(rule => rule.RelationType)
                .HasColumnName("relation_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(rule => rule.IsRequired)
                .HasColumnName("is_required")
                .IsRequired();

            entity.Property(rule => rule.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(rule => rule.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(rule => rule.Campaign)
                .WithMany(campaign => campaign.StoryBeatIndexPathRules)
                .HasForeignKey(rule => rule.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rule => rule.StoryBlock)
                .WithMany(block => block.IndexPathRules)
                .HasForeignKey(rule => rule.StoryBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rule => rule.CampaignId);
            entity.HasIndex(rule => rule.StoryBlockId);
            entity.HasIndex(rule => new
            {
                rule.CampaignId,
                rule.StoryBlockId,
                rule.OrderIndex
            })
            .IsUnique();
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

            entity.Property(beat => beat.SecondaryOrderIndex)
                .HasColumnName("secondary_order_index")
                .HasDefaultValue(1)
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
                    optionalInformation.Property(optional => optional.Id)
                        .IsRequired();

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
                    npc.Property(item => item.Id)
                        .IsRequired();

                    npc.Property(item => item.NpcTag)
                        .IsRequired();
                });

                roleplaying.OwnsMany(content => content.DiscoverableInformation, information =>
                {
                    information.Property(item => item.Id)
                        .IsRequired();

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
                    option.Property(item => item.Id)
                        .IsRequired();

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

            entity.OwnsOne(beat => beat.Combat, combat =>
            {
                combat.ToJson("combat");

                combat.Property(content => content.Description)
                    .IsRequired();

                combat.Property(content => content.Rewards);

                combat.OwnsMany(content => content.EnemyNpcs, enemyNpc =>
                {
                    enemyNpc.Property(item => item.MonsterId)
                        .IsRequired();

                    enemyNpc.Property(item => item.Amount)
                        .IsRequired();
                });
            });

            entity.OwnsOne(beat => beat.Transition, transition =>
            {
                transition.ToJson("transition");

                transition.Property(content => content.Description)
                    .IsRequired();
            });

            entity.HasIndex(beat => beat.StoryBlockId);

            entity.HasIndex(beat => new
            {
                beat.StoryBlockId,
                beat.OrderIndex,
                beat.SecondaryOrderIndex
            })
            .IsUnique();

            entity.HasIndex(beat => beat.CampaignMilestoneId)
                .IsUnique()
                .HasFilter("[campaign_milestone_id] IS NOT NULL");
        });

        modelBuilder.Entity<CampaignEventDefinition>(entity =>
        {
            entity.ToTable("CampaignEventDefinitions");

            entity.HasKey(definition => definition.Id);

            entity.Property(definition => definition.Id)
                .HasColumnName("campaign_event_definition_id");

            entity.Property(definition => definition.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(definition => definition.Key)
                .HasColumnName("key")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(definition => definition.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(definition => definition.Description)
                .HasColumnName("description")
                .HasMaxLength(2048);

            entity.Property(definition => definition.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(definition => definition.IsRepeatable)
                .HasColumnName("is_repeatable")
                .IsRequired();

            entity.Property(definition => definition.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(definition => definition.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasMany(definition => definition.Options)
                .WithOne(option => option.CampaignEventDefinition)
                .HasForeignKey(option => option.CampaignEventDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(definition => definition.States)
                .WithOne(state => state.CampaignEventDefinition)
                .HasForeignKey(state => state.CampaignEventDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(definition => new
            {
                definition.CampaignId,
                definition.Key
            })
            .IsUnique();
        });

        modelBuilder.Entity<CampaignEventOption>(entity =>
        {
            entity.ToTable("CampaignEventOptions");

            entity.HasKey(option => option.Id);

            entity.Property(option => option.Id)
                .HasColumnName("campaign_event_option_id");

            entity.Property(option => option.CampaignEventDefinitionId)
                .HasColumnName("campaign_event_definition_id");

            entity.Property(option => option.Key)
                .HasColumnName("key")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(option => option.Label)
                .HasColumnName("label")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(option => option.Description)
                .HasColumnName("description")
                .HasMaxLength(2048);

            entity.Property(option => option.SortOrder)
                .HasColumnName("sort_order")
                .IsRequired();

            entity.HasIndex(option => new
            {
                option.CampaignEventDefinitionId,
                option.Key
            })
            .IsUnique();
        });

        modelBuilder.Entity<CampaignEventState>(entity =>
        {
            entity.ToTable("CampaignEventStates");

            entity.HasKey(state => state.Id);

            entity.Property(state => state.Id)
                .HasColumnName("campaign_event_state_id");

            entity.Property(state => state.CampaignSessionId)
                .HasColumnName("campaign_session_id");

            entity.Property(state => state.CampaignEventDefinitionId)
                .HasColumnName("campaign_event_definition_id");

            entity.Property(state => state.BooleanValue)
                .HasColumnName("boolean_value");

            entity.Property(state => state.SelectedOptionId)
                .HasColumnName("selected_option_id");

            entity.Property(state => state.TextValue)
                .HasColumnName("text_value")
                .HasMaxLength(4096);

            entity.Property(state => state.NumericValue)
                .HasColumnName("numeric_value")
                .HasPrecision(18, 4);

            entity.Property(state => state.SourceStoryBlockId)
                .HasColumnName("source_story_block_id");

            entity.Property(state => state.SourceStoryBeatId)
                .HasColumnName("source_story_beat_id");

            entity.Property(state => state.ResolvedAtUtc)
                .HasColumnName("resolved_at_utc")
                .IsRequired();

            entity.Property(state => state.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .IsRequired();

            entity.HasOne(state => state.CampaignSession)
                .WithMany(session => session.EventStates)
                .HasForeignKey(state => state.CampaignSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(state => state.SelectedOption)
                .WithMany()
                .HasForeignKey(state => state.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(state => state.SourceStoryBlock)
                .WithMany()
                .HasForeignKey(state => state.SourceStoryBlockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(state => state.SourceStoryBeat)
                .WithMany()
                .HasForeignKey(state => state.SourceStoryBeatId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(state => new
            {
                state.CampaignSessionId,
                state.CampaignEventDefinitionId
            })
            .IsUnique();
        });

        modelBuilder.Entity<ConditionalRule>(entity =>
        {
            entity.ToTable("ConditionalRules");

            entity.HasKey(rule => rule.Id);

            entity.Property(rule => rule.Id)
                .HasColumnName("conditional_rule_id");

            entity.Property(rule => rule.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(rule => rule.TargetType)
                .HasColumnName("target_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(rule => rule.TargetId)
                .HasColumnName("target_id");

            entity.Property(rule => rule.RootConditionGroupId)
                .HasColumnName("root_condition_group_id");

            entity.Property(rule => rule.EffectType)
                .HasColumnName("effect_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(rule => rule.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(rule => rule.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(rule => rule.RootConditionGroup)
                .WithMany()
                .HasForeignKey(rule => rule.RootConditionGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(rule => new
            {
                rule.TargetType,
                rule.TargetId
            });
        });

        modelBuilder.Entity<ConditionGroup>(entity =>
        {
            entity.ToTable("ConditionGroups");

            entity.HasKey(group => group.Id);

            entity.Property(group => group.Id)
                .HasColumnName("condition_group_id");

            entity.Property(group => group.ParentConditionGroupId)
                .HasColumnName("parent_condition_group_id");

            entity.Property(group => group.Operator)
                .HasColumnName("operator")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(group => group.Negate)
                .HasColumnName("negate")
                .IsRequired();

            entity.Property(group => group.SortOrder)
                .HasColumnName("sort_order")
                .IsRequired();

            entity.HasOne(group => group.ParentConditionGroup)
                .WithMany(group => group.Groups)
                .HasForeignKey(group => group.ParentConditionGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConditionClause>(entity =>
        {
            entity.ToTable("ConditionClauses");

            entity.HasKey(clause => clause.Id);

            entity.Property(clause => clause.Id)
                .HasColumnName("condition_clause_id");

            entity.Property(clause => clause.ConditionGroupId)
                .HasColumnName("condition_group_id");

            entity.Property(clause => clause.CampaignEventDefinitionId)
                .HasColumnName("campaign_event_definition_id");

            entity.Property(clause => clause.ComparisonOperator)
                .HasColumnName("comparison_operator")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(clause => clause.BooleanValue)
                .HasColumnName("boolean_value");

            entity.Property(clause => clause.ExpectedOptionId)
                .HasColumnName("expected_option_id");

            entity.Property(clause => clause.TextValue)
                .HasColumnName("text_value")
                .HasMaxLength(4096);

            entity.Property(clause => clause.NumericValue)
                .HasColumnName("numeric_value")
                .HasPrecision(18, 4);

            entity.Property(clause => clause.SortOrder)
                .HasColumnName("sort_order")
                .IsRequired();

            entity.HasOne(clause => clause.ConditionGroup)
                .WithMany(group => group.Clauses)
                .HasForeignKey(clause => clause.ConditionGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(clause => clause.CampaignEventDefinition)
                .WithMany()
                .HasForeignKey(clause => clause.CampaignEventDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(clause => clause.ExpectedOption)
                .WithMany()
                .HasForeignKey(clause => clause.ExpectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(clause => clause.CampaignEventDefinitionId);
        });

        modelBuilder.Entity<StoryOutcomeEffect>(entity =>
        {
            entity.ToTable("StoryOutcomeEffects");

            entity.HasKey(effect => effect.Id);

            entity.Property(effect => effect.Id)
                .HasColumnName("story_outcome_effect_id");

            entity.Property(effect => effect.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(effect => effect.SourceType)
                .HasColumnName("source_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(effect => effect.SourceId)
                .HasColumnName("source_id");

            entity.Property(effect => effect.CampaignEventDefinitionId)
                .HasColumnName("campaign_event_definition_id");

            entity.Property(effect => effect.OperationType)
                .HasColumnName("operation_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(effect => effect.BooleanValue)
                .HasColumnName("boolean_value");

            entity.Property(effect => effect.SelectedOptionId)
                .HasColumnName("selected_option_id");

            entity.Property(effect => effect.TextValue)
                .HasColumnName("text_value")
                .HasMaxLength(4096);

            entity.Property(effect => effect.NumericValue)
                .HasColumnName("numeric_value")
                .HasPrecision(18, 4);

            entity.Property(effect => effect.SortOrder)
                .HasColumnName("sort_order")
                .IsRequired();

            entity.HasOne(effect => effect.CampaignEventDefinition)
                .WithMany()
                .HasForeignKey(effect => effect.CampaignEventDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(effect => effect.SelectedOption)
                .WithMany()
                .HasForeignKey(effect => effect.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(effect => new
            {
                effect.SourceType,
                effect.SourceId
            });
        });

        modelBuilder.Entity<CampaignChoiceDefinition>(entity =>
        {
            entity.ToTable("CampaignChoiceDefinitions");

            entity.HasKey(choice => choice.Id);

            entity.Property(choice => choice.Id)
                .HasColumnName("campaign_choice_definition_id");

            entity.Property(choice => choice.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(choice => choice.StoryBlockId)
                .HasColumnName("story_block_id");

            entity.Property(choice => choice.StoryBeatId)
                .HasColumnName("story_beat_id");

            entity.Property(choice => choice.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(choice => choice.SelectionMode)
                .HasColumnName("selection_mode")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.HasOne(choice => choice.StoryBlock)
                .WithMany()
                .HasForeignKey(choice => choice.StoryBlockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(choice => choice.StoryBeat)
                .WithMany()
                .HasForeignKey(choice => choice.StoryBeatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CampaignChoiceOption>(entity =>
        {
            entity.ToTable("CampaignChoiceOptions");

            entity.HasKey(option => option.Id);

            entity.Property(option => option.Id)
                .HasColumnName("campaign_choice_option_id");

            entity.Property(option => option.CampaignChoiceDefinitionId)
                .HasColumnName("campaign_choice_definition_id");

            entity.Property(option => option.StoryBeatId)
                .HasColumnName("story_beat_id");

            entity.Property(option => option.Key)
                .HasColumnName("key")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(option => option.Label)
                .HasColumnName("label")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(option => option.Description)
                .HasColumnName("description")
                .HasMaxLength(2048);

            entity.Property(option => option.SortOrder)
                .HasColumnName("sort_order")
                .IsRequired();

            entity.HasOne(option => option.CampaignChoiceDefinition)
                .WithMany(choice => choice.Options)
                .HasForeignKey(option => option.CampaignChoiceDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(option => option.StoryBeat)
                .WithMany()
                .HasForeignKey(option => option.StoryBeatId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(option => new
            {
                option.CampaignChoiceDefinitionId,
                option.Key
            })
            .IsUnique();
        });

        modelBuilder.Entity<CampaignChoiceSelection>(entity =>
        {
            entity.ToTable("CampaignChoiceSelections");

            entity.HasKey(selection => selection.Id);

            entity.Property(selection => selection.Id)
                .HasColumnName("campaign_choice_selection_id");

            entity.Property(selection => selection.CampaignSessionId)
                .HasColumnName("campaign_session_id");

            entity.Property(selection => selection.CampaignChoiceDefinitionId)
                .HasColumnName("campaign_choice_definition_id");

            entity.Property(selection => selection.CampaignChoiceOptionId)
                .HasColumnName("campaign_choice_option_id");

            entity.Property(selection => selection.SelectedAtUtc)
                .HasColumnName("selected_at_utc")
                .IsRequired();

            entity.HasOne(selection => selection.CampaignSession)
                .WithMany(session => session.ChoiceSelections)
                .HasForeignKey(selection => selection.CampaignSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(selection => selection.CampaignChoiceDefinition)
                .WithMany()
                .HasForeignKey(selection => selection.CampaignChoiceDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(selection => selection.CampaignChoiceOption)
                .WithMany()
                .HasForeignKey(selection => selection.CampaignChoiceOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(selection => new
            {
                selection.CampaignSessionId,
                selection.CampaignChoiceDefinitionId,
                selection.CampaignChoiceOptionId
            })
            .IsUnique();
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
                .IsRequired();

            entity.Property(settings => settings.StoreMechanics)
                .HasColumnName("store_mechanics")
                .HasMaxLength(64)
                .HasConversion<string>()
                .HasDefaultValue(StoreMechanics.GlobalStores)
                .IsRequired();
        });

        modelBuilder.Entity<StoreEntry>(entity =>
        {
            entity.ToTable("StoreEntries");

            entity.HasKey(store => store.StoreId);

            entity.Property(store => store.StoreId)
                .HasColumnName("store_id");

            entity.Property(store => store.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(store => store.StoreType)
                .HasColumnName("store_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(store => store.StoreLocation)
                .HasColumnName("store_location")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(store => store.StoreName)
                .HasColumnName("store_name")
                .HasMaxLength(256);

            entity.Property(store => store.StoreDescription)
                .HasColumnName("store_description")
                .HasMaxLength(4096);

            entity.HasIndex(store => store.CampaignId);

            entity.HasMany(store => store.Items)
                .WithOne(item => item.Store)
                .HasForeignKey(item => item.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(store => store.Campaign)
                .WithMany(campaign => campaign.Stores)
                .HasForeignKey(store => store.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StoreItem>(entity =>
        {
            entity.ToTable("StoreItems");

            entity.HasKey(item => item.StoreItemId);

            entity.Property(item => item.StoreItemId)
                .HasColumnName("store_item_id");

            entity.Property(item => item.StoreId)
                .HasColumnName("store_id");

            entity.Property(item => item.Quantity)
                .HasColumnName("quantity");

            entity.Property(item => item.TimesSold)
                .HasColumnName("times_sold")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(item => item.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(item => item.ItemDescription)
                .HasColumnName("item_description")
                .HasMaxLength(4096);

            entity.Property(item => item.ItemPrice)
                .HasColumnName("item_price")
                .IsRequired();

            entity.Property(item => item.ItemPriceDiscount)
                .HasColumnName("item_price_discount")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(item => item.ItemPricePercentageDiscount)
                .HasColumnName("item_price_percentage_discount")
                .HasDefaultValue(0)
                .IsRequired();

            entity.HasIndex(item => item.StoreId);
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

        modelBuilder.Entity<CampaignPresentation>(entity =>
        {
            entity.ToTable("CampaignPresentations");

            entity.HasKey(presentation => presentation.Id);

            entity.Property(presentation => presentation.Id)
                .HasColumnName("campaign_presentation_id");

            entity.Property(presentation => presentation.CampaignSessionId)
                .HasColumnName("campaign_session_id");

            entity.Property(presentation => presentation.Status)
                .HasColumnName("status")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(presentation => presentation.ActiveStoryBlockId)
                .HasColumnName("active_story_block_id");

            entity.Property(presentation => presentation.CurrentStoryBeatId)
                .HasColumnName("current_story_beat_id");

            entity.Property(presentation => presentation.StartedAt)
                .HasColumnName("started_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(presentation => presentation.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(presentation => presentation.EndedAt)
                .HasColumnName("ended_at");

            entity.HasOne(presentation => presentation.CampaignSession)
                .WithOne(session => session.Presentation)
                .HasForeignKey<CampaignPresentation>(presentation => presentation.CampaignSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(presentation => presentation.ActiveStoryBlock)
                .WithMany(block => block.ActivePresentations)
                .HasForeignKey(presentation => presentation.ActiveStoryBlockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(presentation => presentation.CurrentStoryBeat)
                .WithMany(beat => beat.CurrentPresentations)
                .HasForeignKey(presentation => presentation.CurrentStoryBeatId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(presentation => presentation.Entries)
                .WithOne(entry => entry.CampaignPresentation)
                .HasForeignKey(entry => entry.CampaignPresentationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(presentation => presentation.StoryBeatSelections)
                .WithOne(selection => selection.CampaignPresentation)
                .HasForeignKey(selection => selection.CampaignPresentationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(presentation => presentation.CampaignSessionId)
                .IsUnique();

            entity.HasIndex(presentation => new
            {
                presentation.Status,
                presentation.UpdatedAt
            });
        });

        modelBuilder.Entity<CampaignPresentationEntry>(entity =>
        {
            entity.ToTable("CampaignPresentationEntries");

            entity.HasKey(entry => entry.Id);

            entity.Property(entry => entry.Id)
                .HasColumnName("campaign_presentation_entry_id");

            entity.Property(entry => entry.CampaignPresentationId)
                .HasColumnName("campaign_presentation_id");

            entity.Property(entry => entry.Sequence)
                .HasColumnName("sequence")
                .IsRequired();

            entity.Property(entry => entry.EntryType)
                .HasColumnName("entry_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(entry => entry.StoryBlockId)
                .HasColumnName("story_block_id");

            entity.Property(entry => entry.StoryBeatId)
                .HasColumnName("story_beat_id");

            entity.Property(entry => entry.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(entry => entry.StoryBlock)
                .WithMany(block => block.PresentationEntries)
                .HasForeignKey(entry => entry.StoryBlockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(entry => entry.StoryBeat)
                .WithMany(beat => beat.PresentationEntries)
                .HasForeignKey(entry => entry.StoryBeatId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(entry => new
            {
                entry.CampaignPresentationId,
                entry.Sequence
            })
            .IsUnique();

            entity.HasIndex(entry => new
            {
                entry.CampaignPresentationId,
                entry.CreatedAt
            });
        });

        modelBuilder.Entity<CampaignPresentationStoryBeatSelection>(entity =>
        {
            entity.ToTable("CampaignPresentationStoryBeatSelections");

            entity.HasKey(selection => selection.Id);

            entity.Property(selection => selection.Id)
                .HasColumnName("campaign_presentation_story_beat_selection_id");

            entity.Property(selection => selection.CampaignPresentationId)
                .HasColumnName("campaign_presentation_id");

            entity.Property(selection => selection.StoryBlockId)
                .HasColumnName("story_block_id");

            entity.Property(selection => selection.OrderIndex)
                .HasColumnName("order_index")
                .IsRequired();

            entity.Property(selection => selection.SelectedSecondaryOrderIndex)
                .HasColumnName("selected_secondary_order_index")
                .IsRequired();

            entity.Property(selection => selection.SelectedStoryBeatId)
                .HasColumnName("selected_story_beat_id");

            entity.Property(selection => selection.SelectedAt)
                .HasColumnName("selected_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(selection => selection.StoryBlock)
                .WithMany(block => block.PresentationStoryBeatSelections)
                .HasForeignKey(selection => selection.StoryBlockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(selection => selection.SelectedStoryBeat)
                .WithMany(beat => beat.SelectedInPresentations)
                .HasForeignKey(selection => selection.SelectedStoryBeatId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(selection => new
            {
                selection.CampaignPresentationId,
                selection.StoryBlockId,
                selection.OrderIndex
            })
            .IsUnique();

            entity.HasIndex(selection => selection.SelectedStoryBeatId);
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

            entity.Property(note => note.StoryBeatId)
                .HasColumnName("story_beat_id");

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

            entity.HasOne(note => note.StoryBeat)
                .WithMany()
                .HasForeignKey(note => note.StoryBeatId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(note => note.Choices)
                .WithOne(choice => choice.SessionNote)
                .HasForeignKey(choice => choice.SessionNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(note => note.MechanicsChanges)
                .WithOne(change => change.SessionNote)
                .HasForeignKey(change => change.SessionNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(note => note.StoryBeatReferences)
                .WithOne(reference => reference.SessionNote)
                .HasForeignKey(reference => reference.SessionNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(note => new
            {
                note.SessionId,
                note.Order
            })
            .IsUnique();

            entity.HasIndex(note => note.StoryBeatId);
        });

        modelBuilder.Entity<SessionNoteStoryBeatReference>(entity =>
        {
            entity.ToTable("SessionNoteStoryBeatReferences");

            entity.HasKey(reference => reference.Id);

            entity.Property(reference => reference.Id)
                .HasColumnName("session_note_story_beat_reference_id");

            entity.Property(reference => reference.SessionNoteId)
                .HasColumnName("session_note_id")
                .IsRequired();

            entity.Property(reference => reference.StoryBeatId)
                .HasColumnName("story_beat_id")
                .IsRequired();

            entity.Property(reference => reference.ReferenceType)
                .HasColumnName("reference_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(reference => reference.ReferenceId)
                .HasColumnName("reference_id");

            entity.Property(reference => reference.ReferenceOutcome)
                .HasColumnName("reference_outcome")
                .HasMaxLength(64)
                .HasConversion<string>()
                .HasDefaultValue(SessionNoteStoryBeatReferenceOutcome.Presented)
                .IsRequired();

            entity.Property(reference => reference.NpcTag)
                .HasColumnName("npc_tag")
                .HasMaxLength(128);

            entity.Property(reference => reference.ContentSnapshot)
                .HasColumnName("content_snapshot")
                .IsRequired();

            entity.Property(reference => reference.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(reference => reference.StoryBeat)
                .WithMany()
                .HasForeignKey(reference => reference.StoryBeatId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(reference => reference.SessionNoteId);
            entity.HasIndex(reference => reference.StoryBeatId);
            entity.HasIndex(reference => new
            {
                reference.StoryBeatId,
                reference.ReferenceType,
                reference.ReferenceId
            });
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

        modelBuilder.Entity<StoryBeatQuestTask>(entity =>
        {
            entity.ToTable("StoryBeatQuestTasks");

            entity.HasKey(link => new
            {
                link.StoryBeatId,
                link.QuestTaskId
            });

            entity.Property(link => link.StoryBeatId)
                .HasColumnName("story_beat_id");

            entity.Property(link => link.QuestTaskId)
                .HasColumnName("quest_task_id");

            entity.Property(link => link.LinkedAt)
                .HasColumnName("linked_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(link => link.StoryBeat)
                .WithMany(beat => beat.QuestTaskLinks)
                .HasForeignKey(link => link.StoryBeatId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(link => link.QuestTask)
                .WithMany(task => task.StoryBeatLinks)
                .HasForeignKey(link => link.QuestTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(link => link.QuestTaskId);
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

            entity.Property(asset => asset.AssetUrl)
                .HasColumnName("asset_url")
                .HasMaxLength(1024);

            entity.Property(asset => asset.ContentType)
                .HasColumnName("content_type")
                .HasMaxLength(128);

            entity.Property(asset => asset.FileSizeBytes)
                .HasColumnName("file_size_bytes");

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

        modelBuilder.Entity<Map>(entity =>
        {
            entity.ToTable("Maps");

            entity.HasKey(map => map.Id);

            entity.Property(map => map.Id)
                .HasColumnName("map_id");

            entity.Property(map => map.ParentMapId)
                .HasColumnName("parent_map_id");

            entity.Property(map => map.AssetId)
                .HasColumnName("asset_id")
                .IsRequired();

            entity.Property(map => map.Category)
                .HasColumnName("category")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(map => map.ImageWidthPixels)
                .HasColumnName("image_width_pixels")
                .IsRequired();

            entity.Property(map => map.ImageHeightPixels)
                .HasColumnName("image_height_pixels")
                .IsRequired();

            entity.Property(map => map.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(map => map.Description)
                .HasColumnName("description")
                .HasMaxLength(4096)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(map => map.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(map => map.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(map => map.Asset)
                .WithMany(asset => asset.Maps)
                .HasForeignKey(map => map.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(map => map.ParentMap)
                .WithMany(map => map.ChildMaps)
                .HasForeignKey(map => map.ParentMapId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(map => map.AssetId)
                .IsUnique();

            entity.HasIndex(map => map.ParentMapId);

            entity.HasIndex(map => map.Name);
        });

        modelBuilder.Entity<MapCampaign>(entity =>
        {
            entity.ToTable("MapCampaigns");

            entity.HasKey(mapCampaign => new
            {
                mapCampaign.MapId,
                mapCampaign.CampaignId
            });

            entity.Property(mapCampaign => mapCampaign.MapId)
                .HasColumnName("map_id");

            entity.Property(mapCampaign => mapCampaign.CampaignId)
                .HasColumnName("campaign_id");

            entity.Property(mapCampaign => mapCampaign.DateAdded)
                .HasColumnName("date_added")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(mapCampaign => mapCampaign.Map)
                .WithMany(map => map.Campaigns)
                .HasForeignKey(mapCampaign => mapCampaign.MapId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(mapCampaign => mapCampaign.Campaign)
                .WithMany(campaign => campaign.MapCampaigns)
                .HasForeignKey(mapCampaign => mapCampaign.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(mapCampaign => mapCampaign.CampaignId);
        });

        modelBuilder.Entity<MapPin>(entity =>
        {
            entity.ToTable("MapPins");

            entity.HasKey(pin => pin.Id);

            entity.Property(pin => pin.Id)
                .HasColumnName("map_pin_id");

            entity.Property(pin => pin.MapId)
                .HasColumnName("map_id")
                .IsRequired();

            entity.Property(pin => pin.XCoordinate)
                .HasColumnName("x_coordinate")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            entity.Property(pin => pin.YCoordinate)
                .HasColumnName("y_coordinate")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            entity.Property(pin => pin.Label)
                .HasColumnName("label")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(pin => pin.Description)
                .HasColumnName("description")
                .HasMaxLength(4096)
                .HasDefaultValue("")
                .IsRequired();

            entity.Property(pin => pin.TargetType)
                .HasColumnName("target_type")
                .HasMaxLength(64)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(pin => pin.TargetId)
                .HasColumnName("target_id")
                .HasMaxLength(128);

            entity.Property(pin => pin.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(pin => pin.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(pin => pin.Map)
                .WithMany(map => map.Pins)
                .HasForeignKey(pin => pin.MapId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pin => pin.MapId);
        });

        modelBuilder.Entity<MapPinConnection>(entity =>
        {
            entity.ToTable("MapPinConnections");

            entity.HasKey(connection => connection.Id);

            entity.Property(connection => connection.Id)
                .HasColumnName("map_pin_connection_id");

            entity.Property(connection => connection.MapId)
                .HasColumnName("map_id")
                .IsRequired();

            entity.Property(connection => connection.MapPinAId)
                .HasColumnName("map_pin_a_id")
                .IsRequired();

            entity.Property(connection => connection.MapPinBId)
                .HasColumnName("map_pin_b_id")
                .IsRequired();

            entity.Property(connection => connection.DistanceValue)
                .HasColumnName("distance_value")
                .HasColumnType("decimal(18,2)");

            entity.Property(connection => connection.DistanceUnit)
                .HasColumnName("distance_unit")
                .HasMaxLength(64)
                .HasConversion<string>();

            entity.Property(connection => connection.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.Property(connection => connection.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                .IsRequired();

            entity.HasOne(connection => connection.Map)
                .WithMany(map => map.PinConnections)
                .HasForeignKey(connection => connection.MapId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(connection => connection.MapPinA)
                .WithMany(pin => pin.ConnectionsAsA)
                .HasForeignKey(connection => connection.MapPinAId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(connection => connection.MapPinB)
                .WithMany(pin => pin.ConnectionsAsB)
                .HasForeignKey(connection => connection.MapPinBId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(connection => connection.MapId);

            entity.HasIndex(connection => connection.MapPinAId);

            entity.HasIndex(connection => connection.MapPinBId);

            entity.HasIndex(connection => new
            {
                connection.MapId,
                connection.MapPinAId,
                connection.MapPinBId
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
