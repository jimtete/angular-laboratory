using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_RemoveCampaignSession_AndStuff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignChoiceDefinitions",
                columns: table => new
                {
                    campaign_choice_definition_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    selection_mode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignChoiceDefinitions", x => x.campaign_choice_definition_id);
                    table.ForeignKey(
                        name: "FK_CampaignChoiceDefinitions_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignChoiceDefinitions_StoryBeats_story_beat_id",
                        column: x => x.story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignChoiceDefinitions_StoryBlocks_story_block_id",
                        column: x => x.story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignEventDefinitions",
                columns: table => new
                {
                    campaign_event_definition_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    event_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    is_repeatable = table.Column<bool>(type: "bit", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignEventDefinitions", x => x.campaign_event_definition_id);
                    table.ForeignKey(
                        name: "FK_CampaignEventDefinitions_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConditionGroups",
                columns: table => new
                {
                    condition_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_condition_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    @operator = table.Column<string>(name: "operator", type: "nvarchar(64)", maxLength: 64, nullable: false),
                    negate = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionGroups", x => x.condition_group_id);
                    table.ForeignKey(
                        name: "FK_ConditionGroups_ConditionGroups_parent_condition_group_id",
                        column: x => x.parent_condition_group_id,
                        principalTable: "ConditionGroups",
                        principalColumn: "condition_group_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignChoiceOptions",
                columns: table => new
                {
                    campaign_choice_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_choice_definition_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignChoiceOptions", x => x.campaign_choice_option_id);
                    table.ForeignKey(
                        name: "FK_CampaignChoiceOptions_CampaignChoiceDefinitions_campaign_choice_definition_id",
                        column: x => x.campaign_choice_definition_id,
                        principalTable: "CampaignChoiceDefinitions",
                        principalColumn: "campaign_choice_definition_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignChoiceOptions_StoryBeats_story_beat_id",
                        column: x => x.story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignEventOptions",
                columns: table => new
                {
                    campaign_event_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_event_definition_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignEventOptions", x => x.campaign_event_option_id);
                    table.ForeignKey(
                        name: "FK_CampaignEventOptions_CampaignEventDefinitions_campaign_event_definition_id",
                        column: x => x.campaign_event_definition_id,
                        principalTable: "CampaignEventDefinitions",
                        principalColumn: "campaign_event_definition_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConditionalRules",
                columns: table => new
                {
                    conditional_rule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    target_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    target_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    root_condition_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    effect_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionalRules", x => x.conditional_rule_id);
                    table.ForeignKey(
                        name: "FK_ConditionalRules_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConditionalRules_ConditionGroups_root_condition_group_id",
                        column: x => x.root_condition_group_id,
                        principalTable: "ConditionGroups",
                        principalColumn: "condition_group_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignChoiceSelections",
                columns: table => new
                {
                    campaign_choice_selection_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_session_id = table.Column<int>(type: "int", nullable: false),
                    campaign_choice_definition_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_choice_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    selected_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignChoiceSelections", x => x.campaign_choice_selection_id);
                    table.ForeignKey(
                        name: "FK_CampaignChoiceSelections_CampaignChoiceDefinitions_campaign_choice_definition_id",
                        column: x => x.campaign_choice_definition_id,
                        principalTable: "CampaignChoiceDefinitions",
                        principalColumn: "campaign_choice_definition_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignChoiceSelections_CampaignChoiceOptions_campaign_choice_option_id",
                        column: x => x.campaign_choice_option_id,
                        principalTable: "CampaignChoiceOptions",
                        principalColumn: "campaign_choice_option_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignChoiceSelections_CampaignSessions_campaign_session_id",
                        column: x => x.campaign_session_id,
                        principalTable: "CampaignSessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignEventStates",
                columns: table => new
                {
                    campaign_event_state_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_session_id = table.Column<int>(type: "int", nullable: false),
                    campaign_event_definition_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    boolean_value = table.Column<bool>(type: "bit", nullable: true),
                    selected_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    text_value = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    numeric_value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    source_story_block_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    source_story_beat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    resolved_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignEventStates", x => x.campaign_event_state_id);
                    table.ForeignKey(
                        name: "FK_CampaignEventStates_CampaignEventDefinitions_campaign_event_definition_id",
                        column: x => x.campaign_event_definition_id,
                        principalTable: "CampaignEventDefinitions",
                        principalColumn: "campaign_event_definition_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignEventStates_CampaignEventOptions_selected_option_id",
                        column: x => x.selected_option_id,
                        principalTable: "CampaignEventOptions",
                        principalColumn: "campaign_event_option_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignEventStates_CampaignSessions_campaign_session_id",
                        column: x => x.campaign_session_id,
                        principalTable: "CampaignSessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignEventStates_StoryBeats_source_story_beat_id",
                        column: x => x.source_story_beat_id,
                        principalTable: "StoryBeats",
                        principalColumn: "story_beat_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignEventStates_StoryBlocks_source_story_block_id",
                        column: x => x.source_story_block_id,
                        principalTable: "StoryBlocks",
                        principalColumn: "story_block_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConditionClauses",
                columns: table => new
                {
                    condition_clause_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    condition_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_event_definition_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    comparison_operator = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    boolean_value = table.Column<bool>(type: "bit", nullable: true),
                    expected_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    text_value = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    numeric_value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionClauses", x => x.condition_clause_id);
                    table.ForeignKey(
                        name: "FK_ConditionClauses_CampaignEventDefinitions_campaign_event_definition_id",
                        column: x => x.campaign_event_definition_id,
                        principalTable: "CampaignEventDefinitions",
                        principalColumn: "campaign_event_definition_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConditionClauses_CampaignEventOptions_expected_option_id",
                        column: x => x.expected_option_id,
                        principalTable: "CampaignEventOptions",
                        principalColumn: "campaign_event_option_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConditionClauses_ConditionGroups_condition_group_id",
                        column: x => x.condition_group_id,
                        principalTable: "ConditionGroups",
                        principalColumn: "condition_group_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoryOutcomeEffects",
                columns: table => new
                {
                    story_outcome_effect_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    source_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    source_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    campaign_event_definition_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    operation_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    boolean_value = table.Column<bool>(type: "bit", nullable: true),
                    selected_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    text_value = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    numeric_value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryOutcomeEffects", x => x.story_outcome_effect_id);
                    table.ForeignKey(
                        name: "FK_StoryOutcomeEffects_CampaignEventDefinitions_campaign_event_definition_id",
                        column: x => x.campaign_event_definition_id,
                        principalTable: "CampaignEventDefinitions",
                        principalColumn: "campaign_event_definition_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoryOutcomeEffects_CampaignEventOptions_selected_option_id",
                        column: x => x.selected_option_id,
                        principalTable: "CampaignEventOptions",
                        principalColumn: "campaign_event_option_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoryOutcomeEffects_Campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "Campaigns",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChoiceDefinitions_campaign_id",
                table: "CampaignChoiceDefinitions",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChoiceDefinitions_story_beat_id",
                table: "CampaignChoiceDefinitions",
                column: "story_beat_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChoiceDefinitions_story_block_id",
                table: "CampaignChoiceDefinitions",
                column: "story_block_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChoiceOptions_campaign_choice_definition_id_key",
                table: "CampaignChoiceOptions",
                columns: new[] { "campaign_choice_definition_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChoiceOptions_story_beat_id",
                table: "CampaignChoiceOptions",
                column: "story_beat_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChoiceSelections_campaign_choice_definition_id",
                table: "CampaignChoiceSelections",
                column: "campaign_choice_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChoiceSelections_campaign_choice_option_id",
                table: "CampaignChoiceSelections",
                column: "campaign_choice_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChoiceSelections_campaign_session_id_campaign_choice_definition_id_campaign_choice_option_id",
                table: "CampaignChoiceSelections",
                columns: new[] { "campaign_session_id", "campaign_choice_definition_id", "campaign_choice_option_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEventDefinitions_campaign_id_key",
                table: "CampaignEventDefinitions",
                columns: new[] { "campaign_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEventOptions_campaign_event_definition_id_key",
                table: "CampaignEventOptions",
                columns: new[] { "campaign_event_definition_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEventStates_campaign_event_definition_id",
                table: "CampaignEventStates",
                column: "campaign_event_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEventStates_campaign_session_id_campaign_event_definition_id",
                table: "CampaignEventStates",
                columns: new[] { "campaign_session_id", "campaign_event_definition_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEventStates_selected_option_id",
                table: "CampaignEventStates",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEventStates_source_story_beat_id",
                table: "CampaignEventStates",
                column: "source_story_beat_id");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEventStates_source_story_block_id",
                table: "CampaignEventStates",
                column: "source_story_block_id");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalRules_campaign_id",
                table: "ConditionalRules",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalRules_root_condition_group_id",
                table: "ConditionalRules",
                column: "root_condition_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalRules_target_type_target_id",
                table: "ConditionalRules",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "IX_ConditionClauses_campaign_event_definition_id",
                table: "ConditionClauses",
                column: "campaign_event_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionClauses_condition_group_id",
                table: "ConditionClauses",
                column: "condition_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionClauses_expected_option_id",
                table: "ConditionClauses",
                column: "expected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionGroups_parent_condition_group_id",
                table: "ConditionGroups",
                column: "parent_condition_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryOutcomeEffects_campaign_event_definition_id",
                table: "StoryOutcomeEffects",
                column: "campaign_event_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryOutcomeEffects_campaign_id",
                table: "StoryOutcomeEffects",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryOutcomeEffects_selected_option_id",
                table: "StoryOutcomeEffects",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_StoryOutcomeEffects_source_type_source_id",
                table: "StoryOutcomeEffects",
                columns: new[] { "source_type", "source_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignChoiceSelections");

            migrationBuilder.DropTable(
                name: "CampaignEventStates");

            migrationBuilder.DropTable(
                name: "ConditionalRules");

            migrationBuilder.DropTable(
                name: "ConditionClauses");

            migrationBuilder.DropTable(
                name: "StoryOutcomeEffects");

            migrationBuilder.DropTable(
                name: "CampaignChoiceOptions");

            migrationBuilder.DropTable(
                name: "ConditionGroups");

            migrationBuilder.DropTable(
                name: "CampaignEventOptions");

            migrationBuilder.DropTable(
                name: "CampaignChoiceDefinitions");

            migrationBuilder.DropTable(
                name: "CampaignEventDefinitions");
        }
    }
}
