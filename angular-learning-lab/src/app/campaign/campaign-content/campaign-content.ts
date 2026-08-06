import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LucideArrowDown, LucideArrowUp, LucideCheck, LucideEye, LucideGitBranch, LucideGripVertical, LucideLink, LucideLock, LucideMusic, LucidePencil, LucidePlus, LucideTrash2, LucideX, LucideZap } from '@lucide/angular';
import { catchError, finalize, forkJoin, map, Observable, of, switchMap, timeout } from 'rxjs';

import {
  Ability,
  AbilityValue,
  ApiError,
  CampaignApiService,
  CampaignEventModel,
  CampaignEventsApiService,
  CampaignMilestoneImportance,
  CampaignMilestoneModel,
  CampaignMilestoneRequest,
  CampaignNpcModel,
  CampaignQuestDeleteBlockerModel,
  CampaignQuestModel,
  CampaignQuestTaskModel,
  CampaignQuestType,
  CampaignRuleGroupRequest,
  CampaignRulesApiService,
  ConditionalRuleEffectType,
  ConditionalRuleModel,
  ConditionalTargetType,
  OutcomeEffectModel,
  OutcomeEffectOperation,
  OutcomeEffectRequest,
  OutcomeEffectsApiService,
  OutcomeSourceType,
  RuleComparisonOperator,
  RuleConditionRequest,
  RuleGroupOperator,
  Skill,
  SkillValue,
  CreateStoryBlockRequest,
  CreateCombatStoryBeatRequest,
  CreateDecisionStoryBeatRequest,
  CreateInformationStoryBeatRequest,
  CreateMilestoneStoryBeatRequest,
  CreateNarrativeStoryBeatRequest,
  CreateRoleplayingStoryBeatRequest,
  CreateTransitionStoryBeatRequest,
  LibraryApiService,
  LibraryFileModel,
  UpdateCampaignQuestRequest,
  MonsterApiService,
  MonsterModel,
  getCampaignMilestoneImportanceLabel,
  getCampaignMilestoneImportanceSlug,
  StoryBeatOptionalInformationModel,
  StoryBeatOptionalInformationPlacement,
  StoryBeatOptionalInformationRequest,
  StoryBeatIndexPathRuleModel,
  StoryBeatIndexPathRuleRelationType,
  StoryBeatIndexPathRuleRelationTypeValue,
  StoryBeatModel,
  StoryBeatQuestTaskModel,
  StoryBeatRoleplayingCheckType,
  StoryBeatRoleplayingInformationModel,
  StoryBeatType,
  StoryBlockMusicFileModel,
  StoryBlockMusicFileRequest,
  StoryBlockModel,
  toCampaignMilestoneImportance,
  UpdateCombatStoryBeatRequest,
  UpdateDecisionStoryBeatRequest,
  UpdateStoryBlockTitleRequest,
  UpdateRoleplayingStoryBeatRequest,
  UpdateTransitionStoryBeatRequest,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';
import { CombatNpcsPage } from './combat-npcs-page/combat-npcs-page';
import { CampaignEventsPage } from './campaign-events-page/campaign-events-page';
import { CampaignStoresPage } from './campaign-stores-page/campaign-stores-page';
import { RuleBuilder } from '../story-authoring/rule-builder/rule-builder';
import { OutcomeEffectEditor } from '../story-authoring/outcome-effect-editor/outcome-effect-editor';

type CampaignContentTab =
  'main-story' |
  'campaign-events' |
  'campaign-milestones' |
  'quests' |
  'roleplaying-npcs' |
  'combat-npcs' |
  'campaign-stores';
type QuestCarouselItem = CampaignQuestModel | 'add-quest';
type QuestFormStep = 'details' | 'tasks';
type StoryBlockDropPosition = 'before' | 'after';

interface SkillOption {
  skill: Skill;
  label: string;
  className: string;
}

interface AbilityOption {
  ability: Ability;
  label: string;
}

interface RoleplayingNpcOption {
  key: string;
  name: string;
}

interface CampaignRoleplayingNpcTableRow {
  key: string;
  campaignNpcId: string;
  tag: string;
  name: string;
  displayName: string;
  description: string;
  createdAt: string;
  updatedAt: string;
}

interface StoryBlockViewModel extends StoryBlockModel {
  displayIndex: number;
  beats: StoryBeatViewModel[];
}

interface StoryBeatViewModel extends StoryBeatModel {
  displayIndex: number;
  milestone: CampaignMilestoneModel | null;
}

interface StoryBeatRowViewModel {
  key: string;
  storyBlockId: string;
  orderIndex: number;
  beats: StoryBeatViewModel[];
  activeIndex: number;
  activeBeat: StoryBeatViewModel | null;
}

interface SiblingStoryBeatDraftSource {
  storyBlockId: string;
  orderIndex: number;
  displayIndex: number;
}

interface CreateStoryBeatOrderDraft {
  orderIndex?: number | null;
  secondaryOrderIndex?: number | null;
}

interface StoryBeatOptionalInformationDraft {
  draftId: number;
  skill: Skill;
  difficultyClass: number;
  information: string;
}

interface StoryBeatRoleplayingInformationDraft {
  draftId: number;
  npcTag: string;
  npcName: string;
  checkType: StoryBeatRoleplayingCheckType;
  skill: Skill;
  ability: Ability;
  difficultyClass: number;
  information: string;
}

interface StoryBeatNarrativeParagraphDraft {
  draftId: number;
  text: string;
}

interface StoryBeatDecisionChoiceDraft {
  draftId: number;
  id: string | null;
  title: string;
  description: string;
}

interface StoryBeatCombatEnemyNpcDraft {
  draftId: number;
  monsterId: number | null;
  amount: number;
}

interface StoryBeatCombatRewardDraft {
  draftId: number;
  text: string;
}

interface StoryBeatNarrativePart {
  text: string;
  className: string | null;
  compactText?: string;
  detailText?: string;
  tokenKey?: string;
}

interface StoryBeatOutcomeEffectSource {
  sourceType: OutcomeSourceType;
  sourceId: string;
}

interface StoryBeatOutcomeEffectSummarySource extends StoryBeatOutcomeEffectSource {
  storyBeatId: string;
  key: string;
  label: string;
}

interface StoryBeatOutcomeEffectSummary extends StoryBeatOutcomeEffectSource {
  key: string;
  label: string;
  effectCount: number;
  eventLabels: string[];
}

interface StoryBeatRuleSummary {
  rule: ConditionalRuleModel;
  label: string;
  description: string;
}

interface StoryBeatOutcomeEffectTarget extends StoryBeatOutcomeEffectSource {
  key: string;
  title: string;
  description: string;
  category: string;
}

interface QuestTaskDraft {
  draftId: number;
  title: string;
  description: string;
  dateCompleted: string | null;
}

interface StoryBeatQuestTaskSearchResult {
  quest: CampaignQuestModel;
  tasks: CampaignQuestTaskModel[];
}

interface StoryBeatIndexPathRuleDialogState {
  storyBlock: StoryBlockViewModel;
  storyBeatRow: StoryBeatRowViewModel;
}

interface StoryBlockMusicDraft {
  draftId: number;
  musicFileId: number;
  storyBeatId: string | null;
  orderIndex: number;
  loop: boolean;
  continueAcrossStoryBlocks: boolean;
}

@Component({
  selector: 'app-campaign-content',
  imports: [
    CampaignEventsPage,
    CampaignStoresPage,
    CombatNpcsPage,
    OutcomeEffectEditor,
    RuleBuilder,
    LucideArrowDown,
    LucideArrowUp,
    LucideCheck,
    LucideEye,
    LucideGitBranch,
    LucideGripVertical,
    LucideLink,
    LucideLock,
    LucideMusic,
    LucidePencil,
    LucidePlus,
    LucideTrash2,
    LucideX,
    LucideZap,
  ],
  templateUrl: './campaign-content.html',
  styleUrl: './campaign-content.css',
})
export class CampaignContent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly campaignEventsApiService = inject(CampaignEventsApiService);
  private readonly campaignRulesApiService = inject(CampaignRulesApiService);
  private readonly libraryApiService = inject(LibraryApiService);
  private readonly outcomeEffectsApiService = inject(OutcomeEffectsApiService);
  private readonly monsterApiService = inject(MonsterApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly selectedTab = signal<CampaignContentTab>('main-story');
  protected readonly storyBlocks = signal<StoryBlockViewModel[]>([]);
  protected readonly selectedStoryBlockId = signal<string | null>(null);
  protected readonly milestones = signal<CampaignMilestoneModel[]>([]);
  protected readonly quests = signal<CampaignQuestModel[]>([]);
  protected readonly roleplayingNpcs = signal<CampaignNpcModel[]>([]);
  protected readonly roleplayingNpcsLoadError = signal('');
  protected readonly editingRoleplayingNpcKey = signal<string | null>(null);
  protected readonly roleplayingNpcNameDrafts = signal<Record<string, string>>({});
  protected readonly savingRoleplayingNpcTag = signal<string | null>(null);
  protected readonly isLoadingStoryContent = signal(false);
  protected readonly storyBlockMusicDialogBlock = signal<StoryBlockViewModel | null>(null);
  protected readonly storyBlockMusicDrafts = signal<StoryBlockMusicDraft[]>([]);
  protected readonly libraryMusicFiles = signal<LibraryFileModel[]>([]);
  protected readonly selectedMusicFileIdDraft = signal<number | null>(null);
  protected readonly selectedMusicStoryBeatIdDraft = signal<string | null>(null);
  protected readonly musicLoopDraft = signal(true);
  protected readonly musicContinueDraft = signal(false);
  protected readonly isLoadingStoryBlockMusic = signal(false);
  protected readonly isSavingStoryBlockMusic = signal(false);
  protected readonly isReorderingStoryBlocks = signal(false);
  protected readonly isReorderingStoryBeats = signal(false);
  protected readonly draggedStoryBlockId = signal<string | null>(null);
  protected readonly storyBlockDropTargetId = signal<string | null>(null);
  protected readonly storyBlockDropPosition = signal<StoryBlockDropPosition>('after');
  protected readonly storyBeatAlternativeIndexes = signal<Record<string, number>>({});
  protected readonly expandedStoryBeatSkillTokenKeys = signal<Record<string, boolean>>({});
  protected readonly isLoadingMilestones = signal(false);
  protected readonly isLoadingQuests = signal(false);
  protected readonly isLoadingRoleplayingNpcs = signal(false);
  protected readonly isCreatingStoryBlock = signal(false);
  protected readonly isUpdatingStoryBlockTitle = signal(false);
  protected readonly creatingStoryBeatBlockId = signal<string | null>(null);
  protected readonly updatingStoryBeatId = signal<string | null>(null);
  protected readonly isCreateStoryBlockDialogOpen = signal(false);
  protected readonly isCreateStoryBeatDialogOpen = signal(false);
  protected readonly isCreateMilestoneDialogOpen = signal(false);
  protected readonly isCreateQuestDialogOpen = signal(false);
  protected readonly isCreatingMilestone = signal(false);
  protected readonly isCreatingQuest = signal(false);
  protected readonly isDeletingQuest = signal(false);
  protected readonly isDeletingMilestone = signal(false);
  protected readonly deletingStoryBlockId = signal<string | null>(null);
  protected readonly deletingStoryBeatId = signal<string | null>(null);
  protected readonly editingMilestone = signal<CampaignMilestoneModel | null>(null);
  protected readonly editingQuest = signal<CampaignQuestModel | null>(null);
  protected readonly editingStoryBlock = signal<StoryBlockViewModel | null>(null);
  protected readonly editingStoryBeat = signal<StoryBeatViewModel | null>(null);
  protected readonly storyBeatDialogBlock = signal<StoryBlockViewModel | null>(null);
  protected readonly siblingStoryBeatDraftSource = signal<SiblingStoryBeatDraftSource | null>(null);
  protected readonly storyBeatRulesDialogBeat = signal<StoryBeatViewModel | null>(null);
  protected readonly storyBeatIndexPathRuleDialog = signal<StoryBeatIndexPathRuleDialogState | null>(null);
  protected readonly storyBeatQuestTaskDialogBeat = signal<StoryBeatViewModel | null>(null);
  protected readonly storyBeatRule = signal<ConditionalRuleModel | null>(null);
  protected readonly storyBeatRuleDraft = signal<CampaignRuleGroupRequest | null>(null);
  protected readonly storyBeatRuleEffectType = signal<ConditionalRuleEffectType>(
    ConditionalRuleEffectType.RequiredForAvailability,
  );
  protected readonly storyBeatRuleEffectTypes = [
    { value: ConditionalRuleEffectType.RequiredForAvailability, label: 'Required For Availability' },
    { value: ConditionalRuleEffectType.RequiredForVisibility, label: 'Required For Visibility' },
    { value: ConditionalRuleEffectType.ExclusivePath, label: 'Exclusive Path' },
    { value: ConditionalRuleEffectType.OptionalInformation, label: 'Optional Information' },
  ];
  protected readonly storyBeatRuleEventOptions = signal<CampaignEventModel[]>([]);
  protected readonly storyBeatRuleSummaryEventOptions = signal<CampaignEventModel[]>([]);
  protected readonly storyBeatRuleSummaries = signal<Record<string, ConditionalRuleModel[]>>({});
  protected readonly storyBeatIndexPathRelationTypeDraft = signal<StoryBeatIndexPathRuleRelationType>(
    StoryBeatIndexPathRuleRelationType.ExactlyOne,
  );
  protected readonly storyBeatIndexPathRequiredDraft = signal(false);
  protected readonly isSavingStoryBeatIndexPathRule = signal(false);
  protected readonly isDeletingStoryBeatIndexPathRule = signal(false);
  protected readonly storyBeatIndexPathRelationTypes = [
    {
      value: StoryBeatIndexPathRuleRelationType.ExactlyOne,
      label: 'Exactly One',
      description: 'Only one beat in this index path can be selected.',
    },
    {
      value: StoryBeatIndexPathRuleRelationType.Or,
      label: 'OR',
      description: 'At least one beat in this index path can satisfy the path.',
    },
    {
      value: StoryBeatIndexPathRuleRelationType.And,
      label: 'AND',
      description: 'All beats in this index path are expected to happen.',
    },
  ];
  protected readonly storyBeatQuestTasks = signal<StoryBeatQuestTaskModel[]>([]);
  protected readonly campaignStoryBeatQuestTasks = signal<StoryBeatQuestTaskModel[]>([]);
  protected readonly storyBeatQuestTaskSearchDraft = signal('');
  protected readonly isLoadingStoryBeatRules = signal(false);
  protected readonly isSavingStoryBeatRule = signal(false);
  protected readonly isDeletingStoryBeatRule = signal(false);
  protected readonly isLoadingStoryBeatQuestTasks = signal(false);
  protected readonly linkingQuestTaskId = signal<string | null>(null);
  protected readonly unlinkingQuestTaskId = signal<string | null>(null);
  protected readonly storyBeatEventEffectsDialogBeat = signal<StoryBeatViewModel | null>(null);
  protected readonly storyBeatEventEffects = signal<OutcomeEffectModel[]>([]);
  protected readonly storyBeatEventEffectDrafts = signal<OutcomeEffectRequest[]>([]);
  protected readonly storyBeatEventEffectOptions = signal<CampaignEventModel[]>([]);
  protected readonly storyBeatEventEffectSummaries = signal<Record<string, StoryBeatOutcomeEffectSummary[]>>({});
  protected readonly selectedRoleplayingEventSource = signal<StoryBeatOutcomeEffectSource | null>(null);
  protected readonly isLoadingStoryBeatEventEffects = signal(false);
  protected readonly isSavingStoryBeatEventEffects = signal(false);
  protected readonly isDeletingStoryBeatEventEffects = signal(false);
  protected readonly deleteConfirmationStoryBlock = signal<StoryBlockViewModel | null>(null);
  protected readonly deleteConfirmationStoryBeat = signal<{
    storyBlock: StoryBlockViewModel;
    storyBeat: StoryBeatViewModel;
  } | null>(null);
  protected readonly deleteConfirmationMilestone = signal<CampaignMilestoneModel | null>(null);
  protected readonly questCarouselIndex = signal(0);
  protected readonly storyBlockTitleDraft = signal('');
  protected readonly storyBeatTitleDraft = signal('');
  protected readonly storyBeatTypeDraft = signal<StoryBeatType>(StoryBeatType.Information);
  protected readonly storyBeatNarrativeDraft = signal('');
  protected readonly storyBeatRoleplayingDraft = signal('');
  protected readonly storyBeatNarrativeParagraphDrafts = signal<StoryBeatNarrativeParagraphDraft[]>([]);
  protected readonly storyBeatOptionalInformationDrafts = signal<StoryBeatOptionalInformationDraft[]>([]);
  protected readonly storyBeatRoleplayingInformationDrafts = signal<StoryBeatRoleplayingInformationDraft[]>([]);
  protected readonly storyBeatDecisionDescriptionDraft = signal('');
  protected readonly storyBeatDecisionChoiceDrafts = signal<StoryBeatDecisionChoiceDraft[]>([]);
  protected readonly activeStoryBeatDecisionChoiceDraftId = signal<number | null>(null);
  protected readonly storyBeatCombatDescriptionDraft = signal('');
  protected readonly storyBeatTransitionDescriptionDraft = signal('');
  protected readonly storyBeatCombatRewardDrafts = signal<StoryBeatCombatRewardDraft[]>([]);
  protected readonly storyBeatCombatEnemyNpcDrafts = signal<StoryBeatCombatEnemyNpcDraft[]>([]);
  protected readonly combatNpcOptions = signal<MonsterModel[]>([]);
  protected readonly isLoadingCombatNpcOptions = signal(false);
  protected readonly storyBeatMilestoneDraft = signal<number | null>(null);
  protected readonly milestoneTitleDraft = signal('');
  protected readonly milestoneDescriptionDraft = signal('');
  protected readonly questTypeDraft = signal<CampaignQuestType>(CampaignQuestType.MainQuest);
  protected readonly questTitleDraft = signal('');
  protected readonly questDescriptionDraft = signal('');
  protected readonly questGivenByDraft = signal('');
  protected readonly questRewardDraft = signal('');
  protected readonly questTaskDrafts = signal<QuestTaskDraft[]>([]);
  protected readonly questFormStep = signal<QuestFormStep>('details');
  protected readonly milestoneImportanceDraft = signal<CampaignMilestoneImportance>(
    CampaignMilestoneImportance.Low,
  );
  protected readonly questCarouselItems = computed<QuestCarouselItem[]>(() => [
    ...this.quests(),
    'add-quest',
  ]);
  protected readonly activeQuestCarouselItem = computed(() => (
    this.questCarouselItems()[this.questCarouselIndex()] ?? 'add-quest'
  ));
  protected readonly roleplayingNpcRows = computed(() => (
    this.toRoleplayingNpcRows(this.roleplayingNpcs())
  ));
  protected readonly canSaveStoryBeatRule = computed(() => (
    this.storyBeatRulesDialogBeat() !== null &&
    this.storyBeatRuleDraft() !== null &&
    !this.isLoadingStoryBeatRules() &&
    !this.isSavingStoryBeatRule()
  ));
  protected readonly canSaveStoryBeatIndexPathRule = computed(() => (
    this.storyBeatIndexPathRuleDialog() !== null &&
    this.storyBeatIndexPathRuleDialog()!.storyBeatRow.beats.length > 1 &&
    !this.isSavingStoryBeatIndexPathRule() &&
    !this.isDeletingStoryBeatIndexPathRule()
  ));
  protected readonly canSaveStoryBeatEventEffects = computed(() => (
    this.storyBeatEventEffectsDialogBeat() !== null &&
    this.hasSelectedStoryBeatEventEffectSource(this.storyBeatEventEffectsDialogBeat()!) &&
    !this.isLoadingStoryBeatEventEffects() &&
    !this.isSavingStoryBeatEventEffects() &&
    this.storyBeatEventEffectDrafts().every((effect) => (
      typeof (effect.eventDefinitionId ?? effect.eventId) === 'string' &&
      (effect.eventDefinitionId ?? effect.eventId ?? '').trim().length > 0
    ))
  ));
  protected readonly storyBeatQuestTaskSearchResults = computed<StoryBeatQuestTaskSearchResult[]>(() => {
    const query = this.normalizeText(this.storyBeatQuestTaskSearchDraft()).toLowerCase();
    const linkedTaskIds = new Set(this.campaignStoryBeatQuestTasks().map((task) => task.questTaskId));

    return this.quests()
      .map((quest) => {
        const questMatches = this.storyBeatQuestSearchText(quest).includes(query);
        const tasks = quest.tasks.filter((task) => (
          !linkedTaskIds.has(task.questTaskId) &&
          (query.length === 0 || questMatches || this.storyBeatQuestTaskSearchText(task).includes(query))
        ));

        return { quest, tasks };
      })
      .filter((result) => result.tasks.length > 0);
  });
  protected readonly selectedStoryBlock = computed(() => {
    const selectedStoryBlockId = this.selectedStoryBlockId();

    if (!selectedStoryBlockId) {
      return null;
    }

    return this.storyBlocks().find((storyBlock) => (
      storyBlock.storyBlockId === selectedStoryBlockId
    )) ?? null;
  });
  protected readonly canAddStoryBlockMusic = computed(() => {
    const musicFileId = this.selectedMusicFileIdDraft();

    return musicFileId !== null &&
      this.storyBlockMusicDrafts().every((draft) => (
        draft.musicFileId !== musicFileId ||
        draft.storyBeatId !== this.selectedMusicStoryBeatIdDraft()
      ));
  });
  protected readonly canSaveStoryBlockMusic = computed(() => (
    !this.isLoadingStoryBlockMusic() && !this.isSavingStoryBlockMusic()
  ));
  protected readonly storyBeatSkillSuggestions = computed(() => {
    const query = this.getActiveSlashSkillQuery(this.storyBeatNarrativeDraft());

    if (query === null) {
      return [];
    }

    const normalizedQuery = query.toLowerCase();

    return this.skillOptions.filter((option) => (
      option.label.toLowerCase().startsWith(normalizedQuery)
    ));
  });
  protected readonly storyBeatNarrativePreviewParts = computed(() => (
    this.toNarrativePreviewParts(this.storyBeatNarrativeDraft())
  ));
  protected readonly storyBeatRoleplayingPreviewParts = computed(() => (
    this.toRoleplayingPreviewParts(this.storyBeatRoleplayingDraft())
  ));
  protected readonly roleplayingNpcOptions = computed(() => (
    this.toRoleplayingNpcOptions(this.storyBeatRoleplayingDraft())
  ));
  protected readonly activeStoryBeatDecisionChoiceDraft = computed(() => {
    const choices = this.storyBeatDecisionChoiceDrafts();
    const activeChoiceId = this.activeStoryBeatDecisionChoiceDraftId();

    return choices.find((choice) => choice.draftId === activeChoiceId) ??
      choices[0] ??
      null;
  });
  protected readonly availableStoryBeatMilestoneOptions = computed(() => {
    const editingStoryBeat = this.editingStoryBeat();
    const linkedMilestoneIds = new Set(
      this.storyBlocks().flatMap((storyBlock) => (
        storyBlock.beats
          .filter((beat) => beat.storyBeatId !== editingStoryBeat?.storyBeatId)
          .map((beat) => beat.milestone?.id)
          .filter((milestoneId): milestoneId is number => typeof milestoneId === 'number')
      )),
    );

    return this.milestones().filter((milestone) => !linkedMilestoneIds.has(milestone.id));
  });
  protected readonly canMoveQuestCarousel = computed(() => this.questCarouselItems().length > 1);
  protected readonly canCreateStoryBlock = computed(() => {
    const title = this.normalizeText(this.storyBlockTitleDraft());

    return (
      title.length > 0 &&
      title.length <= 256 &&
      !this.isCreatingStoryBlock() &&
      !this.isUpdatingStoryBlockTitle()
    );
  });
  protected readonly canContinueQuestDetails = computed(() => (
    this.normalizeText(this.questTitleDraft()).length > 0 &&
    this.normalizeText(this.questDescriptionDraft()).length > 0 &&
    this.normalizeText(this.questGivenByDraft()).length > 0 &&
    this.normalizeText(this.questRewardDraft()).length > 0 &&
    !this.isCreatingQuest()
  ));
  protected readonly canCreateStoryBeat = computed(() => {
    const title = this.normalizeText(this.storyBeatTitleDraft());
    const storyBeatType = this.storyBeatTypeDraft();

    if (
      title.length === 0 ||
      title.length > 256 ||
      this.creatingStoryBeatBlockId() !== null ||
      this.updatingStoryBeatId() !== null
    ) {
      return false;
    }

    if (storyBeatType === StoryBeatType.Information) {
      return (
        this.normalizeText(this.storyBeatNarrativeDraft()).length > 0 &&
        this.storyBeatOptionalInformationDrafts().every((draft) => (
          this.isValidDifficultyClass(draft.difficultyClass) &&
          this.normalizeText(draft.information).length > 0
        ))
      );
    }

    if (storyBeatType === StoryBeatType.Narrative) {
      const paragraphs = this.storyBeatNarrativeParagraphDrafts()
        .map((draft) => this.normalizeText(draft.text));

      return (
        paragraphs.length >= 1 &&
        paragraphs.length <= 10 &&
        paragraphs.every((paragraph) => paragraph.length > 0)
      );
    }

    if (storyBeatType === StoryBeatType.Roleplaying) {
      const npcTags = new Set(this.roleplayingNpcOptions().map((npc) => npc.key));

      return (
        this.normalizeText(this.storyBeatRoleplayingDraft()).length > 0 &&
        npcTags.size > 0 &&
        this.storyBeatRoleplayingInformationDrafts().every((draft) => (
          npcTags.has(draft.npcTag) &&
          (
            draft.checkType === StoryBeatRoleplayingCheckType.None ||
            this.isValidDifficultyClass(draft.difficultyClass)
          ) &&
          this.normalizeText(draft.information).length > 0
        ))
      );
    }

    if (storyBeatType === StoryBeatType.Decision) {
      const choices = this.storyBeatDecisionChoiceDrafts();

      return (
        this.normalizeText(this.storyBeatDecisionDescriptionDraft()).length > 0 &&
        this.normalizeText(this.storyBeatDecisionDescriptionDraft()).length <= 2048 &&
        choices.length >= 1 &&
        choices.length <= 20 &&
        choices.every((choice) => (
          this.normalizeText(choice.title).length > 0 &&
          this.normalizeText(choice.title).length <= 256 &&
          this.normalizeText(choice.description).length > 0 &&
          this.normalizeText(choice.description).length <= 2048
        ))
      );
    }

    if (storyBeatType === StoryBeatType.Combat) {
      const monsterIds = this.storyBeatCombatEnemyNpcDrafts()
        .map((draft) => draft.monsterId)
        .filter((monsterId): monsterId is number => monsterId !== null);
      const uniqueMonsterIds = new Set(monsterIds);

      return (
        this.normalizeText(this.storyBeatCombatDescriptionDraft()).length > 0 &&
        monsterIds.length > 0 &&
        monsterIds.length === uniqueMonsterIds.size &&
        this.storyBeatCombatEnemyNpcDrafts().every((draft) => (
          draft.monsterId !== null && draft.amount >= 1
        ))
      );
    }

    if (storyBeatType === StoryBeatType.Transition) {
      return (
        this.normalizeText(this.storyBeatTransitionDescriptionDraft()).length > 0 &&
        this.normalizeText(this.storyBeatTransitionDescriptionDraft()).length <= 2048
      );
    }

    if (storyBeatType === StoryBeatType.Milestone) {
      return this.storyBeatMilestoneDraft() !== null;
    }

    return false;
  });
  protected readonly canCreateMilestone = computed(() => (
    this.normalizeText(this.milestoneTitleDraft()).length > 0 &&
    !this.isCreatingMilestone()
  ));
  protected readonly canCreateQuest = computed(() => (
    this.canContinueQuestDetails() &&
    !this.isCreatingQuest() &&
    !this.isDeletingQuest() &&
    this.questTaskDrafts()
      .some((task) => (
        this.normalizeText(task.title).length > 0 &&
        this.normalizeText(task.description).length > 0
      ))
  ));
  protected readonly questDialogActionText = computed(() => {
    if (this.isCreatingQuest()) {
      return this.editingQuest() ? 'Updating...' : 'Creating...';
    }

    return this.editingQuest() ? 'Update' : 'Create';
  });
  protected readonly milestoneDialogActionText = computed(() => {
    if (this.isCreatingMilestone()) {
      return this.editingMilestone() ? 'Updating...' : 'Creating...';
    }

    return this.editingMilestone() ? 'Update' : 'Create';
  });
  protected readonly importanceOptions = [
    {
      value: CampaignMilestoneImportance.Low,
      label: 'Low',
    },
    {
      value: CampaignMilestoneImportance.High,
      label: 'High',
    },
    {
      value: CampaignMilestoneImportance.Optional,
      label: 'Optional',
    },
  ];
  protected readonly questTypeOptions = [
    {
      value: CampaignQuestType.MainQuest,
      label: 'Main Quest',
    },
    {
      value: CampaignQuestType.SideQuest,
      label: 'Side Quest',
    },
    {
      value: CampaignQuestType.PersonalQuest,
      label: 'Personal Quest',
    },
    {
      value: CampaignQuestType.CollectibleHunt,
      label: 'Collectible Hunt',
    },
  ];
  protected readonly storyBeatTypeOptions = [
    {
      value: StoryBeatType.Information,
      label: 'Information',
      disabled: false,
    },
    {
      value: StoryBeatType.Narrative,
      label: 'Narrative',
      disabled: false,
    },
    {
      value: StoryBeatType.Roleplaying,
      label: 'Roleplaying',
      disabled: false,
    },
    {
      value: StoryBeatType.Decision,
      label: 'Decision',
      disabled: false,
    },
    {
      value: StoryBeatType.Combat,
      label: 'Combat',
      disabled: false,
    },
    {
      value: StoryBeatType.Transition,
      label: 'Transition',
      disabled: false,
    },
    {
      value: StoryBeatType.Milestone,
      label: 'Milestone',
      disabled: false,
    },
  ];
  protected readonly skillOptions: SkillOption[] = [
    { skill: Skill.Acrobatics, label: 'Acrobatics', className: 'skill-acrobatics' },
    { skill: Skill.AnimalHandling, label: 'Animal Handling', className: 'skill-animal-handling' },
    { skill: Skill.Arcana, label: 'Arcana', className: 'skill-arcana' },
    { skill: Skill.Athletics, label: 'Athletics', className: 'skill-athletics' },
    { skill: Skill.Deception, label: 'Deception', className: 'skill-deception' },
    { skill: Skill.History, label: 'History', className: 'skill-history' },
    { skill: Skill.Insight, label: 'Insight', className: 'skill-insight' },
    { skill: Skill.Intimidation, label: 'Intimidation', className: 'skill-intimidation' },
    { skill: Skill.Investigation, label: 'Investigation', className: 'skill-investigation' },
    { skill: Skill.Medicine, label: 'Medicine', className: 'skill-medicine' },
    { skill: Skill.Nature, label: 'Nature', className: 'skill-nature' },
    { skill: Skill.Perception, label: 'Perception', className: 'skill-perception' },
    { skill: Skill.Performance, label: 'Performance', className: 'skill-performance' },
    { skill: Skill.Persuasion, label: 'Persuasion', className: 'skill-persuasion' },
    { skill: Skill.Religion, label: 'Religion', className: 'skill-religion' },
    { skill: Skill.SleightOfHand, label: 'Sleight of Hand', className: 'skill-sleight-of-hand' },
    { skill: Skill.Stealth, label: 'Stealth', className: 'skill-stealth' },
    { skill: Skill.Survival, label: 'Survival', className: 'skill-survival' },
  ];
  protected readonly roleplayingCheckTypeOptions = [
    {
      value: StoryBeatRoleplayingCheckType.None,
      label: 'None',
    },
    {
      value: StoryBeatRoleplayingCheckType.Skill,
      label: 'Skill',
    },
    {
      value: StoryBeatRoleplayingCheckType.Ability,
      label: 'Ability',
    },
  ];
  protected readonly abilityOptions: AbilityOption[] = [
    { ability: Ability.STRENGTH, label: 'Strength' },
    { ability: Ability.DEXTERITY, label: 'Dexterity' },
    { ability: Ability.CONSTITUTION, label: 'Constitution' },
    { ability: Ability.INTELLIGENCE, label: 'Intelligence' },
    { ability: Ability.CHARISMA, label: 'Charisma' },
    { ability: Ability.WISDOM, label: 'Wisdom' },
  ];
  private nextQuestTaskDraftId = 1;
  private nextStoryBeatOptionalInformationDraftId = 1;
  private nextStoryBeatNarrativeParagraphDraftId = 1;
  private nextStoryBeatDecisionChoiceDraftId = 1;
  private nextStoryBeatCombatEnemyNpcDraftId = 1;
  private nextStoryBeatCombatRewardDraftId = 1;

  ngOnInit(): void {
    this.selectedTab.set(this.toCampaignContentTab(
      this.route.snapshot.queryParamMap.get('tab'),
    ));

    if (this.selectedTab() === 'main-story') {
      this.loadStoryContent();
    } else if (this.selectedTab() === 'campaign-events') {
      return;
    } else if (this.selectedTab() === 'campaign-milestones') {
      this.loadMilestones();
    } else if (this.selectedTab() === 'quests') {
      this.loadQuests();
    } else if (this.selectedTab() === 'roleplaying-npcs') {
      this.loadRoleplayingNpcs();
    }
  }

  refreshCampaignPage(): boolean {
    if (this.selectedTab() === 'main-story') {
      this.loadStoryContent();
      return true;
    }

    if (this.selectedTab() === 'campaign-events') {
      return false;
    }

    if (this.selectedTab() === 'campaign-milestones') {
      this.loadMilestones();
      return true;
    }

    if (this.selectedTab() === 'quests') {
      this.loadQuests();
      return true;
    }

    if (this.selectedTab() === 'roleplaying-npcs') {
      this.loadRoleplayingNpcs();
      return true;
    }

    return false;
  }

  isRefreshingCampaignPage(): boolean {
    return (
      this.isLoadingStoryContent() ||
      this.isLoadingMilestones() ||
      this.isLoadingQuests() ||
      this.isLoadingRoleplayingNpcs()
    );
  }

  protected selectTab(tab: CampaignContentTab): void {
    this.selectedTab.set(tab);

    if (tab === 'main-story') {
      this.loadStoryContent();
      return;
    }

    if (tab === 'campaign-events') {
      return;
    }

    if (tab === 'campaign-milestones') {
      this.loadMilestones();
      return;
    }

    if (tab === 'quests') {
      this.loadQuests();
      return;
    }

    if (tab === 'roleplaying-npcs') {
      this.loadRoleplayingNpcs();
    }
  }

  private toCampaignContentTab(value: string | null): CampaignContentTab {
    return value === 'campaign-milestones' ||
      value === 'campaign-events' ||
      value === 'quests' ||
      value === 'roleplaying-npcs' ||
      value === 'combat-npcs' ||
      value === 'campaign-stores'
      ? value
      : 'main-story';
  }

  protected openCreateStoryBlockDialog(): void {
    this.editingStoryBlock.set(null);
    this.storyBlockTitleDraft.set('');
    this.isCreateStoryBlockDialogOpen.set(true);
  }

  protected closeCreateStoryBlockDialog(): void {
    if (this.isCreatingStoryBlock() || this.isUpdatingStoryBlockTitle()) {
      return;
    }

    this.isCreateStoryBlockDialogOpen.set(false);
    this.editingStoryBlock.set(null);
  }

  protected setStoryBlockTitleDraft(event: Event): void {
    this.storyBlockTitleDraft.set((event.target as HTMLInputElement).value);
  }

  protected openEditStoryBlockDialog(storyBlock: StoryBlockViewModel): void {
    this.editingStoryBlock.set(storyBlock);
    this.storyBlockTitleDraft.set(storyBlock.title);
    this.isCreateStoryBlockDialogOpen.set(true);
  }

  protected confirmDeleteStoryBlock(storyBlock: StoryBlockViewModel): void {
    this.deleteConfirmationStoryBlock.set(storyBlock);
  }

  protected cancelDeleteStoryBlock(): void {
    if (this.deletingStoryBlockId()) {
      return;
    }

    this.deleteConfirmationStoryBlock.set(null);
  }

  protected saveStoryBlockTitle(): void {
    const editingStoryBlock = this.editingStoryBlock();

    if (editingStoryBlock) {
      this.updateStoryBlockTitle(editingStoryBlock);
      return;
    }

    this.createStoryBlock();
  }

  protected storyBlockDialogTitle(): string {
    return this.editingStoryBlock() ? 'Edit Story Block' : 'Create Story Block';
  }

  protected storyBlockDialogActionText(): string {
    if (this.isCreatingStoryBlock()) {
      return 'Creating...';
    }

    if (this.isUpdatingStoryBlockTitle()) {
      return 'Updating...';
    }

    return this.editingStoryBlock() ? 'Update' : 'Create';
  }

  protected selectStoryBlock(storyBlock: StoryBlockViewModel): void {
    this.selectedStoryBlockId.set(storyBlock.storyBlockId);
  }

  protected isSelectedStoryBlock(storyBlock: StoryBlockViewModel): boolean {
    return this.selectedStoryBlockId() === storyBlock.storyBlockId;
  }

  protected openStoryBlockMusicDialog(storyBlock: StoryBlockViewModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId) {
      return;
    }

    this.storyBlockMusicDialogBlock.set(storyBlock);
    this.storyBlockMusicDrafts.set([]);
    this.libraryMusicFiles.set([]);
    this.selectedMusicFileIdDraft.set(null);
    this.selectedMusicStoryBeatIdDraft.set(null);
    this.musicLoopDraft.set(true);
    this.musicContinueDraft.set(false);
    this.isLoadingStoryBlockMusic.set(true);

    forkJoin({
      music: this.campaignApiService.fetchStoryBlockMusicFiles(campaignId, storyBlock.storyBlockId),
      libraryFiles: this.libraryApiService.fetchAllFiles(),
    })
      .pipe(finalize(() => this.isLoadingStoryBlockMusic.set(false)))
      .subscribe({
        next: ({ music, libraryFiles }) => {
          const linkedMusic = music.data ?? [];
          const audioFiles = (libraryFiles.data ?? [])
            .filter((file) => file.contentType.toLowerCase().startsWith('audio/'))
            .sort((left, right) => this.libraryMusicFileLabel(left).localeCompare(
              this.libraryMusicFileLabel(right),
            ));

          this.libraryMusicFiles.set(audioFiles);
          this.storyBlockMusicDrafts.set(linkedMusic.map((link, index) => ({
            draftId: Date.now() + index,
            musicFileId: link.musicFileId,
            storyBeatId: link.storyBeatId,
            orderIndex: link.orderIndex,
            loop: link.loop,
            continueAcrossStoryBlocks: link.continueAcrossStoryBlocks,
          })));
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story block music could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected closeStoryBlockMusicDialog(): void {
    if (!this.isSavingStoryBlockMusic()) {
      this.storyBlockMusicDialogBlock.set(null);
    }
  }

  protected setSelectedMusicFileDraft(event: Event): void {
    const value = Number((event.target as HTMLSelectElement).value);

    this.selectedMusicFileIdDraft.set(Number.isFinite(value) && value > 0 ? value : null);
  }

  protected setSelectedMusicStoryBeatDraft(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;

    this.selectedMusicStoryBeatIdDraft.set(value.length > 0 ? value : null);
  }

  protected setMusicLoopDraft(event: Event): void {
    this.musicLoopDraft.set((event.target as HTMLInputElement).checked);
  }

  protected setMusicContinueDraft(event: Event): void {
    this.musicContinueDraft.set((event.target as HTMLInputElement).checked);
  }

  protected addStoryBlockMusicDraft(): void {
    const musicFileId = this.selectedMusicFileIdDraft();

    if (musicFileId === null || !this.canAddStoryBlockMusic()) {
      return;
    }

    this.storyBlockMusicDrafts.update((drafts) => [
      ...drafts,
      {
        draftId: Date.now(),
        musicFileId,
        storyBeatId: this.selectedMusicStoryBeatIdDraft(),
        orderIndex: drafts.length + 1,
        loop: this.musicLoopDraft(),
        continueAcrossStoryBlocks: this.musicContinueDraft(),
      },
    ]);
  }

  protected removeStoryBlockMusicDraft(draftId: number): void {
    this.storyBlockMusicDrafts.update((drafts) => (
      drafts
        .filter((draft) => draft.draftId !== draftId)
        .map((draft, index) => ({
          ...draft,
          orderIndex: index + 1,
        }))
    ));
  }

  protected setStoryBlockMusicDraftTarget(draftId: number, event: Event): void {
    const value = (event.target as HTMLSelectElement).value;

    this.updateStoryBlockMusicDraft(draftId, {
      storyBeatId: value.length > 0 ? value : null,
    });
  }

  protected setStoryBlockMusicDraftLoop(draftId: number, event: Event): void {
    this.updateStoryBlockMusicDraft(draftId, {
      loop: (event.target as HTMLInputElement).checked,
    });
  }

  protected setStoryBlockMusicDraftContinue(draftId: number, event: Event): void {
    this.updateStoryBlockMusicDraft(draftId, {
      continueAcrossStoryBlocks: (event.target as HTMLInputElement).checked,
    });
  }

  protected saveStoryBlockMusic(): void {
    const campaignId = this.getCampaignId();
    const storyBlock = this.storyBlockMusicDialogBlock();

    if (!campaignId || !storyBlock || !this.canSaveStoryBlockMusic()) {
      return;
    }

    const musicFiles: StoryBlockMusicFileRequest[] = this.storyBlockMusicDrafts()
      .map((draft, index) => ({
        musicFileId: draft.musicFileId,
        storyBeatId: draft.storyBeatId,
        orderIndex: index + 1,
        loop: draft.loop,
        continueAcrossStoryBlocks: draft.continueAcrossStoryBlocks,
      }));

    this.isSavingStoryBlockMusic.set(true);
    this.campaignApiService
      .updateStoryBlockMusicFiles(campaignId, storyBlock.storyBlockId, { musicFiles })
      .pipe(finalize(() => this.isSavingStoryBlockMusic.set(false)))
      .subscribe({
        next: (response) => {
          this.applyStoryBlockMusic(storyBlock.storyBlockId, response.data ?? []);
          this.storyBlockMusicDialogBlock.set(null);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story block music could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected startStoryBlockDrag(storyBlock: StoryBlockViewModel, event: DragEvent): void {
    if (this.isReorderingStoryBlocks()) {
      event.preventDefault();
      return;
    }

    this.draggedStoryBlockId.set(storyBlock.storyBlockId);
    this.storyBlockDropTargetId.set(storyBlock.storyBlockId);
    this.storyBlockDropPosition.set('after');
    event.dataTransfer?.setData('text/plain', storyBlock.storyBlockId);

    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  protected dragOverStoryBlock(storyBlock: StoryBlockViewModel, event: DragEvent): void {
    if (!this.draggedStoryBlockId() || this.isReorderingStoryBlocks()) {
      return;
    }

    event.preventDefault();

    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }

    const currentTarget = event.currentTarget as HTMLElement | null;
    const targetBounds = currentTarget?.getBoundingClientRect();
    const dropPosition = targetBounds && event.clientY < targetBounds.top + targetBounds.height / 2
      ? 'before'
      : 'after';

    this.storyBlockDropTargetId.set(storyBlock.storyBlockId);
    this.storyBlockDropPosition.set(dropPosition);
  }

  protected dropStoryBlock(storyBlock: StoryBlockViewModel, event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();

    const targetStoryBlockId = this.storyBlockDropTargetId() ?? storyBlock.storyBlockId;
    const dropPosition = this.storyBlockDropPosition();

    this.reorderDraggedStoryBlock(targetStoryBlockId, dropPosition);
  }

  protected endStoryBlockDrag(): void {
    this.clearStoryBlockDragState();
  }

  protected isDraggingStoryBlock(storyBlock: StoryBlockViewModel): boolean {
    return this.draggedStoryBlockId() === storyBlock.storyBlockId;
  }

  protected isStoryBlockDropBefore(storyBlock: StoryBlockViewModel): boolean {
    return this.storyBlockDropTargetId() === storyBlock.storyBlockId &&
      this.storyBlockDropPosition() === 'before' &&
      this.draggedStoryBlockId() !== null &&
      !this.isDraggingStoryBlock(storyBlock);
  }

  protected isStoryBlockDropAfter(storyBlock: StoryBlockViewModel): boolean {
    return this.storyBlockDropTargetId() === storyBlock.storyBlockId &&
      this.storyBlockDropPosition() === 'after' &&
      this.draggedStoryBlockId() !== null &&
      !this.isDraggingStoryBlock(storyBlock);
  }

  protected moveStoryBlockByKeyboard(
    storyBlock: StoryBlockViewModel,
    direction: 1 | -1,
    event: Event,
  ): void {
    event.preventDefault();

    if (this.isReorderingStoryBlocks()) {
      return;
    }

    const storyBlocks = this.storyBlocks();
    const currentIndex = storyBlocks.findIndex((block) => block.storyBlockId === storyBlock.storyBlockId);
    const targetIndex = currentIndex + direction;

    if (currentIndex < 0 || targetIndex < 0 || targetIndex >= storyBlocks.length) {
      return;
    }

    const nextStoryBlocks = [...storyBlocks];
    [nextStoryBlocks[currentIndex], nextStoryBlocks[targetIndex]] = [
      nextStoryBlocks[targetIndex]!,
      nextStoryBlocks[currentIndex]!,
    ];

    this.commitStoryBlockOrder(nextStoryBlocks, storyBlocks);
  }

  protected deleteStoryBlock(): void {
    const campaignId = this.getCampaignId();
    const storyBlock = this.deleteConfirmationStoryBlock();

    if (!campaignId || !storyBlock || this.deletingStoryBlockId()) {
      return;
    }

    this.deletingStoryBlockId.set(storyBlock.storyBlockId);

    this.campaignApiService
      .deleteStoryBlock(campaignId, storyBlock.storyBlockId)
      .pipe(finalize(() => this.deletingStoryBlockId.set(null)))
      .subscribe({
        next: () => {
          this.deleteConfirmationStoryBlock.set(null);
          this.storyBlocks.update((blocks) => {
            const nextBlocks = blocks
              .filter((block) => block.storyBlockId !== storyBlock.storyBlockId)
              .map((block, index) => ({
                ...block,
                displayIndex: index + 1,
              }));

            if (this.selectedStoryBlockId() === storyBlock.storyBlockId) {
              this.selectedStoryBlockId.set(nextBlocks[0]?.storyBlockId ?? null);
            }

            return nextBlocks;
          });
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story block could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private createStoryBlock(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || !this.canCreateStoryBlock()) {
      return;
    }

    const request: CreateStoryBlockRequest = {
      title: this.normalizeText(this.storyBlockTitleDraft()),
    };

    this.isCreatingStoryBlock.set(true);

    this.campaignApiService
      .createStoryBlock(campaignId, request)
      .pipe(finalize(() => this.isCreatingStoryBlock.set(false)))
      .subscribe({
        next: (response) => {
          this.isCreateStoryBlockDialogOpen.set(false);
          const storyBlock = response.data;

          if (!storyBlock) {
            this.loadStoryContent();
            return;
          }

          this.storyBlocks.update((blocks) => [
            ...blocks,
            {
              ...storyBlock,
              displayIndex: blocks.length + 1,
              beats: [],
            },
          ]);
          this.selectedStoryBlockId.set(storyBlock.storyBlockId);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story block could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private updateStoryBlockTitle(storyBlock: StoryBlockViewModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || !this.canCreateStoryBlock()) {
      return;
    }

    const request: UpdateStoryBlockTitleRequest = {
      title: this.normalizeText(this.storyBlockTitleDraft()),
    };

    this.isUpdatingStoryBlockTitle.set(true);

    this.campaignApiService
      .updateStoryBlockTitle(campaignId, storyBlock.storyBlockId, request)
      .pipe(finalize(() => this.isUpdatingStoryBlockTitle.set(false)))
      .subscribe({
        next: (response) => {
          this.isCreateStoryBlockDialogOpen.set(false);
          this.editingStoryBlock.set(null);

          const updatedStoryBlock = response.data;

          if (!updatedStoryBlock) {
            this.loadStoryContent();
            return;
          }

          this.storyBlocks.update((blocks) => blocks.map((block) => (
            block.storyBlockId === updatedStoryBlock.storyBlockId
              ? {
                ...block,
                ...updatedStoryBlock,
              }
              : block
          )));
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story block title could not be updated.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected openCreateStoryBeatDialog(storyBlock: StoryBlockViewModel): void {
    if (this.hasTransitionStoryBeat(storyBlock)) {
      return;
    }

    this.resetStoryBeatDialogState();
    this.storyBeatDialogBlock.set(storyBlock);
    this.isCreateStoryBeatDialogOpen.set(true);
  }

  protected openCreateSiblingStoryBeatDialog(
    storyBlock: StoryBlockViewModel,
    storyBeat: StoryBeatViewModel,
  ): void {
    if (this.hasTransitionStoryBeat(storyBlock)) {
      return;
    }

    this.resetStoryBeatDialogState();
    this.storyBeatDialogBlock.set(storyBlock);
    this.siblingStoryBeatDraftSource.set({
      storyBlockId: storyBlock.storyBlockId,
      orderIndex: storyBeat.orderIndex,
      displayIndex: storyBeat.displayIndex,
    });
    this.storyBeatTypeDraft.set(this.toStoryBeatType(storyBeat.storyBeatType));
    this.isCreateStoryBeatDialogOpen.set(true);

    if (this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Combat) {
      this.loadCombatNpcOptions();
    }
  }

  protected closeCreateStoryBeatDialog(): void {
    if (this.creatingStoryBeatBlockId() || this.updatingStoryBeatId()) {
      return;
    }

    this.isCreateStoryBeatDialogOpen.set(false);
    this.resetStoryBeatDialogState();
  }

  protected openEditStoryBeatDialog(
    storyBlock: StoryBlockViewModel,
    storyBeat: StoryBeatViewModel,
  ): void {
    this.storyBeatDialogBlock.set(storyBlock);
    this.editingStoryBeat.set(storyBeat);
    const storyBeatType = this.toStoryBeatType(storyBeat.storyBeatType);

    this.storyBeatTitleDraft.set(storyBeat.title ?? '');
    this.storyBeatTypeDraft.set(storyBeatType);
    this.storyBeatNarrativeDraft.set(storyBeat.information?.narrative ?? '');
    this.storyBeatRoleplayingDraft.set(storyBeat.roleplaying?.mainDescription ?? '');
    this.storyBeatDecisionDescriptionDraft.set(storyBeat.decision?.description ?? '');
    this.storyBeatCombatDescriptionDraft.set(storyBeat.combat?.description ?? '');
    this.storyBeatTransitionDescriptionDraft.set(storyBeat.transition?.description ?? '');
    this.storyBeatCombatRewardDrafts.set(
      this.toCombatRewardDrafts(storyBeat.combat?.rewards ?? null),
    );
    this.storyBeatMilestoneDraft.set(storyBeat.milestone?.id ?? null);
    this.storyBeatNarrativeParagraphDrafts.set(
      storyBeat.narrative?.paragraphs.length
        ? storyBeat.narrative.paragraphs.map((paragraph) => ({
          draftId: this.nextStoryBeatNarrativeParagraphDraftId++,
          text: paragraph,
        }))
        : [this.createStoryBeatNarrativeParagraphDraft()],
    );
    this.storyBeatOptionalInformationDrafts.set(
      storyBeatType === StoryBeatType.Information
        ? this.appendedStoryBeatInformation(storyBeat).map((information) => ({
          draftId: this.nextStoryBeatOptionalInformationDraftId++,
          skill: this.toSkill(information.skill) ?? Skill.Perception,
          difficultyClass: information.difficultyClass,
          information: information.information,
        }))
        : [],
    );
    this.storyBeatRoleplayingInformationDrafts.set(
      storyBeatType === StoryBeatType.Roleplaying
        ? (storyBeat.roleplaying?.discoverableInformation ?? []).map((information) => ({
          draftId: this.nextStoryBeatOptionalInformationDraftId++,
          npcTag: information.npcTag || this.findRoleplayingNpcTag(storyBeat, information.npcName ?? ''),
          npcName: this.findRoleplayingNpcName(storyBeat, information.npcTag, information.npcName),
          checkType: this.toRoleplayingCheckType(information.checkType),
          skill: this.toSkill(information.skill) ?? Skill.Perception,
          ability: this.toAbility(information.ability) ?? Ability.CHARISMA,
          difficultyClass: information.difficultyClass ?? 10,
          information: information.information,
        }))
        : [],
    );
    const decisionChoiceDrafts = storyBeatType === StoryBeatType.Decision &&
      (storyBeat.decision?.decisions.length ?? 0) > 0
      ? (storyBeat.decision?.decisions ?? []).map((choice) => ({
        draftId: this.nextStoryBeatDecisionChoiceDraftId++,
        id: choice.id ?? null,
        title: choice.title,
        description: choice.description,
      }))
      : [this.createStoryBeatDecisionChoiceDraft()];

    this.storyBeatDecisionChoiceDrafts.set(decisionChoiceDrafts);
    this.activeStoryBeatDecisionChoiceDraftId.set(decisionChoiceDrafts[0]?.draftId ?? null);
    this.storyBeatCombatEnemyNpcDrafts.set(
      storyBeatType === StoryBeatType.Combat
        ? (storyBeat.combat?.enemyNpcs ?? []).map((enemyNpc) => ({
          draftId: this.nextStoryBeatCombatEnemyNpcDraftId++,
          monsterId: enemyNpc.monsterId,
          amount: enemyNpc.amount,
        }))
        : [],
    );

    if (storyBeatType === StoryBeatType.Combat) {
      this.loadCombatNpcOptions();
    }

    this.isCreateStoryBeatDialogOpen.set(true);
  }

  protected openStoryBeatRulesDialog(storyBeat: StoryBeatViewModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingStoryBeatRules()) {
      return;
    }

    this.storyBeatRulesDialogBeat.set(storyBeat);
    this.storyBeatRule.set(null);
    this.storyBeatRuleDraft.set(null);
    this.storyBeatRuleEffectType.set(ConditionalRuleEffectType.RequiredForAvailability);
    this.isLoadingStoryBeatRules.set(true);

    forkJoin({
      events: this.campaignEventsApiService.fetchCampaignEvents(campaignId),
      rules: this.campaignRulesApiService.fetchRules(
        campaignId,
        ConditionalTargetType.StoryBeat,
        storyBeat.storyBeatId,
      ),
    })
      .pipe(finalize(() => this.isLoadingStoryBeatRules.set(false)))
      .subscribe({
        next: ({ events, rules }) => {
          const existingRule = (rules.data ?? [])[0] ?? null;

          this.storyBeatRuleEventOptions.set(events.data ?? []);
          this.storyBeatRule.set(existingRule);
          this.storyBeatRuleDraft.set(existingRule?.root ? this.cloneRuleGroup(existingRule.root) : null);
          this.storyBeatRuleEffectType.set(
            existingRule?.effectType ?? ConditionalRuleEffectType.RequiredForAvailability,
          );
        },
        error: (error: unknown) => {
          this.closeStoryBeatRulesDialog();
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat rules could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected closeStoryBeatRulesDialog(): void {
    if (this.isLoadingStoryBeatRules() || this.isSavingStoryBeatRule() || this.isDeletingStoryBeatRule()) {
      return;
    }

    this.storyBeatRulesDialogBeat.set(null);
    this.storyBeatRule.set(null);
    this.storyBeatRuleDraft.set(null);
    this.storyBeatRuleEventOptions.set([]);
  }

  protected saveStoryBeatRule(): void {
    const campaignId = this.getCampaignId();
    const storyBeat = this.storyBeatRulesDialogBeat();
    const root = this.storyBeatRuleDraft();

    if (!campaignId || !storyBeat || !root || this.isSavingStoryBeatRule()) {
      return;
    }

    const existingRule = this.storyBeatRule();
    const request = {
      effectType: this.storyBeatRuleEffectType(),
      targetType: ConditionalTargetType.StoryBeat,
      targetId: storyBeat.storyBeatId,
      root,
    };
    const saveRequest = existingRule
      ? this.campaignRulesApiService.updateRule(campaignId, existingRule.id, request)
      : this.campaignRulesApiService.createRule(campaignId, request);

    this.isSavingStoryBeatRule.set(true);
    saveRequest
      .pipe(finalize(() => this.isSavingStoryBeatRule.set(false)))
      .subscribe({
        next: (response) => {
          const savedRule = response.data ?? null;

          this.storyBeatRule.set(savedRule);
          this.setStoryBeatRuleSummary(storyBeat, savedRule);
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat rule could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected setStoryBeatRuleEffectType(event: Event): void {
    const value = Number((event.target as HTMLSelectElement).value);

    this.storyBeatRuleEffectType.set(
      value === ConditionalRuleEffectType.RequiredForVisibility
        ? ConditionalRuleEffectType.RequiredForVisibility
        : value === ConditionalRuleEffectType.ExclusivePath
          ? ConditionalRuleEffectType.ExclusivePath
          : value === ConditionalRuleEffectType.OptionalInformation
            ? ConditionalRuleEffectType.OptionalInformation
            : ConditionalRuleEffectType.RequiredForAvailability,
    );
  }

  protected deleteStoryBeatRule(): void {
    const campaignId = this.getCampaignId();
    const existingRule = this.storyBeatRule();

    if (!campaignId || !existingRule || this.isDeletingStoryBeatRule()) {
      return;
    }

    this.isDeletingStoryBeatRule.set(true);
    this.campaignRulesApiService
      .deleteRule(campaignId, existingRule.id)
      .pipe(finalize(() => this.isDeletingStoryBeatRule.set(false)))
      .subscribe({
        next: (response) => {
          this.storyBeatRule.set(null);
          this.storyBeatRuleDraft.set(null);
          this.storyBeatRuleEffectType.set(ConditionalRuleEffectType.RequiredForAvailability);
          if (this.storyBeatRulesDialogBeat()) {
            this.removeStoryBeatRuleSummary(this.storyBeatRulesDialogBeat()!, existingRule.id);
          }
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat rule could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected openStoryBeatIndexPathRuleDialog(
    storyBlock: StoryBlockViewModel,
    storyBeatRow: StoryBeatRowViewModel,
  ): void {
    if (storyBeatRow.beats.length < 2) {
      this.modalHelper.showError('Index path settings require at least two story beats at the same index.');
      return;
    }

    const existingRule = this.storyBeatIndexPathRuleFor(storyBeatRow);

    this.storyBeatIndexPathRuleDialog.set({ storyBlock, storyBeatRow });
    this.storyBeatIndexPathRelationTypeDraft.set(
      this.toStoryBeatIndexPathRuleRelationType(existingRule?.relationType) ??
        StoryBeatIndexPathRuleRelationType.ExactlyOne,
    );
    this.storyBeatIndexPathRequiredDraft.set(existingRule?.isRequired ?? false);
  }

  protected closeStoryBeatIndexPathRuleDialog(): void {
    if (this.isSavingStoryBeatIndexPathRule() || this.isDeletingStoryBeatIndexPathRule()) {
      return;
    }

    this.storyBeatIndexPathRuleDialog.set(null);
    this.storyBeatIndexPathRelationTypeDraft.set(StoryBeatIndexPathRuleRelationType.ExactlyOne);
    this.storyBeatIndexPathRequiredDraft.set(false);
  }

  protected setStoryBeatIndexPathRelationType(event: Event): void {
    const relationType = this.toStoryBeatIndexPathRuleRelationType(
      (event.target as HTMLInputElement).value,
    );

    if (relationType !== null) {
      this.storyBeatIndexPathRelationTypeDraft.set(relationType);
    }
  }

  protected setStoryBeatIndexPathRequired(event: Event): void {
    this.storyBeatIndexPathRequiredDraft.set((event.target as HTMLInputElement).checked);
  }

  protected saveStoryBeatIndexPathRule(): void {
    const campaignId = this.getCampaignId();
    const dialog = this.storyBeatIndexPathRuleDialog();

    if (!campaignId || !dialog || !this.canSaveStoryBeatIndexPathRule()) {
      return;
    }

    this.isSavingStoryBeatIndexPathRule.set(true);
    this.campaignApiService
      .upsertStoryBeatIndexPathRule(
        campaignId,
        dialog.storyBlock.storyBlockId,
        dialog.storyBeatRow.orderIndex,
        {
          relationType: this.storyBeatIndexPathRelationTypeDraft(),
          isRequired: this.storyBeatIndexPathRequiredDraft(),
        },
      )
      .pipe(finalize(() => this.isSavingStoryBeatIndexPathRule.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.applyStoryBeatIndexPathRule(response.data);
            this.storyBeatIndexPathRuleDialog.update((currentDialog) => currentDialog
              ? {
                ...currentDialog,
                storyBeatRow: {
                  ...currentDialog.storyBeatRow,
                  beats: currentDialog.storyBeatRow.beats.map((beat) => ({
                    ...beat,
                    indexPathRule: response.data!,
                  })),
                },
              }
              : currentDialog);
          }

          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat index path rule could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected deleteStoryBeatIndexPathRule(): void {
    const campaignId = this.getCampaignId();
    const dialog = this.storyBeatIndexPathRuleDialog();

    if (!campaignId || !dialog || !this.storyBeatIndexPathRuleFor(dialog.storyBeatRow) ||
      this.isDeletingStoryBeatIndexPathRule()) {
      return;
    }

    this.isDeletingStoryBeatIndexPathRule.set(true);
    this.campaignApiService
      .deleteStoryBeatIndexPathRule(
        campaignId,
        dialog.storyBlock.storyBlockId,
        dialog.storyBeatRow.orderIndex,
      )
      .pipe(finalize(() => this.isDeletingStoryBeatIndexPathRule.set(false)))
      .subscribe({
        next: (response) => {
          this.clearStoryBeatIndexPathRule(dialog.storyBlock.storyBlockId, dialog.storyBeatRow.orderIndex);
          this.storyBeatIndexPathRuleDialog.update((currentDialog) => currentDialog
            ? {
              ...currentDialog,
              storyBeatRow: {
                ...currentDialog.storyBeatRow,
                beats: currentDialog.storyBeatRow.beats.map((beat) => ({
                  ...beat,
                  indexPathRule: null,
                })),
              },
            }
            : currentDialog);
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat index path rule could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected openStoryBeatQuestTasksDialog(storyBeat: StoryBeatViewModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingStoryBeatQuestTasks()) {
      return;
    }

    this.storyBeatQuestTaskDialogBeat.set(storyBeat);
    this.storyBeatQuestTasks.set([]);
    this.campaignStoryBeatQuestTasks.set([]);
    this.storyBeatQuestTaskSearchDraft.set('');
    this.isLoadingStoryBeatQuestTasks.set(true);

    forkJoin({
      quests: this.campaignApiService.fetchCampaignQuests(campaignId),
      linkedTasks: this.campaignApiService.fetchCampaignStoryBeatQuestTasks(campaignId),
    })
      .pipe(finalize(() => this.isLoadingStoryBeatQuestTasks.set(false)))
      .subscribe({
        next: ({ quests, linkedTasks }) => {
          const campaignLinks = linkedTasks.data ?? [];

          this.quests.set(quests.data ?? []);
          this.campaignStoryBeatQuestTasks.set(campaignLinks);
          this.storyBeatQuestTasks.set(
            campaignLinks.filter((link) => link.storyBeatId === storyBeat.storyBeatId),
          );
        },
        error: (error: unknown) => {
          this.closeStoryBeatQuestTasksDialog(true);
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat quest tasks could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected closeStoryBeatQuestTasksDialog(force = false): void {
    if (!force && (
      this.isLoadingStoryBeatQuestTasks() ||
      this.linkingQuestTaskId() !== null ||
      this.unlinkingQuestTaskId() !== null
    )) {
      return;
    }

    this.storyBeatQuestTaskDialogBeat.set(null);
    this.storyBeatQuestTasks.set([]);
    this.campaignStoryBeatQuestTasks.set([]);
    this.storyBeatQuestTaskSearchDraft.set('');
  }

  protected setStoryBeatQuestTaskSearchDraft(event: Event): void {
    this.storyBeatQuestTaskSearchDraft.set((event.target as HTMLInputElement).value);
  }

  protected linkQuestTaskToStoryBeat(task: CampaignQuestTaskModel): void {
    const campaignId = this.getCampaignId();
    const storyBeat = this.storyBeatQuestTaskDialogBeat();

    if (!campaignId || !storyBeat || this.linkingQuestTaskId() !== null) {
      return;
    }

    this.linkingQuestTaskId.set(task.questTaskId);
    this.campaignApiService
      .linkQuestTaskToStoryBeat(campaignId, storyBeat.storyBeatId, task.questTaskId)
      .pipe(finalize(() => this.linkingQuestTaskId.set(null)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.storyBeatQuestTasks.update((tasks) => [...tasks, response.data!]);
            this.campaignStoryBeatQuestTasks.update((tasks) => [...tasks, response.data!]);
          }
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Quest task could not be linked to the story beat.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected unlinkQuestTaskFromStoryBeat(link: StoryBeatQuestTaskModel): void {
    const campaignId = this.getCampaignId();
    const storyBeat = this.storyBeatQuestTaskDialogBeat();

    if (!campaignId || !storyBeat || this.unlinkingQuestTaskId() !== null) {
      return;
    }

    this.unlinkingQuestTaskId.set(link.questTaskId);
    this.campaignApiService
      .unlinkQuestTaskFromStoryBeat(campaignId, storyBeat.storyBeatId, link.questTaskId)
      .pipe(finalize(() => this.unlinkingQuestTaskId.set(null)))
      .subscribe({
        next: (response) => {
          this.storyBeatQuestTasks.update((tasks) => (
            tasks.filter((task) => task.questTaskId !== link.questTaskId)
          ));
          this.campaignStoryBeatQuestTasks.update((tasks) => (
            tasks.filter((task) => task.questTaskId !== link.questTaskId)
          ));
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Quest task could not be unlinked from the story beat.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected storyBeatQuestTaskQuestTitle(link: StoryBeatQuestTaskModel): string {
    return this.quests().find((quest) => quest.questId === link.questId)?.title ?? 'Quest';
  }

  protected storyBeatQuestOptionLabel(quest: CampaignQuestModel): string {
    return `${quest.title} - ${this.questTypeLabel(quest)}`;
  }

  protected questTypeLabel(quest: CampaignQuestModel): string {
    const questType = this.toQuestType(quest.type);

    return this.questTypeOptions.find((option) => option.value === questType)?.label ?? 'Quest';
  }

  protected isEventAdjustableStoryBeat(storyBeat: StoryBeatViewModel): boolean {
    return this.isRoleplayingStoryBeat(storyBeat) ||
      this.isDecisionStoryBeat(storyBeat) ||
      this.isCombatStoryBeat(storyBeat) ||
      this.isMilestoneStoryBeat(storyBeat);
  }

  protected storyBeatRuleSummariesFor(storyBeat: StoryBeatViewModel): StoryBeatRuleSummary[] {
    return (this.storyBeatRuleSummaries()[storyBeat.storyBeatId] ?? [])
      .filter((rule) => (
        rule.effectType === ConditionalRuleEffectType.RequiredForAvailability ||
        rule.effectType === ConditionalRuleEffectType.RequiredForVisibility
      ))
      .map((rule) => ({
        rule,
        label: this.storyBeatRuleSummaryLabel(rule),
        description: this.storyBeatRuleSummaryDescription(rule.root),
      }));
  }

  protected isVisibilityStoryBeatRule(rule: ConditionalRuleModel): boolean {
    return rule.effectType === ConditionalRuleEffectType.RequiredForVisibility;
  }

  protected storyBeatRuleSummaryLabel(rule: ConditionalRuleModel): string {
    return rule.effectType === ConditionalRuleEffectType.RequiredForVisibility
      ? 'Required for Visibility'
      : 'Required for Availability';
  }

  protected storyBeatRuleSummaryDescription(root: CampaignRuleGroupRequest | null): string {
    if (!root) {
      return 'No conditions';
    }

    const conditions = this.toRuleConditionSummaries(root);
    const remainingCount = Math.max(0, conditions.length - 2);

    if (conditions.length === 0) {
      return 'No conditions';
    }

    return [
      conditions.slice(0, 2).join(` ${this.ruleGroupOperatorLabel(root.operator)} `),
      remainingCount > 0 ? `+${remainingCount}` : '',
    ].filter((part) => part.length > 0).join(' ');
  }

  protected hasNestedStoryBeatEventEffectTargets(storyBeat: StoryBeatViewModel): boolean {
    return this.isRoleplayingStoryBeat(storyBeat) || this.isDecisionStoryBeat(storyBeat);
  }

  protected storyBeatEventEffectSummariesFor(storyBeat: StoryBeatViewModel): StoryBeatOutcomeEffectSummary[] {
    return this.storyBeatEventEffectSummaries()[storyBeat.storyBeatId] ?? [];
  }

  protected hasStoryBeatEventEffects(storyBeat: StoryBeatViewModel): boolean {
    return this.storyBeatEventEffectSummariesFor(storyBeat).length > 0;
  }

  protected storyBeatEventEffectLabels(storyBeat: StoryBeatViewModel): string[] {
    return this.storyBeatEventLabelsFor(this.storyBeatEventEffectSummariesFor(storyBeat)).slice(0, 3);
  }

  protected storyBeatEventEffectOverflowCount(storyBeat: StoryBeatViewModel): number {
    const totalEventCount = this.storyBeatEventLabelsFor(
      this.storyBeatEventEffectSummariesFor(storyBeat),
    ).length;

    return Math.max(0, totalEventCount - this.storyBeatEventEffectLabels(storyBeat).length);
  }

  protected storyBeatEventEffectSummaryLabel(storyBeat: StoryBeatViewModel): string {
    const eventCount = this.storyBeatEventLabelsFor(
      this.storyBeatEventEffectSummariesFor(storyBeat),
    ).length;

    return `${eventCount} ${eventCount === 1 ? 'event' : 'events'}`;
  }

  protected storyBeatEventEffectSummaryLabels(summary: StoryBeatOutcomeEffectSummary): string[] {
    return summary.eventLabels.slice(0, 3);
  }

  protected storyBeatEventEffectSummaryOverflowCount(summary: StoryBeatOutcomeEffectSummary): number {
    return Math.max(0, summary.eventLabels.length - this.storyBeatEventEffectSummaryLabels(summary).length);
  }

  protected storyBeatEventCorrelationPartLabel(summary: StoryBeatOutcomeEffectSummary): string {
    return summary.label;
  }

  protected storyBeatEventEffectSummaryTooltip(summary: StoryBeatOutcomeEffectSummary): string {
    return `${this.storyBeatEventCorrelationPartLabel(summary)} correlates with ${summary.eventLabels.join(', ')}`;
  }

  protected storyBeatEventEffectSummaryAriaLabel(summary: StoryBeatOutcomeEffectSummary): string {
    return `Event correlations for ${this.storyBeatEventCorrelationPartLabel(summary)}: ${summary.eventLabels.join(', ')}`;
  }

  protected storyBeatEventEffectTooltip(storyBeat: StoryBeatViewModel): string {
    return this.storyBeatEventEffectSummariesFor(storyBeat)
      .map((summary) => this.storyBeatEventEffectSummaryTooltip(summary))
      .join('\n');
  }

  private storyBeatEventLabelsFor(summaries: StoryBeatOutcomeEffectSummary[]): string[] {
    return Array.from(new Set(summaries.flatMap((summary) => summary.eventLabels)));
  }

  protected openStoryBeatEventEffectsDialog(storyBeat: StoryBeatViewModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingStoryBeatEventEffects()) {
      return;
    }

    this.storyBeatEventEffectsDialogBeat.set(storyBeat);
    this.storyBeatEventEffects.set([]);
    this.storyBeatEventEffectDrafts.set([]);
    this.storyBeatEventEffectOptions.set([]);
    this.selectedRoleplayingEventSource.set(null);

    const nestedTargets = this.hasNestedStoryBeatEventEffectTargets(storyBeat)
      ? this.storyBeatOutcomeEffectTargets(storyBeat)
      : [];
    const initialSource = nestedTargets.length === 1
      ? this.toStoryBeatOutcomeEffectSource(nestedTargets[0]!)
      : this.hasNestedStoryBeatEventEffectTargets(storyBeat)
        ? null
        : this.toStoryBeatOutcomeEffectSource(storyBeat);

    if (this.hasNestedStoryBeatEventEffectTargets(storyBeat)) {
      this.selectedRoleplayingEventSource.set(initialSource);
    }

    if (this.hasNestedStoryBeatEventEffectTargets(storyBeat)) {
      this.isLoadingStoryBeatEventEffects.set(true);
      forkJoin({
        events: this.campaignEventsApiService.fetchCampaignEvents(campaignId),
        effects: initialSource
          ? this.outcomeEffectsApiService.fetchOutcomeEffects(
            campaignId,
            initialSource.sourceType,
            initialSource.sourceId,
          )
          : of({ data: [] as OutcomeEffectModel[] }),
      })
        .pipe(finalize(() => this.isLoadingStoryBeatEventEffects.set(false)))
        .subscribe({
          next: ({ events, effects }) => {
            const outcomeEffects = effects.data ?? [];

            this.storyBeatEventEffectOptions.set(events.data ?? []);
            this.storyBeatEventEffects.set(outcomeEffects);
            this.storyBeatEventEffectDrafts.set(outcomeEffects.map((effect) => (
              this.toOutcomeEffectDraft(effect)
            )));

            if (initialSource) {
              this.setStoryBeatEventEffectSummary(storyBeat, initialSource, outcomeEffects);
            }
          },
          error: (error: unknown) => {
            this.closeStoryBeatEventEffectsDialog();
            this.modalHelper.showError(
              this.getErrorMessage(error, 'Nested story beat event correlations could not be loaded.'),
              { statusCode: this.getErrorStatus(error) },
            );
          },
        });
      return;
    }

    this.isLoadingStoryBeatEventEffects.set(true);
    forkJoin({
      events: this.campaignEventsApiService.fetchCampaignEvents(campaignId),
      effects: this.outcomeEffectsApiService.fetchOutcomeEffects(
        campaignId,
        OutcomeSourceType.StoryBeat,
        storyBeat.storyBeatId,
      ),
    })
      .pipe(finalize(() => this.isLoadingStoryBeatEventEffects.set(false)))
      .subscribe({
        next: ({ events, effects }) => {
          const outcomeEffects = effects.data ?? [];

          this.storyBeatEventEffectOptions.set(events.data ?? []);
          this.storyBeatEventEffects.set(outcomeEffects);
          this.storyBeatEventEffectDrafts.set(outcomeEffects.map((effect) => (
            this.toOutcomeEffectDraft(effect)
          )));
          this.setStoryBeatEventEffectSummary(
            storyBeat,
            { sourceType: OutcomeSourceType.StoryBeat, sourceId: storyBeat.storyBeatId },
            outcomeEffects,
          );
        },
        error: (error: unknown) => {
          this.closeStoryBeatEventEffectsDialog();
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat event correlations could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected closeStoryBeatEventEffectsDialog(): void {
    if (
      this.isLoadingStoryBeatEventEffects() ||
      this.isSavingStoryBeatEventEffects() ||
      this.isDeletingStoryBeatEventEffects()
    ) {
      return;
    }

    this.storyBeatEventEffectsDialogBeat.set(null);
    this.storyBeatEventEffects.set([]);
    this.storyBeatEventEffectDrafts.set([]);
    this.storyBeatEventEffectOptions.set([]);
    this.selectedRoleplayingEventSource.set(null);
  }

  protected selectStoryBeatEventSource(target: StoryBeatOutcomeEffectTarget): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingStoryBeatEventEffects()) {
      return;
    }

    const source = this.toStoryBeatOutcomeEffectSource(target);

    this.selectedRoleplayingEventSource.set(source);
    this.storyBeatEventEffects.set([]);
    this.storyBeatEventEffectDrafts.set([]);
    this.isLoadingStoryBeatEventEffects.set(true);
    this.outcomeEffectsApiService
      .fetchOutcomeEffects(campaignId, source.sourceType, source.sourceId)
      .pipe(finalize(() => this.isLoadingStoryBeatEventEffects.set(false)))
      .subscribe({
        next: (response) => {
          const outcomeEffects = response.data ?? [];

          this.storyBeatEventEffects.set(outcomeEffects);
          this.storyBeatEventEffectDrafts.set(outcomeEffects.map((effect) => (
            this.toOutcomeEffectDraft(effect)
          )));
          const storyBeat = this.storyBeatEventEffectsDialogBeat();

          if (storyBeat) {
            this.setStoryBeatEventEffectSummary(storyBeat, source, outcomeEffects);
          }
        },
        error: (error: unknown) => {
          this.selectedRoleplayingEventSource.set(null);
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat event correlations could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected clearSelectedRoleplayingEventSource(): void {
    this.selectedRoleplayingEventSource.set(null);
    this.storyBeatEventEffects.set([]);
    this.storyBeatEventEffectDrafts.set([]);
  }

  protected hasSelectedStoryBeatEventEffectSource(storyBeat: StoryBeatViewModel): boolean {
    return !this.hasNestedStoryBeatEventEffectTargets(storyBeat) ||
      this.selectedRoleplayingEventSource() !== null;
  }

  protected saveStoryBeatEventEffects(): void {
    const campaignId = this.getCampaignId();
    const storyBeat = this.storyBeatEventEffectsDialogBeat();

    if (!campaignId || !storyBeat || !this.canSaveStoryBeatEventEffects()) {
      return;
    }

    const source = this.toStoryBeatOutcomeEffectSource(storyBeat);

    this.isSavingStoryBeatEventEffects.set(true);
    this.replaceStoryBeatOutcomeEffects(campaignId, source)
      .pipe(finalize(() => this.isSavingStoryBeatEventEffects.set(false)))
      .subscribe({
        next: (effects) => {
          this.storyBeatEventEffects.set(effects);
          this.storyBeatEventEffectDrafts.set(effects.map((effect) => this.toOutcomeEffectDraft(effect)));
          this.setStoryBeatEventEffectSummary(storyBeat, source, effects);
          this.modalHelper.showSuccess('Story beat event correlations saved.');
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat event correlations could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected clearStoryBeatEventEffects(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isDeletingStoryBeatEventEffects()) {
      return;
    }

    const effects = this.storyBeatEventEffects();
    const storyBeat = this.storyBeatEventEffectsDialogBeat();
    const source = storyBeat ? this.toStoryBeatOutcomeEffectSource(storyBeat) : null;

    if (effects.length === 0) {
      this.storyBeatEventEffectDrafts.set([]);
      if (storyBeat && source) {
        this.setStoryBeatEventEffectSummary(storyBeat, source, []);
      }
      return;
    }

    this.isDeletingStoryBeatEventEffects.set(true);
    forkJoin(effects.map((effect) => this.outcomeEffectsApiService.deleteOutcomeEffect(
      campaignId,
      effect.id,
    )))
      .pipe(finalize(() => this.isDeletingStoryBeatEventEffects.set(false)))
      .subscribe({
        next: () => {
          this.storyBeatEventEffects.set([]);
          this.storyBeatEventEffectDrafts.set([]);
          if (storyBeat && source) {
            this.setStoryBeatEventEffectSummary(storyBeat, source, []);
          }
          this.modalHelper.showSuccess('Story beat event correlations cleared.');
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat event correlations could not be cleared.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected confirmDeleteStoryBeat(
    storyBlock: StoryBlockViewModel,
    storyBeat: StoryBeatViewModel,
  ): void {
    this.deleteConfirmationStoryBeat.set({ storyBlock, storyBeat });
  }

  protected cancelDeleteStoryBeat(): void {
    if (this.deletingStoryBeatId()) {
      return;
    }

    this.deleteConfirmationStoryBeat.set(null);
  }

  protected storyBeatDialogTitle(): string {
    if (this.editingStoryBeat()) {
      return 'Edit Story Beat';
    }

    return this.siblingStoryBeatDraftSource()
      ? 'Create Sibling Story Beat'
      : 'Create Story Beat';
  }

  protected storyBeatDialogActionText(): string {
    if (this.creatingStoryBeatBlockId()) {
      return 'Creating...';
    }

    if (this.updatingStoryBeatId()) {
      return 'Updating...';
    }

    if (this.editingStoryBeat()) {
      return 'Update';
    }

    return this.siblingStoryBeatDraftSource() ? 'Create Sibling' : 'Create';
  }

  protected storyBeatTypeDraftValue(): string {
    return this.storyBeatTypeDraft().toString();
  }

  protected hasValidStoryBeatTypeDraft(): boolean {
    return this.storyBeatTypeOptions.some((option) => (
      !option.disabled && option.value === this.storyBeatTypeDraft()
    ));
  }

  protected setStoryBeatTypeDraft(event: Event): void {
    const value = Number((event.target as HTMLSelectElement).value);

    this.storyBeatTypeDraft.set(
      value === StoryBeatType.Narrative
        ? StoryBeatType.Narrative
        : value === StoryBeatType.Roleplaying
          ? StoryBeatType.Roleplaying
          : value === StoryBeatType.Decision
            ? StoryBeatType.Decision
            : value === StoryBeatType.Combat
              ? StoryBeatType.Combat
              : value === StoryBeatType.Transition
                ? StoryBeatType.Transition
                : value === StoryBeatType.Milestone
                  ? StoryBeatType.Milestone
                  : StoryBeatType.Information,
    );

    if (this.storyBeatTypeDraft() === StoryBeatType.Combat) {
      this.loadCombatNpcOptions();
    }
  }

  protected setStoryBeatTitleDraft(event: Event): void {
    this.storyBeatTitleDraft.set((event.target as HTMLInputElement).value);
  }

  protected setStoryBeatNarrativeDraft(event: Event): void {
    const value = (event.target as HTMLTextAreaElement).value;

    this.storyBeatNarrativeDraft.set(this.convertSlashTokens(value));
  }

  protected setStoryBeatRoleplayingDraft(event: Event): void {
    const value = (event.target as HTMLTextAreaElement).value;
    const npcOptions = this.toRoleplayingNpcOptions(value);

    this.storyBeatRoleplayingDraft.set(value);
    this.storyBeatRoleplayingInformationDrafts.update((drafts) => drafts.map((draft) => (
      npcOptions.some((npc) => npc.key === draft.npcTag)
        ? draft
        : {
          ...draft,
          npcTag: npcOptions[0]?.key ?? '',
          npcName: npcOptions[0]?.name ?? '',
        }
    )));
  }

  protected selectInlineSkillSuggestion(skill: Skill): void {
    const narrative = this.storyBeatNarrativeDraft();
    const slashIndex = narrative.lastIndexOf('/');

    if (slashIndex < 0) {
      return;
    }

    const beforeSlash = narrative.slice(0, slashIndex);
    const afterSlash = narrative.slice(slashIndex + 1);
    const separatorIndex = afterSlash.indexOf('::');
    const afterSkillQuery = separatorIndex >= 0 ? afterSlash.slice(separatorIndex) : '';
    const skillLabel = this.getSkillLabel(skill);

    this.storyBeatNarrativeDraft.set(`${beforeSlash}/${skillLabel}${afterSkillQuery || '::'}`);
  }

  protected isInformationStoryBeatDraft(): boolean {
    return this.storyBeatTypeDraft() === StoryBeatType.Information;
  }

  protected isNarrativeStoryBeatDraft(): boolean {
    return this.storyBeatTypeDraft() === StoryBeatType.Narrative;
  }

  protected isRoleplayingStoryBeatDraft(): boolean {
    return this.storyBeatTypeDraft() === StoryBeatType.Roleplaying;
  }

  protected isDecisionStoryBeatDraft(): boolean {
    return this.storyBeatTypeDraft() === StoryBeatType.Decision;
  }

  protected isCombatStoryBeatDraft(): boolean {
    return this.storyBeatTypeDraft() === StoryBeatType.Combat;
  }

  protected isTransitionStoryBeatDraft(): boolean {
    return this.storyBeatTypeDraft() === StoryBeatType.Transition;
  }

  protected isMilestoneStoryBeatDraft(): boolean {
    return this.storyBeatTypeDraft() === StoryBeatType.Milestone;
  }

  protected setStoryBeatTransitionDescriptionDraft(event: Event): void {
    this.storyBeatTransitionDescriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected setStoryBeatCombatDescriptionDraft(event: Event): void {
    this.storyBeatCombatDescriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected addStoryBeatCombatRewardDraft(): void {
    this.storyBeatCombatRewardDrafts.update((drafts) => [
      ...drafts,
      this.createStoryBeatCombatRewardDraft(),
    ]);
  }

  protected removeStoryBeatCombatRewardDraft(draftId: number): void {
    this.storyBeatCombatRewardDrafts.update((drafts) => (
      drafts.filter((draft) => draft.draftId !== draftId)
    ));
  }

  protected setStoryBeatCombatRewardDraft(draftId: number, event: Event): void {
    const text = (event.target as HTMLInputElement).value;

    this.storyBeatCombatRewardDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, text } : draft
    )));
  }

  protected toggleStoryBeatCombatEnemyNpc(monsterId: number): void {
    this.storyBeatCombatEnemyNpcDrafts.update((drafts) => {
      if (drafts.some((draft) => draft.monsterId === monsterId)) {
        return drafts.filter((draft) => draft.monsterId !== monsterId);
      }

      return [
        ...drafts,
        this.createStoryBeatCombatEnemyNpcDraft(monsterId),
      ];
    });
  }

  protected isCombatNpcSelected(monsterId: number): boolean {
    return this.storyBeatCombatEnemyNpcDrafts().some((draft) => draft.monsterId === monsterId);
  }

  protected storyBeatCombatEnemyAmountFor(monsterId: number): number {
    return this.storyBeatCombatEnemyNpcDrafts()
      .find((draft) => draft.monsterId === monsterId)
      ?.amount ?? 1;
  }

  protected setStoryBeatCombatEnemyAmount(monsterId: number, event: Event): void {
    const amount = Number((event.target as HTMLInputElement).value);

    this.storyBeatCombatEnemyNpcDrafts.update((drafts) => drafts.map((draft) => (
      draft.monsterId === monsterId
        ? { ...draft, amount: Number.isInteger(amount) && amount > 0 ? amount : 1 }
        : draft
    )));
  }

  protected combatNpcOptionLabel(monster: MonsterModel): string {
    return [monster.name, monster.race, monster.class]
      .map((value) => this.normalizeText(value))
      .filter((value) => value.length > 0)
      .join(' - ');
  }

  protected setStoryBeatMilestoneDraft(event: Event): void {
    const value = Number((event.target as HTMLSelectElement).value);

    this.storyBeatMilestoneDraft.set(Number.isFinite(value) && value > 0 ? value : null);
  }

  protected addStoryBeatNarrativeParagraphDraft(): void {
    if (this.storyBeatNarrativeParagraphDrafts().length >= 10) {
      return;
    }

    this.storyBeatNarrativeParagraphDrafts.update((drafts) => [
      ...drafts,
      this.createStoryBeatNarrativeParagraphDraft(),
    ]);
  }

  protected removeStoryBeatNarrativeParagraphDraft(draftId: number): void {
    this.storyBeatNarrativeParagraphDrafts.update((drafts) => {
      const nextDrafts = drafts.filter((draft) => draft.draftId !== draftId);

      return nextDrafts.length > 0 ? nextDrafts : [this.createStoryBeatNarrativeParagraphDraft()];
    });
  }

  protected setStoryBeatNarrativeParagraphDraft(draftId: number, event: Event): void {
    const text = (event.target as HTMLTextAreaElement).value;

    this.storyBeatNarrativeParagraphDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, text } : draft
    )));
  }

  protected addStoryBeatOptionalInformationDraft(): void {
    this.storyBeatOptionalInformationDrafts.update((drafts) => [
      ...drafts,
      this.createStoryBeatOptionalInformationDraft(),
    ]);
  }

  protected removeStoryBeatOptionalInformationDraft(draftId: number): void {
    this.storyBeatOptionalInformationDrafts.update((drafts) => (
      drafts.filter((draft) => draft.draftId !== draftId)
    ));
  }

  protected setStoryBeatOptionalInformationSkill(draftId: number, event: Event): void {
    const skill = Number((event.target as HTMLSelectElement).value) as Skill;

    this.storyBeatOptionalInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, skill } : draft
    )));
  }

  protected setStoryBeatOptionalInformationDifficulty(draftId: number, event: Event): void {
    const difficultyClass = Number((event.target as HTMLInputElement).value);

    this.storyBeatOptionalInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, difficultyClass } : draft
    )));
  }

  protected setStoryBeatOptionalInformationText(draftId: number, event: Event): void {
    const information = (event.target as HTMLTextAreaElement).value;

    this.storyBeatOptionalInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, information } : draft
    )));
  }

  protected addStoryBeatRoleplayingInformationDraft(): void {
    this.storyBeatRoleplayingInformationDrafts.update((drafts) => [
      ...drafts,
      this.createStoryBeatRoleplayingInformationDraft(),
    ]);
  }

  protected removeStoryBeatRoleplayingInformationDraft(draftId: number): void {
    this.storyBeatRoleplayingInformationDrafts.update((drafts) => (
      drafts.filter((draft) => draft.draftId !== draftId)
    ));
  }

  protected setStoryBeatRoleplayingInformationNpc(draftId: number, event: Event): void {
    const npcTag = (event.target as HTMLSelectElement).value;
    const npcName = this.roleplayingNpcOptions().find((npc) => npc.key === npcTag)?.name ?? '';

    this.storyBeatRoleplayingInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, npcTag, npcName } : draft
    )));
  }

  protected setStoryBeatRoleplayingInformationCheckType(draftId: number, event: Event): void {
    const checkType = Number((event.target as HTMLSelectElement).value) as StoryBeatRoleplayingCheckType;

    this.storyBeatRoleplayingInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, checkType } : draft
    )));
  }

  protected setStoryBeatRoleplayingInformationSkill(draftId: number, event: Event): void {
    const skill = Number((event.target as HTMLSelectElement).value) as Skill;

    this.storyBeatRoleplayingInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, skill } : draft
    )));
  }

  protected setStoryBeatRoleplayingInformationAbility(draftId: number, event: Event): void {
    const ability = Number((event.target as HTMLSelectElement).value) as Ability;

    this.storyBeatRoleplayingInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, ability } : draft
    )));
  }

  protected setStoryBeatRoleplayingInformationDifficulty(draftId: number, event: Event): void {
    const difficultyClass = Number((event.target as HTMLInputElement).value);

    this.storyBeatRoleplayingInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, difficultyClass } : draft
    )));
  }

  protected setStoryBeatRoleplayingInformationText(draftId: number, event: Event): void {
    const information = (event.target as HTMLTextAreaElement).value;

    this.storyBeatRoleplayingInformationDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, information } : draft
    )));
  }

  protected setStoryBeatDecisionDescriptionDraft(event: Event): void {
    this.storyBeatDecisionDescriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected addStoryBeatDecisionChoiceDraft(): void {
    if (this.storyBeatDecisionChoiceDrafts().length >= 20) {
      return;
    }

    const draft = this.createStoryBeatDecisionChoiceDraft();

    this.storyBeatDecisionChoiceDrafts.update((drafts) => [
      ...drafts,
      draft,
    ]);
    this.activeStoryBeatDecisionChoiceDraftId.set(draft.draftId);
  }

  protected removeStoryBeatDecisionChoiceDraft(draftId: number): void {
    const drafts = this.storyBeatDecisionChoiceDrafts();
    const removedIndex = drafts.findIndex((draft) => draft.draftId === draftId);
    const nextDrafts = drafts.filter((draft) => draft.draftId !== draftId);
    const fallbackDrafts = nextDrafts.length > 0
      ? nextDrafts
      : [this.createStoryBeatDecisionChoiceDraft()];

    this.storyBeatDecisionChoiceDrafts.set(fallbackDrafts);

    if (
      this.activeStoryBeatDecisionChoiceDraftId() === draftId ||
      !fallbackDrafts.some((draft) => draft.draftId === this.activeStoryBeatDecisionChoiceDraftId())
    ) {
      const nextActiveIndex = Math.min(Math.max(removedIndex, 0), fallbackDrafts.length - 1);

      this.activeStoryBeatDecisionChoiceDraftId.set(
        fallbackDrafts[nextActiveIndex]?.draftId ?? null,
      );
    }
  }

  protected selectStoryBeatDecisionChoiceDraft(draftId: number): void {
    if (this.storyBeatDecisionChoiceDrafts().some((draft) => draft.draftId === draftId)) {
      this.activeStoryBeatDecisionChoiceDraftId.set(draftId);
    }
  }

  protected isActiveStoryBeatDecisionChoiceDraft(draftId: number): boolean {
    return this.activeStoryBeatDecisionChoiceDraft()?.draftId === draftId;
  }

  protected setStoryBeatDecisionChoiceTitle(draftId: number, event: Event): void {
    const title = (event.target as HTMLInputElement).value;

    this.storyBeatDecisionChoiceDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, title } : draft
    )));
  }

  protected setStoryBeatDecisionChoiceDescription(draftId: number, event: Event): void {
    const description = (event.target as HTMLTextAreaElement).value;

    this.storyBeatDecisionChoiceDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, description } : draft
    )));
  }

  protected saveStoryBeat(): void {
    const editingStoryBeat = this.editingStoryBeat();

    if (editingStoryBeat) {
      this.updateStoryBeat(editingStoryBeat);
      return;
    }

    this.createStoryBeat();
  }

  protected deleteStoryBeat(): void {
    const campaignId = this.getCampaignId();
    const confirmation = this.deleteConfirmationStoryBeat();

    if (!campaignId || !confirmation || this.deletingStoryBeatId()) {
      return;
    }

    const { storyBlock, storyBeat } = confirmation;

    this.deletingStoryBeatId.set(storyBeat.storyBeatId);

    this.campaignApiService
      .deleteStoryBeat(campaignId, storyBlock.storyBlockId, storyBeat.storyBeatId)
      .pipe(finalize(() => this.deletingStoryBeatId.set(null)))
      .subscribe({
        next: () => {
          this.deleteConfirmationStoryBeat.set(null);

          if (storyBlock.beats.some((beat) => this.isTransitionStoryBeat(beat))) {
            this.loadStoryContent();
            return;
          }

          this.storyBlocks.update((blocks) => blocks.map((block) => (
            block.storyBlockId === storyBlock.storyBlockId
              ? {
                ...block,
                beats: block.beats
                  .filter((beat) => beat.storyBeatId !== storyBeat.storyBeatId)
                  .map((beat, index) => ({
                    ...beat,
                    displayIndex: index + 1,
                  })),
              }
              : block
          )));
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private createStoryBeat(): void {
    const campaignId = this.getCampaignId();
    const storyBlock = this.storyBeatDialogBlock();

    if (!campaignId || !storyBlock || !this.canCreateStoryBeat()) {
      return;
    }

    const storyBeatType = this.storyBeatTypeDraft();
    const siblingSource = this.siblingStoryBeatDraftSource();

    this.creatingStoryBeatBlockId.set(storyBlock.storyBlockId);

    const saveStoryBeat = storyBeatType === StoryBeatType.Narrative
      ? this.campaignApiService.createNarrativeStoryBeat(
        campaignId,
        storyBlock.storyBlockId,
        this.toNarrativeStoryBeatRequest(),
      )
      : storyBeatType === StoryBeatType.Roleplaying
        ? this.ensureRoleplayingNpcRecords(campaignId).pipe(
          switchMap(() => this.campaignApiService.createRoleplayingStoryBeat(
            campaignId,
            storyBlock.storyBlockId,
            this.toRoleplayingStoryBeatRequest(),
          )),
        )
        : storyBeatType === StoryBeatType.Decision
          ? this.campaignApiService.createDecisionStoryBeat(
            campaignId,
            storyBlock.storyBlockId,
            this.toDecisionStoryBeatRequest(),
          )
            : storyBeatType === StoryBeatType.Combat
              ? this.campaignApiService.createCombatStoryBeat(
                campaignId,
                storyBlock.storyBlockId,
                this.toCombatStoryBeatRequest(),
              )
              : storyBeatType === StoryBeatType.Transition
                ? this.campaignApiService.createTransitionStoryBeat(
                  campaignId,
                  storyBlock.storyBlockId,
                  this.toTransitionStoryBeatRequest(),
                )
                : storyBeatType === StoryBeatType.Milestone
                  ? this.campaignApiService.createMilestoneStoryBeat(
                    campaignId,
                    storyBlock.storyBlockId,
                    this.toMilestoneStoryBeatRequest(),
                  )
                  : this.campaignApiService.createInformationStoryBeat(
                    campaignId,
                    storyBlock.storyBlockId,
                    this.toInformationStoryBeatRequest(),
                  );

    saveStoryBeat
      .pipe(finalize(() => this.creatingStoryBeatBlockId.set(null)))
      .subscribe({
        next: (response) => {
          this.isCreateStoryBeatDialogOpen.set(false);
          this.storyBeatDialogBlock.set(null);
          this.siblingStoryBeatDraftSource.set(null);

          const storyBeat = response.data;

          if (!storyBeat) {
            this.loadStoryContent();
            return;
          }

          if (
            this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Transition ||
            storyBlock.beats.some((beat) => this.isTransitionStoryBeat(beat))
          ) {
            this.loadStoryContent();
            return;
          }

          let siblingAlternativeIndex: number | null = null;

          this.storyBlocks.update((blocks) => blocks.map((block) => {
            if (block.storyBlockId !== storyBlock.storyBlockId) {
              return block;
            }

            const orderIndex = siblingSource?.orderIndex ?? storyBeat.orderIndex;
            const storyBeatViewModel: StoryBeatViewModel = {
              ...storyBeat,
              orderIndex,
              secondaryOrderIndex: storyBeat.secondaryOrderIndex ?? 1,
              milestone: storyBeat.milestone ?? null,
              displayIndex: siblingSource?.displayIndex ?? storyBeat.orderIndex,
            };
            const beats = [...block.beats, storyBeatViewModel];

            if (siblingSource) {
              siblingAlternativeIndex = beats
                .filter((beat) => beat.orderIndex === orderIndex)
                .sort((firstBeat, secondBeat) => (
                  firstBeat.secondaryOrderIndex - secondBeat.secondaryOrderIndex
                ))
                .findIndex((beat) => beat.storyBeatId === storyBeat.storyBeatId);
            }

            return {
              ...block,
              beats,
            };
          }));

          if (siblingSource && siblingAlternativeIndex !== null) {
            this.setStoryBeatAlternativeIndexByKey(
              this.storyBeatRowKey(siblingSource.storyBlockId, siblingSource.orderIndex),
              siblingAlternativeIndex,
            );
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private updateStoryBeat(storyBeatToUpdate: StoryBeatViewModel): void {
    const campaignId = this.getCampaignId();
    const storyBlock = this.storyBeatDialogBlock();

    if (!campaignId || !storyBlock || !this.canCreateStoryBeat()) {
      return;
    }

    const storyBeatType = this.storyBeatTypeDraft();

    this.updatingStoryBeatId.set(storyBeatToUpdate.storyBeatId);

    const saveStoryBeat = storyBeatType === StoryBeatType.Narrative
      ? this.campaignApiService.updateNarrativeStoryBeat(
        campaignId,
        storyBlock.storyBlockId,
        storyBeatToUpdate.storyBeatId,
        this.toNarrativeStoryBeatRequest(),
      )
      : storyBeatType === StoryBeatType.Roleplaying
        ? this.ensureRoleplayingNpcRecords(campaignId).pipe(
          switchMap(() => this.campaignApiService.updateRoleplayingStoryBeat(
            campaignId,
            storyBlock.storyBlockId,
            storyBeatToUpdate.storyBeatId,
            this.toRoleplayingStoryBeatRequest(),
          )),
        )
        : storyBeatType === StoryBeatType.Decision
          ? this.campaignApiService.updateDecisionStoryBeat(
            campaignId,
            storyBlock.storyBlockId,
            storyBeatToUpdate.storyBeatId,
            this.toDecisionStoryBeatRequest(),
          )
            : storyBeatType === StoryBeatType.Combat
              ? this.campaignApiService.updateCombatStoryBeat(
                campaignId,
                storyBlock.storyBlockId,
                storyBeatToUpdate.storyBeatId,
                this.toCombatStoryBeatRequest(),
              )
              : storyBeatType === StoryBeatType.Transition
                ? this.campaignApiService.updateTransitionStoryBeat(
                  campaignId,
                  storyBlock.storyBlockId,
                  storyBeatToUpdate.storyBeatId,
                  this.toTransitionStoryBeatRequest(),
                )
                : storyBeatType === StoryBeatType.Milestone
                  ? this.campaignApiService.updateMilestoneStoryBeat(
                    campaignId,
                    storyBlock.storyBlockId,
                    storyBeatToUpdate.storyBeatId,
                    this.toMilestoneStoryBeatRequest(),
                  )
                  : this.campaignApiService.updateInformationStoryBeat(
                    campaignId,
                    storyBlock.storyBlockId,
                    storyBeatToUpdate.storyBeatId,
                    this.toInformationStoryBeatRequest(),
                  );

    saveStoryBeat
      .pipe(finalize(() => this.updatingStoryBeatId.set(null)))
      .subscribe({
        next: (response) => {
          this.isCreateStoryBeatDialogOpen.set(false);
          this.storyBeatDialogBlock.set(null);
          this.editingStoryBeat.set(null);

          const storyBeat = response.data;

          if (!storyBeat) {
            this.loadStoryContent();
            return;
          }

          if (storyBlock.beats.some((beat) => this.isTransitionStoryBeat(beat))) {
            this.loadStoryContent();
            return;
          }

          this.storyBlocks.update((blocks) => blocks.map((block) => (
            block.storyBlockId === storyBlock.storyBlockId
              ? {
                ...block,
                beats: block.beats.map((beat) => (
                  beat.storyBeatId === storyBeat.storyBeatId
                    ? {
                      ...storyBeat,
                      orderIndex: beat.orderIndex,
                      secondaryOrderIndex: beat.secondaryOrderIndex,
                      milestone: storyBeat.milestone ?? beat.milestone ?? null,
                      displayIndex: beat.displayIndex,
                    }
                    : beat
                )),
              }
              : block
          )));
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beat could not be updated.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected storyBlockTitle(storyBlock: StoryBlockViewModel): string {
    return storyBlock.title;
  }

  protected storyBlockSummary(storyBlock: StoryBlockViewModel): string {
    const beatCount = storyBlock.beats.length;

    if (beatCount === 0) {
      return 'No story beats yet.';
    }

    return `${beatCount} ${beatCount === 1 ? 'story beat' : 'story beats'} planned.`;
  }

  protected storyBlockMusicCount(storyBlock: StoryBlockViewModel): number {
    return storyBlock.musicFiles?.length ?? 0;
  }

  protected storyBeatMusicCount(storyBeat: StoryBeatViewModel): number {
    return storyBeat.musicFiles?.length ?? 0;
  }

  protected libraryMusicFileLabel(file: LibraryFileModel): string {
    return file.displayName || file.originalFileName;
  }

  protected storyBlockMusicDraftFile(draft: StoryBlockMusicDraft): LibraryFileModel | null {
    return this.libraryMusicFiles().find((file) => file.id === draft.musicFileId) ?? null;
  }

  protected storyBlockMusicDraftTargetLabel(
    storyBlock: StoryBlockViewModel,
    draft: StoryBlockMusicDraft,
  ): string {
    if (!draft.storyBeatId) {
      return 'Whole block';
    }

    const beat = storyBlock.beats.find((candidate) => candidate.storyBeatId === draft.storyBeatId);

    return beat ? this.storyBeatTitle(beat) : 'Missing beat';
  }

  protected storyBeatRowsFor(storyBlock: StoryBlockViewModel): StoryBeatRowViewModel[] {
    const groupedBeats = new Map<number, StoryBeatViewModel[]>();

    storyBlock.beats.forEach((storyBeat) => {
      const orderIndex = storyBeat.orderIndex;
      const rowBeats = groupedBeats.get(orderIndex) ?? [];

      rowBeats.push(storyBeat);
      groupedBeats.set(orderIndex, rowBeats);
    });

    return Array.from(groupedBeats.entries())
      .sort(([firstOrderIndex], [secondOrderIndex]) => firstOrderIndex - secondOrderIndex)
      .map(([orderIndex, beats]) => {
        const sortedBeats = [...beats].sort((firstBeat, secondBeat) => (
          firstBeat.secondaryOrderIndex - secondBeat.secondaryOrderIndex
        ));
        const key = this.storyBeatRowKey(storyBlock.storyBlockId, orderIndex);
        const activeIndex = this.clampStoryBeatAlternativeIndex(
          this.storyBeatAlternativeIndexes()[key] ?? 0,
          sortedBeats,
        );

        return {
          key,
          storyBlockId: storyBlock.storyBlockId,
          orderIndex,
          beats: sortedBeats,
          activeIndex,
          activeBeat: sortedBeats[activeIndex] ?? null,
        };
      });
  }

  protected canMoveStoryBeatRow(
    storyBlock: StoryBlockViewModel,
    storyBeatRow: StoryBeatRowViewModel,
    direction: -1 | 1,
  ): boolean {
    if (this.isReorderingStoryBeats()) {
      return false;
    }

    const storyBeatRows = this.storyBeatRowsFor(storyBlock);
    const currentIndex = storyBeatRows.findIndex((row) => row.key === storyBeatRow.key);
    const targetIndex = currentIndex + direction;

    if (currentIndex < 0 || targetIndex < 0 || targetIndex >= storyBeatRows.length) {
      return false;
    }

    if (direction === -1 && storyBeatRow.beats.some((beat) => this.isTransitionStoryBeat(beat))) {
      return false;
    }

    if (direction === 1 && storyBeatRows[targetIndex]?.beats.some((beat) => this.isTransitionStoryBeat(beat))) {
      return false;
    }

    return true;
  }

  protected moveStoryBeatRow(
    storyBlock: StoryBlockViewModel,
    storyBeatRow: StoryBeatRowViewModel,
    direction: -1 | 1,
  ): void {
    if (!this.canMoveStoryBeatRow(storyBlock, storyBeatRow, direction)) {
      return;
    }

    const storyBeatRows = this.storyBeatRowsFor(storyBlock);
    const currentIndex = storyBeatRows.findIndex((row) => row.key === storyBeatRow.key);
    const targetIndex = currentIndex + direction;
    const nextStoryBeatRows = [...storyBeatRows];

    [nextStoryBeatRows[currentIndex], nextStoryBeatRows[targetIndex]] = [
      nextStoryBeatRows[targetIndex]!,
      nextStoryBeatRows[currentIndex]!,
    ];

    this.commitStoryBeatOrder(storyBlock, nextStoryBeatRows);
  }

  protected storyBeatSequenceLabel(
    storyBeatRow: StoryBeatRowViewModel,
    storyBeat: StoryBeatViewModel,
  ): string {
    if (storyBeatRow.beats.length <= 1) {
      return storyBeatRow.orderIndex.toString();
    }

    return `${storyBeatRow.orderIndex}.${storyBeat.secondaryOrderIndex}`;
  }

  protected storyBeatRowPagerLabel(storyBeatRow: StoryBeatRowViewModel): string {
    return `${storyBeatRow.activeIndex + 1}/${storyBeatRow.beats.length}`;
  }

  protected storyBeatIndexPathRuleFor(storyBeatRow: StoryBeatRowViewModel): StoryBeatIndexPathRuleModel | null {
    return storyBeatRow.beats.find((beat) => beat.indexPathRule)?.indexPathRule ?? null;
  }

  protected storyBeatIndexPathRuleLabel(storyBeatRow: StoryBeatRowViewModel): string {
    const rule = this.storyBeatIndexPathRuleFor(storyBeatRow);

    return rule
      ? this.storyBeatIndexPathRelationLabel(rule.relationType)
      : 'Path';
  }

  protected storyBeatIndexPathRelationLabel(
    relationType: StoryBeatIndexPathRuleRelationTypeValue | null | undefined,
  ): string {
    const normalizedRelationType = this.toStoryBeatIndexPathRuleRelationType(relationType);

    switch (normalizedRelationType) {
      case StoryBeatIndexPathRuleRelationType.And:
        return 'AND';
      case StoryBeatIndexPathRuleRelationType.Or:
        return 'OR';
      case StoryBeatIndexPathRuleRelationType.ExactlyOne:
        return 'Exactly One';
      default:
        return 'Path';
    }
  }

  protected storyBeatIndexPathRuleDescription(storyBeatRow: StoryBeatRowViewModel): string {
    const rule = this.storyBeatIndexPathRuleFor(storyBeatRow);

    if (!rule) {
      return 'No index path rule configured.';
    }

    return [
      `${this.storyBeatIndexPathRelationLabel(rule.relationType)} relation`,
      rule.isRequired ? 'Required path' : 'Optional path',
    ].join(' - ');
  }

  protected storyBeatPreviousAlternative(storyBeatRow: StoryBeatRowViewModel): StoryBeatViewModel | null {
    return storyBeatRow.beats[storyBeatRow.activeIndex - 1] ?? null;
  }

  protected storyBeatNextAlternative(storyBeatRow: StoryBeatRowViewModel): StoryBeatViewModel | null {
    return storyBeatRow.beats[storyBeatRow.activeIndex + 1] ?? null;
  }

  protected showPreviousStoryBeatAlternative(storyBeatRow: StoryBeatRowViewModel): void {
    this.setStoryBeatAlternativeIndex(storyBeatRow, storyBeatRow.activeIndex - 1);
  }

  protected showNextStoryBeatAlternative(storyBeatRow: StoryBeatRowViewModel): void {
    this.setStoryBeatAlternativeIndex(storyBeatRow, storyBeatRow.activeIndex + 1);
  }

  protected storyBeatTitle(storyBeat: StoryBeatViewModel): string {
    return storyBeat.title || `Story Beat ${storyBeat.displayIndex}`;
  }

  protected storyBeatNarrative(storyBeat: StoryBeatViewModel): string {
    if (this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Narrative) {
      return storyBeat.narrative?.paragraphs.join('\n\n') || 'No narrative yet.';
    }

    if (this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Milestone) {
      return storyBeat.milestone?.description || 'No milestone description.';
    }

    if (this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Roleplaying) {
      return storyBeat.roleplaying?.mainDescription || 'No roleplaying text yet.';
    }

    if (this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Decision) {
      return storyBeat.decision?.description || 'No decision description yet.';
    }

    if (this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Combat) {
      return storyBeat.combat?.description || 'No combat description yet.';
    }

    if (this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Transition) {
      return storyBeat.transition?.description || 'No transition description yet.';
    }

    return storyBeat.information?.narrative || 'No information yet.';
  }

  protected storyBeatTypeLabel(storyBeat: StoryBeatViewModel): string {
    const storyBeatType = this.toStoryBeatType(storyBeat.storyBeatType);

    return StoryBeatType[storyBeatType] ?? 'Story Beat';
  }

  protected isInformationStoryBeat(storyBeat: StoryBeatViewModel): boolean {
    return this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Information;
  }

  protected isNarrativeStoryBeat(storyBeat: StoryBeatViewModel): boolean {
    return this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Narrative;
  }

  protected isRoleplayingStoryBeat(storyBeat: StoryBeatViewModel): boolean {
    return this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Roleplaying;
  }

  protected isDecisionStoryBeat(storyBeat: StoryBeatViewModel): boolean {
    return this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Decision;
  }

  protected isCombatStoryBeat(storyBeat: StoryBeatViewModel): boolean {
    return this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Combat;
  }

  protected isTransitionStoryBeat(storyBeat: StoryBeatViewModel): boolean {
    return this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Transition;
  }

  protected hasTransitionStoryBeat(storyBlock: StoryBlockViewModel): boolean {
    return storyBlock.beats.some((storyBeat) => this.isTransitionStoryBeat(storyBeat));
  }

  protected isMilestoneStoryBeat(storyBeat: StoryBeatViewModel): boolean {
    return this.toStoryBeatType(storyBeat.storyBeatType) === StoryBeatType.Milestone;
  }

  protected milestoneImportanceLabel(milestone: CampaignMilestoneModel): string {
    return getCampaignMilestoneImportanceLabel(milestone.importance);
  }

  protected milestoneOptionLabel(milestone: CampaignMilestoneModel): string {
    return `${milestone.title} - ${this.milestoneImportanceLabel(milestone)}`;
  }

  protected campaignMilestoneImportanceClass(milestone: CampaignMilestoneModel): string {
    return `campaign-milestone-importance-${getCampaignMilestoneImportanceSlug(milestone.importance)}`;
  }

  protected storyBeatMilestoneImportanceClass(storyBeat: StoryBeatViewModel): string {
    if (!storyBeat.milestone) {
      return '';
    }

    return `story-beat-card-milestone-${getCampaignMilestoneImportanceSlug(storyBeat.milestone.importance)}`;
  }

  protected storyBeatTypeClass(storyBeat: StoryBeatViewModel): string {
    const storyBeatType = this.toStoryBeatType(storyBeat.storyBeatType);

    switch (storyBeatType) {
      case StoryBeatType.Information:
        return 'story-beat-card-information';
      case StoryBeatType.Narrative:
        return 'story-beat-card-narrative';
      case StoryBeatType.Roleplaying:
        return 'story-beat-card-roleplaying';
      case StoryBeatType.Decision:
        return 'story-beat-card-decision';
      case StoryBeatType.Combat:
        return 'story-beat-card-combat';
      case StoryBeatType.Transition:
        return 'story-beat-card-transition';
      case StoryBeatType.Milestone:
        return 'story-beat-card-milestone';
      default:
        return 'story-beat-card-information';
    }
  }

  protected storyBeatPreviewParts(storyBeat: StoryBeatViewModel): StoryBeatNarrativePart[] {
    return this.toNarrativePreviewParts(this.storyBeatNarrative(storyBeat), storyBeat.storyBeatId);
  }

  protected isExpandableStoryBeatSkillToken(part: StoryBeatNarrativePart): boolean {
    return Boolean(part.tokenKey && part.compactText && part.detailText);
  }

  protected isStoryBeatSkillTokenExpanded(part: StoryBeatNarrativePart): boolean {
    return part.tokenKey ? this.expandedStoryBeatSkillTokenKeys()[part.tokenKey] ?? false : false;
  }

  protected storyBeatSkillTokenText(part: StoryBeatNarrativePart): string {
    return this.isStoryBeatSkillTokenExpanded(part)
      ? part.detailText ?? part.text
      : part.compactText ?? part.text;
  }

  protected storyBeatSkillTokenTooltip(part: StoryBeatNarrativePart): string {
    return part.detailText ?? part.text;
  }

  protected toggleStoryBeatSkillToken(part: StoryBeatNarrativePart): void {
    const tokenKey = part.tokenKey;

    if (!tokenKey) {
      return;
    }

    this.expandedStoryBeatSkillTokenKeys.update((keys) => ({
      ...keys,
      [tokenKey]: !(keys[tokenKey] ?? false),
    }));
  }

  protected storyBeatRoleplayingPreviewPartsFor(
    storyBeat: StoryBeatViewModel,
  ): StoryBeatNarrativePart[] {
    return this.toRoleplayingPreviewParts(storyBeat.roleplaying?.mainDescription ?? '');
  }

  protected roleplayingStoryBeatInformation(
    storyBeat: StoryBeatViewModel,
  ): StoryBeatRoleplayingInformationModel[] {
    return storyBeat.roleplaying?.discoverableInformation ?? [];
  }

  protected roleplayingOutcomeEffectTargets(storyBeat: StoryBeatViewModel): StoryBeatOutcomeEffectTarget[] {
    const roleplaying = storyBeat.roleplaying;

    if (!roleplaying) {
      return [];
    }

    const npcReferenceTargets = (roleplaying.npcReferences ?? [])
      .map((reference) => {
        const sourceId = this.normalizeText(reference.id);
        const npcTag = this.normalizeText(reference.npcTag || reference.tag);

        if (!sourceId || !npcTag) {
          return null;
        }

        const title = this.getRoleplayingNpcDisplayNameByTag(npcTag, npcTag) || npcTag;

        return {
          sourceType: OutcomeSourceType.RoleplayingNpcInteraction,
          sourceId,
          key: `${OutcomeSourceType.RoleplayingNpcInteraction}:${sourceId}`,
          title,
          description: `NPC interaction: ${npcTag}`,
          category: 'NPC Interaction',
        };
      })
      .filter((target): target is StoryBeatOutcomeEffectTarget => target !== null);

    const informationTargets = this.roleplayingStoryBeatInformation(storyBeat)
      .map((information) => {
        const sourceId = this.normalizeText(information.id);

        if (!sourceId) {
          return null;
        }

        return {
          sourceType: OutcomeSourceType.RoleplayingInformation,
          sourceId,
          key: `${OutcomeSourceType.RoleplayingInformation}:${sourceId}`,
          title: this.roleplayingInformationNpcLabel(storyBeat, information),
          description: `${this.roleplayingInformationCheckLabel(information)} - ${information.information}`,
          category: 'Discoverable Information',
        };
      })
      .filter((target): target is StoryBeatOutcomeEffectTarget => target !== null);

    return [
      ...npcReferenceTargets,
      ...informationTargets,
    ];
  }

  protected decisionOutcomeEffectTargets(storyBeat: StoryBeatViewModel): StoryBeatOutcomeEffectTarget[] {
    return (storyBeat.decision?.decisions ?? [])
      .map((choice) => {
        const sourceId = this.normalizeText(choice.id);
        const title = this.normalizeText(choice.title);

        if (!sourceId || !title) {
          return null;
        }

        return {
          sourceType: OutcomeSourceType.DecisionChoice,
          sourceId,
          key: `${OutcomeSourceType.DecisionChoice}:${sourceId}`,
          title,
          description: this.normalizeText(choice.description) || 'No description.',
          category: 'Decision Choice',
        };
      })
      .filter((target): target is StoryBeatOutcomeEffectTarget => target !== null);
  }

  protected storyBeatOutcomeEffectTargets(storyBeat: StoryBeatViewModel): StoryBeatOutcomeEffectTarget[] {
    if (this.isRoleplayingStoryBeat(storyBeat)) {
      return this.roleplayingOutcomeEffectTargets(storyBeat);
    }

    if (this.isDecisionStoryBeat(storyBeat)) {
      return this.decisionOutcomeEffectTargets(storyBeat);
    }

    return [];
  }

  protected isSelectedStoryBeatEventSource(target: StoryBeatOutcomeEffectTarget): boolean {
    const selectedSource = this.selectedRoleplayingEventSource();

    return selectedSource?.sourceType === target.sourceType &&
      selectedSource.sourceId === target.sourceId;
  }

  protected storyBeatNarrativeParagraphs(storyBeat: StoryBeatViewModel): string[] {
    return storyBeat.narrative?.paragraphs ?? [];
  }

  protected storyBeatDecisionChoices(storyBeat: StoryBeatViewModel): StoryBeatDecisionChoiceDraft[] {
    return (storyBeat.decision?.decisions ?? []).map((choice) => ({
      draftId: choice.orderIndex,
      id: choice.id ?? null,
      title: choice.title,
      description: choice.description,
    }));
  }

  protected combatEnemyLabel(enemy: { monsterId: number; amount: number }): string {
    const monster = this.combatNpcOptions().find((option) => option.id === enemy.monsterId);
    const name = monster ? this.combatNpcOptionLabel(monster) : `Monster ${enemy.monsterId}`;

    return `${enemy.amount} x ${name}`;
  }

  protected combatRewardLines(storyBeat: StoryBeatViewModel): string[] {
    return this.toRewardLines(storyBeat.combat?.rewards ?? null);
  }

  protected transitionConclusionRows(storyBeat: StoryBeatViewModel): {
    key: string;
    category: string;
    text: string;
    sourceTitle: string;
  }[] {
    return (storyBeat.transition?.conclusions ?? []).map((conclusion, index) => ({
      key: `${conclusion.sourceStoryBeatId}-${conclusion.category}-${index}`,
      category: conclusion.category,
      text: conclusion.text,
      sourceTitle: conclusion.sourceTitle,
    }));
  }

  protected getSkillLabel(skill: Skill): string {
    return this.skillOptions.find((option) => option.skill === skill)?.label ?? 'Perception';
  }

  protected getAbilityLabel(ability: Ability): string {
    return this.abilityOptions.find((option) => option.ability === ability)?.label ?? 'Charisma';
  }

  protected getSkillClass(skill: Skill): string {
    return this.skillOptions.find((option) => option.skill === skill)?.className ?? 'skill-perception';
  }

  protected getDifficultyIntensityClass(difficultyClass: number): string {
    if (difficultyClass <= 12) {
      return 'dc-low';
    }

    if (difficultyClass <= 17) {
      return 'dc-medium';
    }

    if (difficultyClass <= 25) {
      return 'dc-high';
    }

    return 'dc-legendary';
  }

  protected getSkillTokenClass(skill: Skill, difficultyClass: number): string {
    return `${this.getSkillClass(skill)} ${this.getDifficultyIntensityClass(difficultyClass)}`;
  }

  protected appendedStoryBeatInformation(
    storyBeat: StoryBeatViewModel,
  ): StoryBeatOptionalInformationModel[] {
    return storyBeat.information?.optionalInformation.filter((information) => (
      this.toOptionalInformationPlacement(information.placement) ===
        StoryBeatOptionalInformationPlacement.Appended
    )) ?? [];
  }

  protected optionalInformationSkillLabel(
    information: StoryBeatOptionalInformationModel,
  ): string {
    return this.getSkillLabel(this.toSkill(information.skill) ?? Skill.Perception);
  }

  protected optionalInformationTokenClass(
    information: StoryBeatOptionalInformationModel,
  ): string {
    return this.getSkillTokenClass(
      this.toSkill(information.skill) ?? Skill.Perception,
      information.difficultyClass,
    );
  }

  protected isRoleplayingSkillCheck(draft: StoryBeatRoleplayingInformationDraft): boolean {
    return draft.checkType === StoryBeatRoleplayingCheckType.Skill;
  }

  protected isRoleplayingAbilityCheck(draft: StoryBeatRoleplayingInformationDraft): boolean {
    return draft.checkType === StoryBeatRoleplayingCheckType.Ability;
  }

  protected roleplayingInformationCheckLabel(
    information: StoryBeatRoleplayingInformationModel,
  ): string {
    const checkType = this.toRoleplayingCheckType(information.checkType);

    if (checkType === StoryBeatRoleplayingCheckType.None) {
      return 'No check';
    }

    if (checkType === StoryBeatRoleplayingCheckType.Ability) {
      return `${this.getAbilityLabel(this.toAbility(information.ability) ?? Ability.CHARISMA)}-${information.difficultyClass ?? 10}`;
    }

    return `${this.getSkillLabel(this.toSkill(information.skill) ?? Skill.Perception)}-${information.difficultyClass ?? 10}`;
  }

  protected roleplayingInformationNpcLabel(
    storyBeat: StoryBeatViewModel,
    information: StoryBeatRoleplayingInformationModel,
  ): string {
    const campaignNpcDisplayName = this.getRoleplayingNpcDisplayNameByTag(
      information.npcTag,
      '',
    );

    if (campaignNpcDisplayName) {
      return campaignNpcDisplayName;
    }

    const legacyNpcName = storyBeat.roleplaying?.npcs?.find((npc) => (
      npc.tag === information.npcTag ||
      (!information.npcTag && npc.name === information.npcName)
    ))?.name;

    const label = legacyNpcName ??
      this.toRoleplayingNpcOptions(storyBeat.roleplaying?.mainDescription ?? '')
        .find((npc) => npc.key === information.npcTag)?.name ??
      information.npcName ??
      information.npcTag;

    return label || 'NPC';
  }

  protected roleplayingNpcName(row: CampaignRoleplayingNpcTableRow): string {
    return row.name || row.tag || 'Unnamed NPC';
  }

  protected roleplayingNpcDisplayName(row: CampaignRoleplayingNpcTableRow): string {
    return row.displayName || row.name || row.tag || 'Unnamed NPC';
  }

  protected roleplayingNpcNameDraft(row: CampaignRoleplayingNpcTableRow): string {
    return this.roleplayingNpcNameDrafts()[row.key] ?? row.displayName;
  }

  protected isEditingRoleplayingNpc(row: CampaignRoleplayingNpcTableRow): boolean {
    return this.editingRoleplayingNpcKey() === row.key;
  }

  protected isRoleplayingNpcNameDirty(row: CampaignRoleplayingNpcTableRow): boolean {
    return this.normalizeText(this.roleplayingNpcNameDraft(row)) !==
      this.normalizeText(row.displayName);
  }

  protected canSaveRoleplayingNpc(row: CampaignRoleplayingNpcTableRow): boolean {
    return (
      this.isEditingRoleplayingNpc(row) &&
      this.isRoleplayingNpcNameDirty(row) &&
      this.normalizeText(this.roleplayingNpcNameDraft(row)).length > 0 &&
      this.savingRoleplayingNpcTag() === null
    );
  }

  protected editRoleplayingNpcName(row: CampaignRoleplayingNpcTableRow): void {
    this.editingRoleplayingNpcKey.set(row.key);
    this.roleplayingNpcNameDrafts.update((drafts) => ({
      ...drafts,
      [row.key]: row.displayName,
    }));
  }

  protected setRoleplayingNpcNameDraft(row: CampaignRoleplayingNpcTableRow, event: Event): void {
    const value = (event.target as HTMLInputElement).value;

    this.roleplayingNpcNameDrafts.update((drafts) => ({
      ...drafts,
      [row.key]: value,
    }));
  }

  protected discardRoleplayingNpcName(row: CampaignRoleplayingNpcTableRow): void {
    this.roleplayingNpcNameDrafts.update((drafts) => ({
      ...drafts,
      [row.key]: row.displayName,
    }));
    this.editingRoleplayingNpcKey.set(null);
  }

  protected saveRoleplayingNpcName(row: CampaignRoleplayingNpcTableRow): void {
    const campaignId = this.getCampaignId();
    const displayName = this.normalizeText(this.roleplayingNpcNameDraft(row));

    if (!campaignId || !this.canSaveRoleplayingNpc(row) || !displayName) {
      return;
    }

    this.savingRoleplayingNpcTag.set(row.tag);

    this.campaignApiService
      .updateRoleplayingNpc(campaignId, row.tag, {
        name: row.name,
        displayName,
        description: row.description,
      })
      .pipe(finalize(() => this.savingRoleplayingNpcTag.set(null)))
      .subscribe({
        next: (response) => {
          const updatedNpc = response.data;

          if (updatedNpc) {
            const normalizedTag = row.tag.toLowerCase();

            this.roleplayingNpcs.update((npcs) => npcs.map((npc) => (
              npc.tag.toLowerCase() === normalizedTag ? updatedNpc : npc
            )));

            this.roleplayingNpcNameDrafts.update((drafts) => ({
              ...drafts,
              [row.key]: this.toCampaignNpcDisplayName(updatedNpc),
            }));
            this.editingRoleplayingNpcKey.set(null);
          }

          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Roleplaying NPC could not be updated.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected roleplayingNpcDescription(row: CampaignRoleplayingNpcTableRow): string {
    return row.description || 'No description.';
  }

  protected roleplayingNpcCreatedAt(row: CampaignRoleplayingNpcTableRow): string {
    return this.toDisplayDate(row.createdAt);
  }

  protected roleplayingNpcUpdatedAt(row: CampaignRoleplayingNpcTableRow): string {
    return this.toDisplayDate(row.updatedAt);
  }

  protected previousQuestCard(viewport?: HTMLElement): void {
    const itemCount = this.questCarouselItems().length;

    if (itemCount < 2) {
      return;
    }

    this.questCarouselIndex.update((index) => (index - 1 + itemCount) % itemCount);
    this.scrollQuestCards(viewport, -1);
  }

  protected nextQuestCard(viewport?: HTMLElement): void {
    const itemCount = this.questCarouselItems().length;

    if (itemCount < 2) {
      return;
    }

    this.questCarouselIndex.update((index) => (index + 1) % itemCount);
    this.scrollQuestCards(viewport, 1);
  }

  protected isAddQuestItem(item: QuestCarouselItem): item is 'add-quest' {
    return item === 'add-quest';
  }

  private storyBeatQuestSearchText(quest: CampaignQuestModel): string {
    return `${quest.title} ${quest.description} ${quest.givenBy} ${quest.reward} ${this.questTypeLabel(quest)}`.toLowerCase();
  }

  private storyBeatQuestTaskSearchText(task: CampaignQuestTaskModel): string {
    return `${task.title} ${task.description}`.toLowerCase();
  }

  protected getQuestTypeColor(quest: CampaignQuestModel): string {
    switch (this.toQuestType(quest.type)) {
      case CampaignQuestType.MainQuest:
        return '#dc2626';
      case CampaignQuestType.SideQuest:
        return '#16a34a';
      case CampaignQuestType.PersonalQuest:
        return '#eab308';
      case CampaignQuestType.CollectibleHunt:
        return '#9333ea';
      default:
        return 'transparent';
    }
  }

  private scrollQuestCards(viewport: HTMLElement | undefined, direction: -1 | 1): void {
    viewport?.scrollBy({
      left: direction * viewport.clientWidth * 0.75,
      behavior: 'smooth',
    });
  }

  protected openCreateQuestDialog(): void {
    this.editingQuest.set(null);
    this.questTypeDraft.set(CampaignQuestType.MainQuest);
    this.questTitleDraft.set('');
    this.questDescriptionDraft.set('');
    this.questGivenByDraft.set('');
    this.questRewardDraft.set('');
    this.questTaskDrafts.set([this.createEmptyQuestTaskDraft()]);
    this.questFormStep.set('details');
    this.isCreateQuestDialogOpen.set(true);
  }

  protected closeCreateQuestDialog(): void {
    if (this.isCreatingQuest() || this.isDeletingQuest()) {
      return;
    }

    this.isCreateQuestDialogOpen.set(false);
    this.editingQuest.set(null);
  }

  protected openEditQuestDialog(quest: CampaignQuestModel): void {
    this.editingQuest.set(quest);
    this.questTypeDraft.set(this.toQuestType(quest.type));
    this.questTitleDraft.set(quest.title);
    this.questDescriptionDraft.set(quest.description);
    this.questGivenByDraft.set(quest.givenBy);
    this.questRewardDraft.set(quest.reward);
    this.questTaskDrafts.set(
      quest.tasks.length > 0
        ? quest.tasks.map((task) => ({
            draftId: this.nextQuestTaskDraftId++,
            title: task.title,
            description: task.description,
            dateCompleted: task.dateCompleted,
          }))
        : [this.createEmptyQuestTaskDraft()],
    );
    this.questFormStep.set('details');
    this.isCreateQuestDialogOpen.set(true);
  }

  protected showQuestDetailsStep(): void {
    if (this.isCreatingQuest()) {
      return;
    }

    this.questFormStep.set('details');
  }

  protected showQuestTasksStep(): void {
    if (!this.canContinueQuestDetails()) {
      return;
    }

    this.questFormStep.set('tasks');
  }

  protected setQuestTypeDraft(event: Event): void {
    this.questTypeDraft.set(Number((event.target as HTMLSelectElement).value));
  }

  protected setQuestTitleDraft(event: Event): void {
    this.questTitleDraft.set((event.target as HTMLInputElement).value);
  }

  protected setQuestDescriptionDraft(event: Event): void {
    this.questDescriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected setQuestGivenByDraft(event: Event): void {
    this.questGivenByDraft.set((event.target as HTMLInputElement).value);
  }

  protected setQuestRewardDraft(event: Event): void {
    this.questRewardDraft.set((event.target as HTMLInputElement).value);
  }

  protected setQuestTaskTitleDraft(taskId: number, event: Event): void {
    const title = (event.target as HTMLInputElement).value;

    this.questTaskDrafts.update((tasks) => tasks.map((task) => (
      task.draftId === taskId ? { ...task, title } : task
    )));
  }

  protected setQuestTaskDescriptionDraft(taskId: number, event: Event): void {
    const description = (event.target as HTMLTextAreaElement).value;

    this.questTaskDrafts.update((tasks) => tasks.map((task) => (
      task.draftId === taskId ? { ...task, description } : task
    )));
  }

  protected addQuestTaskDraft(): void {
    this.questTaskDrafts.update((tasks) => [
      ...tasks,
      this.createEmptyQuestTaskDraft(),
    ]);
  }

  protected removeQuestTaskDraft(taskId: number): void {
    this.questTaskDrafts.update((tasks) => {
      const nextTasks = tasks.filter((task) => task.draftId !== taskId);

      return nextTasks.length > 0 ? nextTasks : [this.createEmptyQuestTaskDraft()];
    });
  }

  protected saveQuest(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || !this.canCreateQuest()) {
      return;
    }

    const editingQuest = this.editingQuest();
    const request: UpdateCampaignQuestRequest = {
      type: this.questTypeDraft(),
      title: this.normalizeText(this.questTitleDraft()),
      description: this.normalizeText(this.questDescriptionDraft()),
      givenBy: this.normalizeText(this.questGivenByDraft()),
      reward: this.normalizeText(this.questRewardDraft()),
      completedAt: editingQuest?.completedAt ?? null,
      tasks: this.questTaskDrafts()
        .map((task) => ({
          title: this.normalizeText(task.title),
          description: this.normalizeText(task.description),
          dateCompleted: task.dateCompleted,
        }))
        .filter((task) => task.title.length > 0 && task.description.length > 0),
    };

    this.isCreatingQuest.set(true);

    const saveQuest = editingQuest
      ? this.campaignApiService.updateCampaignQuest(campaignId, editingQuest.questId, request)
      : this.campaignApiService.createCampaignQuest(campaignId, request);

    saveQuest
      .pipe(finalize(() => this.isCreatingQuest.set(false)))
      .subscribe({
        next: (response) => {
          this.isCreateQuestDialogOpen.set(false);
          this.editingQuest.set(null);
          this.loadQuests(response.data?.questId ?? editingQuest?.questId ?? null);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign quest could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected deleteQuest(): void {
    const campaignId = this.getCampaignId();
    const quest = this.editingQuest();

    if (!campaignId || !quest || this.isCreatingQuest() || this.isDeletingQuest()) {
      return;
    }

    this.isDeletingQuest.set(true);

    this.campaignApiService
      .deleteCampaignQuest(campaignId, quest.questId)
      .pipe(finalize(() => this.isDeletingQuest.set(false)))
      .subscribe({
        next: (response) => {
          this.modalHelper.showSuccess(response.message);
          this.isCreateQuestDialogOpen.set(false);
          this.editingQuest.set(null);
          this.loadQuests();
        },
        error: (error: unknown) => {
          const blockers = this.getQuestDeleteBlockers(error);

          this.modalHelper.showError(
            this.getQuestDeleteErrorMessages(error, blockers),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected questDeleteBlockerMessage(blocker: CampaignQuestDeleteBlockerModel): string {
    return this.normalizeText(blocker.message) ||
      `Task "${blocker.questTaskTitle}" is linked to campaign story content.`;
  }

  protected questDeleteBlockerLocation(blocker: CampaignQuestDeleteBlockerModel): string {
    const storyBlock = [
      blocker.storyBlockOrderIndex !== null ? `Block ${blocker.storyBlockOrderIndex}` : '',
      blocker.storyBlockTitle ?? '',
    ].map((value) => this.normalizeText(String(value))).filter((value) => value.length > 0).join(' - ');
    const beatIndex = [
      blocker.storyBeatOrderIndex,
      blocker.storyBeatSecondaryOrderIndex,
    ].filter((value): value is number => value !== null && value !== undefined).join('.');
    const storyBeat = [
      beatIndex ? `Beat ${beatIndex}` : '',
      blocker.storyBeatTitle ?? '',
    ].map((value) => this.normalizeText(String(value))).filter((value) => value.length > 0).join(' - ');

    return [storyBlock, storyBeat]
      .filter((value) => value.length > 0)
      .join(' / ') || 'Linked campaign content';
  }

  protected questDeleteBlockerStoryBeatIndex(blocker: CampaignQuestDeleteBlockerModel): string {
    const indexes = [
      blocker.storyBeatOrderIndex,
      blocker.storyBeatSecondaryOrderIndex,
    ].filter((value): value is number => value !== null && value !== undefined);

    return indexes.length > 0 ? indexes.join('.') : 'Unknown';
  }

  protected openCreateMilestoneDialog(): void {
    this.editingMilestone.set(null);
    this.milestoneTitleDraft.set('');
    this.milestoneDescriptionDraft.set('');
    this.milestoneImportanceDraft.set(CampaignMilestoneImportance.Low);
    this.isCreateMilestoneDialogOpen.set(true);
  }

  protected closeCreateMilestoneDialog(): void {
    if (this.isCreatingMilestone()) {
      return;
    }

    this.isCreateMilestoneDialogOpen.set(false);
  }

  protected openEditMilestoneDialog(milestone: CampaignMilestoneModel): void {
    this.editingMilestone.set(milestone);
    this.milestoneTitleDraft.set(milestone.title);
    this.milestoneDescriptionDraft.set(milestone.description ?? '');
    this.milestoneImportanceDraft.set(this.toMilestoneImportance(milestone.importance));
    this.isCreateMilestoneDialogOpen.set(true);
  }

  protected confirmDeleteMilestone(milestone: CampaignMilestoneModel): void {
    this.deleteConfirmationMilestone.set(milestone);
  }

  protected cancelDeleteMilestone(): void {
    if (this.isDeletingMilestone()) {
      return;
    }

    this.deleteConfirmationMilestone.set(null);
  }

  protected setMilestoneTitleDraft(event: Event): void {
    this.milestoneTitleDraft.set((event.target as HTMLInputElement).value);
  }

  protected setMilestoneDescriptionDraft(event: Event): void {
    this.milestoneDescriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected setMilestoneImportanceDraft(event: Event): void {
    this.milestoneImportanceDraft.set(Number((event.target as HTMLSelectElement).value));
  }

  protected createMilestone(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || !this.canCreateMilestone()) {
      return;
    }

    const request: CampaignMilestoneRequest = {
      title: this.normalizeText(this.milestoneTitleDraft()),
      description: this.toNullableText(this.milestoneDescriptionDraft()),
      achievedAt: null,
      importance: this.milestoneImportanceDraft(),
    };

    this.isCreatingMilestone.set(true);

    const editingMilestone = this.editingMilestone();
    const saveMilestone = editingMilestone
      ? this.campaignApiService.updateCampaignMilestone(campaignId, editingMilestone.id, request)
      : this.campaignApiService.createCampaignMilestone(campaignId, request);

    saveMilestone
      .pipe(finalize(() => this.isCreatingMilestone.set(false)))
      .subscribe({
        next: () => {
          this.isCreateMilestoneDialogOpen.set(false);
          this.editingMilestone.set(null);
          this.loadMilestones();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign milestone could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected deleteMilestone(): void {
    const campaignId = this.getCampaignId();
    const milestone = this.deleteConfirmationMilestone();

    if (!campaignId || !milestone || this.isDeletingMilestone()) {
      return;
    }

    this.isDeletingMilestone.set(true);

    this.campaignApiService
      .deleteCampaignMilestone(campaignId, milestone.id)
      .pipe(finalize(() => this.isDeletingMilestone.set(false)))
      .subscribe({
        next: () => {
          this.deleteConfirmationMilestone.set(null);
          this.loadMilestones();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign milestone could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadMilestones(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingMilestones()) {
      return;
    }

    this.isLoadingMilestones.set(true);

    this.campaignApiService
      .fetchCampaignMilestones(campaignId)
      .pipe(finalize(() => this.isLoadingMilestones.set(false)))
      .subscribe({
        next: (response) => {
          this.milestones.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign milestones could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadStoryContent(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingStoryContent()) {
      return;
    }

    this.isLoadingStoryContent.set(true);

    this.campaignApiService
      .fetchStoryBlocks(campaignId)
      .pipe(
        switchMap((response) => {
          const storyBlocks = response.data ?? [];
          const milestoneRequest = this.campaignApiService.fetchCampaignMilestones(campaignId);
          const roleplayingNpcsRequest = this.campaignApiService
            .fetchRoleplayingStoryBeatNpcs(campaignId)
            .pipe(
              map((npcsResponse) => npcsResponse.data ?? []),
              catchError(() => of([] as CampaignNpcModel[])),
            );

          const storyBlockRequest = storyBlocks.length === 0
            ? of([] as StoryBlockViewModel[])
            : forkJoin(storyBlocks.map((storyBlock, index) => (
              this.campaignApiService
                .fetchStoryBeats(campaignId, storyBlock.storyBlockId)
                .pipe(map((beatsResponse) => ({
                  ...storyBlock,
                  displayIndex: index + 1,
                  beats: this.toStoryBeatViewModels(beatsResponse.data ?? []),
                })))
            )));

          return forkJoin({
            storyBlocks: storyBlockRequest,
            milestones: milestoneRequest,
            roleplayingNpcs: roleplayingNpcsRequest,
          });
        }),
        finalize(() => this.isLoadingStoryContent.set(false)),
      )
      .subscribe({
        next: ({ storyBlocks, milestones, roleplayingNpcs }) => {
          this.storyBlocks.set(storyBlocks);
          this.milestones.set(milestones.data ?? []);
          this.roleplayingNpcs.set(roleplayingNpcs);
          this.loadStoryBeatRuleSummaries(storyBlocks);
          this.loadStoryBeatEventEffectSummaries(storyBlocks);

          if (
            this.selectedStoryBlockId() &&
            !storyBlocks.some((storyBlock) => (
              storyBlock.storyBlockId === this.selectedStoryBlockId()
            ))
          ) {
            this.selectedStoryBlockId.set(null);
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story content could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadQuests(focusQuestId: string | null = null): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingQuests()) {
      return;
    }

    this.isLoadingQuests.set(true);

    this.campaignApiService
      .fetchCampaignQuests(campaignId)
      .pipe(finalize(() => this.isLoadingQuests.set(false)))
      .subscribe({
        next: (response) => {
          const quests = response.data ?? [];

          this.quests.set(quests);

          if (focusQuestId) {
            const questIndex = quests.findIndex((quest) => quest.questId === focusQuestId);
            this.questCarouselIndex.set(questIndex >= 0 ? questIndex : 0);
            return;
          }

          if (this.questCarouselIndex() >= quests.length + 1) {
            this.questCarouselIndex.set(0);
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign quests could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadRoleplayingNpcs(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingRoleplayingNpcs()) {
      return;
    }

    this.isLoadingRoleplayingNpcs.set(true);
    this.roleplayingNpcsLoadError.set('');

    this.campaignApiService
      .fetchRoleplayingStoryBeatNpcs(campaignId)
      .pipe(
        timeout(15000),
        finalize(() => this.isLoadingRoleplayingNpcs.set(false)),
      )
      .subscribe({
        next: (response) => {
          this.roleplayingNpcs.set(response.data ?? []);
        },
        error: (error: unknown) => {
          const message = this.getErrorMessage(error, 'Roleplaying NPCs could not be loaded.');

          this.roleplayingNpcsLoadError.set(message);
          this.modalHelper.showError(
            message,
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private updateStoryBlockMusicDraft(
    draftId: number,
    changes: Partial<Omit<StoryBlockMusicDraft, 'draftId' | 'musicFileId' | 'orderIndex'>>,
  ): void {
    this.storyBlockMusicDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId
        ? {
          ...draft,
          ...changes,
        }
        : draft
    )));
  }

  private applyStoryBlockMusic(
    storyBlockId: string,
    musicFiles: StoryBlockMusicFileModel[],
  ): void {
    this.storyBlocks.update((storyBlocks) => storyBlocks.map((storyBlock) => {
      if (storyBlock.storyBlockId !== storyBlockId) {
        return storyBlock;
      }

      return {
        ...storyBlock,
        musicFiles,
        beats: storyBlock.beats.map((beat) => ({
          ...beat,
          musicFiles: musicFiles.filter((musicFile) => musicFile.storyBeatId === beat.storyBeatId),
        })),
      };
    }));
  }

  private getCampaignId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  }

  private reorderDraggedStoryBlock(
    targetStoryBlockId: string,
    dropPosition: StoryBlockDropPosition,
  ): void {
    const draggedStoryBlockId = this.draggedStoryBlockId();

    if (!draggedStoryBlockId || draggedStoryBlockId === targetStoryBlockId) {
      this.clearStoryBlockDragState();
      return;
    }

    const storyBlocks = this.storyBlocks();
    const draggedStoryBlock = storyBlocks.find((block) => block.storyBlockId === draggedStoryBlockId);

    if (!draggedStoryBlock) {
      this.clearStoryBlockDragState();
      return;
    }

    const remainingStoryBlocks = storyBlocks.filter((block) => block.storyBlockId !== draggedStoryBlockId);
    const targetIndex = remainingStoryBlocks.findIndex((block) => block.storyBlockId === targetStoryBlockId);

    if (targetIndex < 0) {
      this.clearStoryBlockDragState();
      return;
    }

    const insertIndex = dropPosition === 'before' ? targetIndex : targetIndex + 1;
    const nextStoryBlocks = [
      ...remainingStoryBlocks.slice(0, insertIndex),
      draggedStoryBlock,
      ...remainingStoryBlocks.slice(insertIndex),
    ];

    this.clearStoryBlockDragState();
    this.commitStoryBlockOrder(nextStoryBlocks, storyBlocks);
  }

  private commitStoryBlockOrder(
    nextStoryBlocks: StoryBlockViewModel[],
    previousStoryBlocks: StoryBlockViewModel[],
  ): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isReorderingStoryBlocks()) {
      return;
    }

    const reindexedStoryBlocks = this.toReindexedStoryBlocks(nextStoryBlocks);

    this.storyBlocks.set(reindexedStoryBlocks);
    this.isReorderingStoryBlocks.set(true);
    this.campaignApiService
      .reorderStoryBlocks(campaignId, {
        storyBlockIds: reindexedStoryBlocks.map((storyBlock) => storyBlock.storyBlockId),
      })
      .pipe(finalize(() => this.isReorderingStoryBlocks.set(false)))
      .subscribe({
        next: (response) => {
          const orderedStoryBlocks = response.data ?? [];

          if (orderedStoryBlocks.length === 0) {
            return;
          }

          const blocksById = new Map(
            this.storyBlocks().map((storyBlock) => [storyBlock.storyBlockId, storyBlock]),
          );

          this.storyBlocks.set(this.toReindexedStoryBlocks(
            orderedStoryBlocks.map((storyBlock) => {
              const existingStoryBlock = blocksById.get(storyBlock.storyBlockId);

              return {
                ...(existingStoryBlock ?? {}),
                ...storyBlock,
                beats: existingStoryBlock?.beats ?? [],
                displayIndex: existingStoryBlock?.displayIndex ?? storyBlock.orderIndex,
              };
            }),
          ));
        },
        error: (error: unknown) => {
          this.storyBlocks.set(previousStoryBlocks);
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story blocks could not be reordered.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private commitStoryBeatOrder(
    storyBlock: StoryBlockViewModel,
    nextStoryBeatRows: StoryBeatRowViewModel[],
  ): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isReorderingStoryBeats()) {
      return;
    }

    const previousStoryBlocks = this.storyBlocks();
    const nextStoryBeats = this.toReindexedStoryBeats(nextStoryBeatRows);

    this.storyBeatAlternativeIndexes.set({});
    this.storyBlocks.update((storyBlocks) => storyBlocks.map((block) => (
      block.storyBlockId === storyBlock.storyBlockId
        ? {
          ...block,
          beats: nextStoryBeats,
        }
        : block
    )));
    this.isReorderingStoryBeats.set(true);

    this.campaignApiService
      .reorderStoryBeats(campaignId, storyBlock.storyBlockId, {
        storyBeats: nextStoryBeats.map((storyBeat) => ({
          storyBeatId: storyBeat.storyBeatId,
          orderIndex: storyBeat.orderIndex,
          secondaryOrderIndex: storyBeat.secondaryOrderIndex,
        })),
      })
      .pipe(finalize(() => this.isReorderingStoryBeats.set(false)))
      .subscribe({
        next: (response) => {
          const reorderedStoryBeats = response.data ?? [];

          if (reorderedStoryBeats.length === 0) {
            return;
          }

          this.storyBlocks.update((storyBlocks) => storyBlocks.map((block) => (
            block.storyBlockId === storyBlock.storyBlockId
              ? {
                ...block,
                beats: this.toStoryBeatViewModels(reorderedStoryBeats),
              }
              : block
          )));
        },
        error: (error: unknown) => {
          this.storyBlocks.set(previousStoryBlocks);
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story beats could not be reordered.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private clearStoryBlockDragState(): void {
    this.draggedStoryBlockId.set(null);
    this.storyBlockDropTargetId.set(null);
    this.storyBlockDropPosition.set('after');
  }

  private toReindexedStoryBlocks(storyBlocks: StoryBlockViewModel[]): StoryBlockViewModel[] {
    return storyBlocks.map((storyBlock, index) => ({
      ...storyBlock,
      orderIndex: index + 1,
      displayIndex: index + 1,
    }));
  }

  private toReindexedStoryBeats(storyBeatRows: StoryBeatRowViewModel[]): StoryBeatViewModel[] {
    return storyBeatRows.flatMap((storyBeatRow, rowIndex) => (
      storyBeatRow.beats.map((storyBeat, secondaryIndex) => ({
        ...storyBeat,
        orderIndex: rowIndex + 1,
        secondaryOrderIndex: secondaryIndex + 1,
        displayIndex: rowIndex + 1,
      }))
    ));
  }

  private cloneRuleGroup(group: CampaignRuleGroupRequest): CampaignRuleGroupRequest {
    return {
      operator: group.operator,
      negate: group.negate ?? false,
      clauses: group.clauses.map((clause) => ({ ...clause })),
      groups: group.groups.map((childGroup) => this.cloneRuleGroup(childGroup)),
      isCollapsed: group.isCollapsed ?? false,
    };
  }

  private replaceStoryBeatOutcomeEffects(
    campaignId: string,
    source: StoryBeatOutcomeEffectSource,
  ): Observable<OutcomeEffectModel[]> {
    const deleteRequests = this.storyBeatEventEffects().map((effect) => (
      this.outcomeEffectsApiService.deleteOutcomeEffect(campaignId, effect.id)
    ));
    const createRequests = this.storyBeatEventEffectDrafts()
      .map((effect, sortOrder) => this.toStoryBeatOutcomeEffectRequest(effect, source, sortOrder + 1))
      .filter((effect): effect is OutcomeEffectRequest => effect !== null)
      .map((effect) => this.outcomeEffectsApiService.createOutcomeEffect(campaignId, effect));

    const deleteRequest = deleteRequests.length > 0 ? forkJoin(deleteRequests) : of([]);
    const createRequest = createRequests.length > 0
      ? forkJoin(createRequests).pipe(map((responses) => responses.map((response) => response.data).filter((
        effect,
      ): effect is OutcomeEffectModel => effect !== null)))
      : of([] as OutcomeEffectModel[]);

    return deleteRequest.pipe(switchMap(() => createRequest));
  }

  private loadStoryBeatEventEffectSummaries(storyBlocks: StoryBlockViewModel[]): void {
    const campaignId = this.getCampaignId();
    const sources = storyBlocks.flatMap((storyBlock) => (
      storyBlock.beats.flatMap((storyBeat) => this.storyBeatOutcomeEffectSummarySources(storyBeat))
    ));

    if (!campaignId || sources.length === 0) {
      this.storyBeatEventEffectSummaries.set({});
      return;
    }

    forkJoin(sources.map((source) => (
      this.outcomeEffectsApiService
        .fetchOutcomeEffects(campaignId, source.sourceType, source.sourceId)
        .pipe(
          map((response) => ({ source, effects: response.data ?? [] })),
          catchError(() => of({ source, effects: [] as OutcomeEffectModel[] })),
        )
    )))
      .subscribe((results) => {
        const summaries: Record<string, StoryBeatOutcomeEffectSummary[]> = {};

        for (const result of results) {
          if (result.effects.length === 0) {
            continue;
          }

          summaries[result.source.storyBeatId] = [
            ...(summaries[result.source.storyBeatId] ?? []),
            this.toStoryBeatEventEffectSummary(result.source, result.effects),
          ];
        }

        this.storyBeatEventEffectSummaries.set(summaries);
      });
  }

  private loadStoryBeatRuleSummaries(storyBlocks: StoryBlockViewModel[]): void {
    const campaignId = this.getCampaignId();
    const storyBeats = storyBlocks.flatMap((storyBlock) => storyBlock.beats);

    if (!campaignId || storyBeats.length === 0) {
      this.storyBeatRuleSummaries.set({});
      return;
    }

    forkJoin({
      events: this.campaignEventsApiService.fetchCampaignEvents(campaignId).pipe(
        map((response) => response.data ?? []),
        catchError(() => of([] as CampaignEventModel[])),
      ),
      rules: forkJoin(storyBeats.map((storyBeat) => (
        this.campaignRulesApiService
          .fetchRules(campaignId, ConditionalTargetType.StoryBeat, storyBeat.storyBeatId)
          .pipe(
            map((response) => ({
              storyBeatId: storyBeat.storyBeatId,
              rules: response.data ?? [],
            })),
            catchError(() => of({
              storyBeatId: storyBeat.storyBeatId,
              rules: [] as ConditionalRuleModel[],
            })),
          )
      ))),
    }).subscribe(({ events, rules }) => {
      const summaries: Record<string, ConditionalRuleModel[]> = {};

      this.storyBeatRuleSummaryEventOptions.set(events);

      for (const result of rules) {
        if (result.rules.length > 0) {
          summaries[result.storyBeatId] = result.rules;
        }
      }

      this.storyBeatRuleSummaries.set(summaries);
    });
  }

  private setStoryBeatRuleSummary(storyBeat: StoryBeatViewModel, rule: ConditionalRuleModel | null): void {
    const nextSummaries = { ...this.storyBeatRuleSummaries() };
    const currentRules = nextSummaries[storyBeat.storyBeatId] ?? [];

    if (rule) {
      nextSummaries[storyBeat.storyBeatId] = [
        ...currentRules.filter((candidate) => candidate.id !== rule.id),
        rule,
      ];
    } else {
      delete nextSummaries[storyBeat.storyBeatId];
    }

    this.storyBeatRuleSummaries.set(nextSummaries);
  }

  private removeStoryBeatRuleSummary(storyBeat: StoryBeatViewModel, ruleId: string): void {
    const nextSummaries = { ...this.storyBeatRuleSummaries() };
    const remainingRules = (nextSummaries[storyBeat.storyBeatId] ?? [])
      .filter((rule) => rule.id !== ruleId);

    if (remainingRules.length > 0) {
      nextSummaries[storyBeat.storyBeatId] = remainingRules;
    } else {
      delete nextSummaries[storyBeat.storyBeatId];
    }

    this.storyBeatRuleSummaries.set(nextSummaries);
  }

  private toRuleConditionSummaries(group: CampaignRuleGroupRequest): string[] {
    return [
      ...group.clauses.map((condition) => this.toRuleConditionSummary(condition)),
      ...group.groups.flatMap((childGroup) => this.toRuleConditionSummaries(childGroup)),
    ].filter((summary) => summary.length > 0);
  }

  private toRuleConditionSummary(condition: RuleConditionRequest): string {
    const event = this.storyBeatRuleSummaryEventOptions()
      .find((option) => option.id === condition.eventDefinitionId);
    const eventLabel = this.normalizeText(event?.name) ||
      this.normalizeText(event?.key) ||
      'Event';
    const comparisonLabel = this.ruleComparisonLabel(condition.comparisonOperator);
    const valueLabel = this.ruleConditionValueLabel(condition, event);

    return valueLabel ? `${eventLabel} ${comparisonLabel} ${valueLabel}` : `${eventLabel} ${comparisonLabel}`;
  }

  private ruleGroupOperatorLabel(operator: RuleGroupOperator): string {
    if (operator === RuleGroupOperator.Or) {
      return 'OR';
    }

    if (operator === RuleGroupOperator.ExactlyOne) {
      return 'XOR';
    }

    return 'AND';
  }

  private ruleComparisonLabel(comparisonOperator: RuleComparisonOperator): string {
    switch (comparisonOperator) {
      case RuleComparisonOperator.NotEquals:
        return '!=';
      case RuleComparisonOperator.GreaterThan:
        return '>';
      case RuleComparisonOperator.GreaterThanOrEqual:
        return '>=';
      case RuleComparisonOperator.LessThan:
        return '<';
      case RuleComparisonOperator.LessThanOrEqual:
        return '<=';
      case RuleComparisonOperator.IsSet:
        return 'is set';
      case RuleComparisonOperator.IsNotSet:
        return 'not set';
      default:
        return '=';
    }
  }

  private ruleConditionValueLabel(
    condition: RuleConditionRequest,
    event: CampaignEventModel | undefined,
  ): string {
    if (
      condition.comparisonOperator === RuleComparisonOperator.IsSet ||
      condition.comparisonOperator === RuleComparisonOperator.IsNotSet
    ) {
      return '';
    }

    const selectedOption = event?.options?.find((option) => option.id === condition.expectedOptionId);

    return this.normalizeText(selectedOption?.label) ||
      this.normalizeText(condition.textValue) ||
      this.normalizeText(condition.numericValue?.toString()) ||
      (condition.booleanValue === null || condition.booleanValue === undefined
        ? ''
        : condition.booleanValue ? 'true' : 'false');
  }

  private setStoryBeatEventEffectSummary(
    storyBeat: StoryBeatViewModel,
    source: StoryBeatOutcomeEffectSource,
    effects: OutcomeEffectModel[],
  ): void {
    const sourceKey = this.toOutcomeEffectSourceKey(source);
    const nextSummaries = { ...this.storyBeatEventEffectSummaries() };
    const currentSummaries = nextSummaries[storyBeat.storyBeatId] ?? [];
    const remainingSummaries = currentSummaries.filter((summary) => summary.key !== sourceKey);

    if (effects.length > 0) {
      const summarySource = this.storyBeatOutcomeEffectSummarySources(storyBeat)
        .find((candidate) => candidate.key === sourceKey) ?? {
          ...source,
          storyBeatId: storyBeat.storyBeatId,
          key: sourceKey,
          label: this.storyBeatTypeLabel(storyBeat),
        };

      nextSummaries[storyBeat.storyBeatId] = [
        ...remainingSummaries,
        this.toStoryBeatEventEffectSummary(summarySource, effects),
      ];
    } else if (remainingSummaries.length > 0) {
      nextSummaries[storyBeat.storyBeatId] = remainingSummaries;
    } else {
      delete nextSummaries[storyBeat.storyBeatId];
    }

    this.storyBeatEventEffectSummaries.set(nextSummaries);
  }

  private storyBeatOutcomeEffectSummarySources(storyBeat: StoryBeatViewModel): StoryBeatOutcomeEffectSummarySource[] {
    if (this.hasNestedStoryBeatEventEffectTargets(storyBeat)) {
      return this.storyBeatOutcomeEffectTargets(storyBeat).map((target) => ({
        sourceType: target.sourceType,
        sourceId: target.sourceId,
        storyBeatId: storyBeat.storyBeatId,
        key: this.toOutcomeEffectSourceKey(target),
        label: `${target.category}: ${target.title}`,
      }));
    }

    if (!this.isCombatStoryBeat(storyBeat) && !this.isMilestoneStoryBeat(storyBeat)) {
      return [];
    }

    const source = {
      sourceType: OutcomeSourceType.StoryBeat,
      sourceId: storyBeat.storyBeatId,
    };

    return [{
      ...source,
      storyBeatId: storyBeat.storyBeatId,
      key: this.toOutcomeEffectSourceKey(source),
      label: this.storyBeatTypeLabel(storyBeat),
    }];
  }

  private toStoryBeatEventEffectSummary(
    source: StoryBeatOutcomeEffectSummarySource,
    effects: OutcomeEffectModel[],
  ): StoryBeatOutcomeEffectSummary {
    return {
      sourceType: source.sourceType,
      sourceId: source.sourceId,
      key: source.key,
      label: source.label,
      effectCount: effects.length,
      eventLabels: Array.from(new Set(effects.map((effect) => (
        this.normalizeText(effect.eventKey) ||
        this.normalizeText(effect.eventDefinitionId ?? effect.eventId) ||
        'Unknown event'
      )))),
    };
  }

  private toOutcomeEffectSourceKey(source: StoryBeatOutcomeEffectSource): string {
    return `${source.sourceType}:${source.sourceId}`;
  }

  private toOutcomeEffectDraft(effect: OutcomeEffectModel): OutcomeEffectRequest {
    const operation = this.toOutcomeEffectOperation(effect.operationType ?? effect.operation);
    const eventDefinitionId = effect.eventDefinitionId ?? effect.eventId ?? null;

    return {
      sourceType: this.toOutcomeSourceType(effect.sourceType) ?? OutcomeSourceType.StoryBeat,
      sourceId: effect.sourceId,
      eventDefinitionId: eventDefinitionId ?? undefined,
      eventId: eventDefinitionId,
      eventKey: effect.eventKey,
      operationType: operation,
      operation,
      booleanValue: effect.booleanValue ?? null,
      selectedOptionId: effect.selectedOptionId ?? null,
      textValue: effect.textValue ?? null,
      numericValue: effect.numericValue ?? null,
      value: effect.value ?? effect.booleanValue ?? effect.selectedOptionId ?? effect.textValue ?? effect.numericValue ?? null,
      sortOrder: effect.sortOrder,
    };
  }

  private toStoryBeatOutcomeEffectRequest(
    effect: OutcomeEffectRequest,
    source: StoryBeatOutcomeEffectSource,
    sortOrder: number,
  ): OutcomeEffectRequest | null {
    const eventDefinitionId = effect.eventDefinitionId ?? effect.eventId;

    if (!eventDefinitionId) {
      return null;
    }

    const operation = this.toOutcomeEffectOperation(effect.operationType ?? effect.operation);

    return {
      sourceType: source.sourceType,
      sourceId: source.sourceId,
      eventDefinitionId,
      eventId: eventDefinitionId,
      eventKey: effect.eventKey,
      operationType: operation,
      operation,
      booleanValue: operation === OutcomeEffectOperation.Clear ? null : effect.booleanValue ?? null,
      selectedOptionId: operation === OutcomeEffectOperation.Clear ? null : effect.selectedOptionId ?? null,
      textValue: operation === OutcomeEffectOperation.Clear ? null : effect.textValue ?? null,
      numericValue: operation === OutcomeEffectOperation.Clear ? null : effect.numericValue ?? null,
      value: operation === OutcomeEffectOperation.Clear ? null : effect.value ?? null,
      sortOrder,
    };
  }

  private toStoryBeatOutcomeEffectSource(
    source: StoryBeatViewModel | StoryBeatOutcomeEffectTarget,
  ): StoryBeatOutcomeEffectSource {
    if ('sourceType' in source) {
      return {
        sourceType: source.sourceType,
        sourceId: source.sourceId,
      };
    }

    if (this.hasNestedStoryBeatEventEffectTargets(source)) {
      return this.selectedRoleplayingEventSource() ?? {
        sourceType: OutcomeSourceType.StoryBeat,
        sourceId: source.storyBeatId,
      };
    }

    return {
      sourceType: OutcomeSourceType.StoryBeat,
      sourceId: source.storyBeatId,
    };
  }

  private toOutcomeSourceType(
    value: OutcomeEffectModel['sourceType'] | OutcomeEffectRequest['sourceType'],
  ): OutcomeSourceType | undefined {
    if (value === undefined || value === null) {
      return undefined;
    }

    if (
      value === OutcomeSourceType.DecisionChoice ||
      value === 'DecisionChoice'
    ) {
      return OutcomeSourceType.DecisionChoice;
    }

    if (
      value === OutcomeSourceType.RoleplayingNpcInteraction ||
      value === 'RoleplayingNpcInteraction'
    ) {
      return OutcomeSourceType.RoleplayingNpcInteraction;
    }

    if (
      value === OutcomeSourceType.RoleplayingInformation ||
      value === 'RoleplayingInformation'
    ) {
      return OutcomeSourceType.RoleplayingInformation;
    }

    if (typeof value === 'number') {
      return value as OutcomeSourceType;
    }

    const parsedValue = Number(value);

    if (Number.isFinite(parsedValue)) {
      return parsedValue as OutcomeSourceType;
    }

    return OutcomeSourceType[value as keyof typeof OutcomeSourceType] as OutcomeSourceType | undefined;
  }

  private toOutcomeEffectOperation(
    operation: OutcomeEffectOperation | keyof typeof OutcomeEffectOperation | string | number,
  ): OutcomeEffectOperation {
    if (typeof operation === 'number') {
      return operation as OutcomeEffectOperation;
    }

    const parsedOperation = Number(operation);

    if (Number.isFinite(parsedOperation)) {
      return parsedOperation as OutcomeEffectOperation;
    }

    return OutcomeEffectOperation[operation as keyof typeof OutcomeEffectOperation] ?? OutcomeEffectOperation.Set;
  }

  private resetStoryBeatDialogState(): void {
    this.storyBeatDialogBlock.set(null);
    this.editingStoryBeat.set(null);
    this.siblingStoryBeatDraftSource.set(null);
    this.storyBeatTitleDraft.set('');
    this.storyBeatTypeDraft.set(StoryBeatType.Information);
    this.storyBeatNarrativeDraft.set('');
    this.storyBeatRoleplayingDraft.set('');
    this.storyBeatNarrativeParagraphDrafts.set([this.createStoryBeatNarrativeParagraphDraft()]);
    this.storyBeatOptionalInformationDrafts.set([]);
    this.storyBeatRoleplayingInformationDrafts.set([]);
    this.storyBeatDecisionDescriptionDraft.set('');
    this.storyBeatCombatDescriptionDraft.set('');
    this.storyBeatTransitionDescriptionDraft.set('');
    this.storyBeatCombatRewardDrafts.set([]);
    this.storyBeatCombatEnemyNpcDrafts.set([]);
    const decisionChoiceDraft = this.createStoryBeatDecisionChoiceDraft();

    this.storyBeatDecisionChoiceDrafts.set([decisionChoiceDraft]);
    this.activeStoryBeatDecisionChoiceDraftId.set(decisionChoiceDraft.draftId);
    this.storyBeatMilestoneDraft.set(null);
  }

  private normalizeText(value: string | null | undefined): string {
    return value?.trim() ?? '';
  }

  private toNullableText(value: string): string | null {
    const normalizedValue = this.normalizeText(value);

    return normalizedValue ? normalizedValue : null;
  }

  private toDisplayDate(value: string): string {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return '-';
    }

    return date.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  private ensureRoleplayingNpcRecords(campaignId: string): Observable<CampaignNpcModel[]> {
    const annotatedNpcs = this.roleplayingNpcOptions();

    if (annotatedNpcs.length === 0) {
      return of([]);
    }

    return this.campaignApiService.fetchRoleplayingStoryBeatNpcs(campaignId).pipe(
      switchMap((response) => {
        const existingNpcs = response.data ?? [];
        const existingTags = new Set(existingNpcs.map((npc) => npc.tag.toLowerCase()));
        const missingNpcs = annotatedNpcs.filter((npc) => !existingTags.has(npc.key.toLowerCase()));

        if (missingNpcs.length === 0) {
          this.roleplayingNpcs.set(existingNpcs);
          return of(existingNpcs);
        }

        return forkJoin(missingNpcs.map((npc) => (
          this.campaignApiService.createRoleplayingNpc(campaignId, {
            tag: npc.key,
            name: npc.name,
            displayName: npc.name,
            description: null,
          })
        ))).pipe(
          map((createdResponses) => {
            const createdNpcs = createdResponses
              .map((createdResponse) => createdResponse.data)
              .filter((npc): npc is CampaignNpcModel => npc !== null);
            const nextNpcs = [
              ...existingNpcs,
              ...createdNpcs,
            ];

            this.roleplayingNpcs.set(nextNpcs);

            return nextNpcs;
          }),
        );
      }),
    );
  }

  private loadCombatNpcOptions(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingCombatNpcOptions() || this.combatNpcOptions().length > 0) {
      return;
    }

    this.isLoadingCombatNpcOptions.set(true);

    this.monsterApiService
      .fetchCampaignMonsterDetails(campaignId)
      .pipe(finalize(() => this.isLoadingCombatNpcOptions.set(false)))
      .subscribe({
        next: (response) => {
          this.combatNpcOptions.set(response.data ?? []);
          this.storyBeatCombatEnemyNpcDrafts.update((drafts) => drafts.map((draft) => (
            draft.monsterId === null
              ? { ...draft, monsterId: this.firstAvailableCombatNpcId(draft.draftId) }
              : draft
          )));
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Combat NPCs could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private toRoleplayingNpcRows(
    npcs: CampaignNpcModel[],
  ): CampaignRoleplayingNpcTableRow[] {
    return npcs
      .map((npc) => {
        const tag = this.normalizeText(npc.tag);
        const name = this.normalizeText(npc.name);

        return {
          key: npc.campaignNpcId || tag || name,
          campaignNpcId: npc.campaignNpcId,
          tag: tag || 'untagged',
          name,
          displayName: this.toCampaignNpcDisplayName(npc),
          description: this.normalizeText(npc.description),
          createdAt: npc.createdAt,
          updatedAt: npc.updatedAt,
        };
      })
      .sort((firstRow, secondRow) => (
        this.roleplayingNpcDisplayName(firstRow).localeCompare(
          this.roleplayingNpcDisplayName(secondRow),
        )
      ));
  }

  private toInformationStoryBeatRequest(): CreateInformationStoryBeatRequest {
    const narrative = this.storyBeatNarrativeDraft();
    const inlineInformation = this.toInlineOptionalInformationRequests(narrative);
    const appendedInformation = this.storyBeatOptionalInformationDrafts()
      .map((draft) => ({
        skill: draft.skill,
        difficultyClass: draft.difficultyClass,
        information: this.normalizeText(draft.information),
        placement: StoryBeatOptionalInformationPlacement.Appended,
        narrativeOffset: null,
      }))
      .filter((request) => request.information.length > 0);

    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
      ...this.createStoryBeatOrderDraft(),
      information: {
        narrative,
        optionalInformation: [
          ...inlineInformation,
          ...appendedInformation,
        ],
      },
    };
  }

  private createStoryBeatOrderDraft(): CreateStoryBeatOrderDraft {
    const siblingSource = this.siblingStoryBeatDraftSource();

    return siblingSource
      ? {
        orderIndex: siblingSource.orderIndex,
        secondaryOrderIndex: null,
      }
      : {};
  }

  private toNarrativeStoryBeatRequest(): CreateNarrativeStoryBeatRequest {
    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
      ...this.createStoryBeatOrderDraft(),
      narrative: {
        paragraphs: this.storyBeatNarrativeParagraphDrafts()
          .map((draft) => this.normalizeText(draft.text))
          .filter((paragraph) => paragraph.length > 0),
      },
    };
  }

  private toRoleplayingStoryBeatRequest():
    CreateRoleplayingStoryBeatRequest | UpdateRoleplayingStoryBeatRequest {
    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
      ...this.createStoryBeatOrderDraft(),
      roleplaying: {
        mainDescription: this.normalizeText(this.storyBeatRoleplayingDraft()),
        npcTags: this.roleplayingNpcOptions().map((npc) => npc.key),
        discoverableInformation: this.storyBeatRoleplayingInformationDrafts()
          .map((draft) => {
            const checkType = draft.checkType;

            return {
              npcTag: draft.npcTag,
              checkType,
              skill: checkType === StoryBeatRoleplayingCheckType.Skill ? draft.skill : null,
              ability: checkType === StoryBeatRoleplayingCheckType.Ability ? draft.ability : null,
              difficultyClass: checkType === StoryBeatRoleplayingCheckType.None
                ? null
                : draft.difficultyClass,
              information: this.normalizeText(draft.information),
            };
          })
          .filter((request) => request.information.length > 0),
      },
    };
  }

  private toDecisionStoryBeatRequest(): CreateDecisionStoryBeatRequest | UpdateDecisionStoryBeatRequest {
    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
      ...this.createStoryBeatOrderDraft(),
      decision: {
        description: this.normalizeText(this.storyBeatDecisionDescriptionDraft()),
        decisions: this.storyBeatDecisionChoiceDrafts().map((draft) => ({
          id: draft.id,
          title: this.normalizeText(draft.title),
          description: this.normalizeText(draft.description),
          isSelected: false,
        })),
      },
    };
  }

  private toCombatStoryBeatRequest(): CreateCombatStoryBeatRequest | UpdateCombatStoryBeatRequest {
    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
      ...this.createStoryBeatOrderDraft(),
      combat: {
        description: this.normalizeText(this.storyBeatCombatDescriptionDraft()),
        rewards: this.toNullableCombatRewardRequests(),
        enemyNpcs: this.storyBeatCombatEnemyNpcDrafts()
          .filter((draft) => draft.monsterId !== null && draft.amount >= 1)
          .map((draft) => ({
            monsterId: draft.monsterId ?? 0,
            amount: draft.amount,
          })),
      },
    };
  }

  private toTransitionStoryBeatRequest():
    CreateTransitionStoryBeatRequest | UpdateTransitionStoryBeatRequest {
    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
      ...this.createStoryBeatOrderDraft(),
      transition: {
        description: this.normalizeText(this.storyBeatTransitionDescriptionDraft()),
      },
    };
  }

  private toNullableCombatRewardRequests(): (string | null)[] | null {
    const rewards = this.storyBeatCombatRewardDrafts()
      .map((draft) => this.normalizeText(draft.text))
      .filter((reward) => reward.length > 0);

    return rewards.length > 0 ? rewards : null;
  }

  private toCombatRewardDrafts(rewards: unknown): StoryBeatCombatRewardDraft[] {
    return this.toRewardLines(rewards).map((reward) => (
      this.createStoryBeatCombatRewardDraft(reward)
    ));
  }

  private toRewardLines(rewards: unknown): string[] {
    if (Array.isArray(rewards)) {
      return rewards
        .map((reward) => typeof reward === 'string' ? this.normalizeText(reward) : '')
        .filter((reward) => reward.length > 0);
    }

    if (typeof rewards === 'string') {
      return rewards
        .split(/\r?\n/)
        .map((reward) => this.normalizeText(reward))
        .filter((reward) => reward.length > 0);
    }

    return [];
  }

  private toMilestoneStoryBeatRequest(): CreateMilestoneStoryBeatRequest {
    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
      ...this.createStoryBeatOrderDraft(),
      milestoneId: this.storyBeatMilestoneDraft() ?? 0,
    };
  }

  private toInlineOptionalInformationRequests(
    narrative: string,
  ): StoryBeatOptionalInformationRequest[] {
    return [...narrative.matchAll(/\[([A-Za-z ]+)-(\d{1,2}): ([^\]]+)\]/g)]
      .map((match) => {
        const skill = this.toSkillByLabel(match[1]);
        const difficultyClass = Number(match[2]);
        const information = this.normalizeText(match[3]);

        if (!skill || !this.isValidDifficultyClass(difficultyClass) || !information) {
          return null;
        }

        const request: StoryBeatOptionalInformationRequest = {
          skill,
          difficultyClass,
          information,
          placement: StoryBeatOptionalInformationPlacement.Inline,
          narrativeOffset: match.index,
        };

        return request;
      })
      .filter((request): request is StoryBeatOptionalInformationRequest => request !== null);
  }

  private convertSlashTokens(value: string): string {
    return value.replace(
      /\/([A-Za-z ]+)::(\d{1,2})::([^/]+)\//g,
      (match, skillLabel: string, difficultyClassText: string, information: string) => {
        const skill = this.toSkillByLabel(skillLabel);
        const difficultyClass = Number(difficultyClassText);
        const normalizedInformation = this.normalizeText(information);

        if (!skill || !this.isValidDifficultyClass(difficultyClass) || !normalizedInformation) {
          return match;
        }

        return `[${this.getSkillLabel(skill)}-${difficultyClass}: ${normalizedInformation}]`;
      },
    );
  }

  private toNarrativePreviewParts(value: string, tokenKeyPrefix: string | null = null): StoryBeatNarrativePart[] {
    const parts: StoryBeatNarrativePart[] = [];
    const tokenExpression = /\[([A-Za-z ]+)-(\d{1,2}): ([^\]]+)\]/g;
    let lastIndex = 0;

    for (const match of value.matchAll(tokenExpression)) {
      if (match.index > lastIndex) {
        parts.push({
          text: value.slice(lastIndex, match.index),
          className: null,
        });
      }

      const skill = this.toSkillByLabel(match[1]);
      const difficultyClass = Number(match[2]);
      const skillLabel = skill ? this.getSkillLabel(skill) : match[1];
      const information = match[3];
      const tokenKey = tokenKeyPrefix ? `${tokenKeyPrefix}:${match.index}:${match[0]}` : undefined;

      parts.push({
        text: match[0],
        compactText: `[${skillLabel}-${difficultyClass}]`,
        detailText: information,
        tokenKey,
        className: skill
          ? this.getSkillTokenClass(skill, difficultyClass)
          : null,
      });

      lastIndex = match.index + match[0].length;
    }

    if (lastIndex < value.length) {
      parts.push({
        text: value.slice(lastIndex),
        className: null,
      });
    }

    return parts.length > 0 ? parts : [{ text: value, className: null }];
  }

  private toRoleplayingPreviewParts(value: string): StoryBeatNarrativePart[] {
    const parts: StoryBeatNarrativePart[] = [];
    const tokenExpression = /\/@([A-Za-z0-9_-]+)::([^/]+)\//g;
    let lastIndex = 0;

    for (const match of value.matchAll(tokenExpression)) {
      if (match.index > lastIndex) {
        parts.push({
          text: value.slice(lastIndex, match.index),
          className: null,
        });
      }

      parts.push({
        text: this.getRoleplayingNpcDisplayNameByTag(match[1], match[2]),
        className: 'story-roleplaying-npc-token',
      });

      lastIndex = match.index + match[0].length;
    }

    if (lastIndex < value.length) {
      parts.push({
        text: value.slice(lastIndex),
        className: null,
      });
    }

    return parts.length > 0 ? parts : [{ text: value, className: null }];
  }

  private getRoleplayingNpcDisplayNameByTag(tag: string, fallbackName: string): string {
    const normalizedTag = this.normalizeText(tag).toLowerCase();
    const fallback = this.normalizeText(fallbackName);

    if (!normalizedTag) {
      return fallback;
    }

    const npc = this.roleplayingNpcs().find((candidate) => (
      candidate.tag.toLowerCase() === normalizedTag
    ));

    return (npc ? this.toCampaignNpcDisplayName(npc) : '') ||
      fallback;
  }

  private toCampaignNpcDisplayName(npc: CampaignNpcModel): string {
    return this.normalizeText(npc.displayName) ||
      this.normalizeText(npc.display_name) ||
      this.normalizeText(npc.name);
  }

  private toRoleplayingNpcOptions(value: string): RoleplayingNpcOption[] {
    const npcOptions: RoleplayingNpcOption[] = [];
    const npcTags = new Set<string>();
    const tokenExpression = /\/@([A-Za-z0-9_-]+)::([^/]+)\//g;

    for (const match of value.matchAll(tokenExpression)) {
      const key = this.normalizeText(match[1]);
      const name = this.normalizeText(match[2]);
      const normalizedKey = key.toLowerCase();

      if (!key || !name || npcTags.has(normalizedKey)) {
        continue;
      }

      npcOptions.push({ key, name });
      npcTags.add(normalizedKey);
    }

    return npcOptions;
  }

  private findRoleplayingNpcTag(storyBeat: StoryBeatViewModel, npcName: string): string {
    return storyBeat.roleplaying?.npcs?.find((npc) => (
      npc.name === npcName
    ))?.tag ?? this.toRoleplayingNpcOptions(storyBeat.roleplaying?.mainDescription ?? '')
      .find((npc) => npc.name === npcName)?.key ?? npcName;
  }

  private findRoleplayingNpcName(
    storyBeat: StoryBeatViewModel,
    npcTag: string,
    fallbackName: string | undefined,
  ): string {
    return storyBeat.roleplaying?.npcs?.find((npc) => (
      npc.tag === npcTag
    ))?.name ?? this.toRoleplayingNpcOptions(storyBeat.roleplaying?.mainDescription ?? '')
      .find((npc) => npc.key === npcTag)?.name ?? fallbackName ?? npcTag;
  }

  private getActiveSlashSkillQuery(value: string): string | null {
    const slashIndex = value.lastIndexOf('/');

    if (slashIndex < 0) {
      return null;
    }

    const fragment = value.slice(slashIndex + 1);

    if (fragment.includes('/') || fragment.includes('::') || /\s/.test(fragment)) {
      return null;
    }

    return fragment;
  }

  private createStoryBeatOptionalInformationDraft(): StoryBeatOptionalInformationDraft {
    return {
      draftId: this.nextStoryBeatOptionalInformationDraftId++,
      skill: Skill.Perception,
      difficultyClass: 10,
      information: '',
    };
  }

  private createStoryBeatRoleplayingInformationDraft(): StoryBeatRoleplayingInformationDraft {
    const npc = this.roleplayingNpcOptions()[0] ?? null;

    return {
      draftId: this.nextStoryBeatOptionalInformationDraftId++,
      npcTag: npc?.key ?? '',
      npcName: npc?.name ?? '',
      checkType: StoryBeatRoleplayingCheckType.None,
      skill: Skill.Insight,
      ability: Ability.CHARISMA,
      difficultyClass: 10,
      information: '',
    };
  }

  private createStoryBeatCombatRewardDraft(text = ''): StoryBeatCombatRewardDraft {
    return {
      draftId: this.nextStoryBeatCombatRewardDraftId++,
      text,
    };
  }

  private createStoryBeatCombatEnemyNpcDraft(monsterId: number | null = null): StoryBeatCombatEnemyNpcDraft {
    return {
      draftId: this.nextStoryBeatCombatEnemyNpcDraftId++,
      monsterId,
      amount: 1,
    };
  }

  private firstAvailableCombatNpcId(excludingDraftId: number | null = null): number | null {
    const selectedMonsterIds = new Set(
      this.storyBeatCombatEnemyNpcDrafts()
        .filter((draft) => draft.draftId !== excludingDraftId)
        .map((draft) => draft.monsterId)
        .filter((monsterId): monsterId is number => monsterId !== null),
    );

    return this.combatNpcOptions()
      .find((monster) => !selectedMonsterIds.has(monster.id))
      ?.id ?? null;
  }

  private createStoryBeatDecisionChoiceDraft(): StoryBeatDecisionChoiceDraft {
    return {
      draftId: this.nextStoryBeatDecisionChoiceDraftId++,
      id: null,
      title: '',
      description: '',
    };
  }

  private createStoryBeatNarrativeParagraphDraft(): StoryBeatNarrativeParagraphDraft {
    return {
      draftId: this.nextStoryBeatNarrativeParagraphDraftId++,
      text: '',
    };
  }

  private isValidDifficultyClass(difficultyClass: number): boolean {
    return Number.isInteger(difficultyClass) && difficultyClass >= 1 && difficultyClass <= 30;
  }

  private toSkillByLabel(label: string): Skill | null {
    const normalizedLabel = label.toLowerCase().replace(/\s+/g, '');

    return this.skillOptions.find((option) => (
      option.label.toLowerCase().replace(/\s+/g, '') === normalizedLabel ||
      Skill[option.skill].toLowerCase() === normalizedLabel
    ))?.skill ?? null;
  }

  private toMilestoneImportance(
    importance: CampaignMilestoneModel['importance'],
  ): CampaignMilestoneImportance {
    if (typeof importance === 'number') {
      return importance as CampaignMilestoneImportance;
    }

    const parsedImportance = Number(importance);

    if (Number.isFinite(parsedImportance)) {
      return parsedImportance as CampaignMilestoneImportance;
    }

    return toCampaignMilestoneImportance(importance);
  }

  private createEmptyQuestTaskDraft(): QuestTaskDraft {
    return {
      draftId: this.nextQuestTaskDraftId++,
      title: '',
      description: '',
      dateCompleted: null,
    };
  }

  private toStoryBeatViewModels(storyBeats: StoryBeatModel[]): StoryBeatViewModel[] {
    return storyBeats.map((storyBeat, index) => ({
      ...storyBeat,
      orderIndex: storyBeat.orderIndex ?? index + 1,
      secondaryOrderIndex: storyBeat.secondaryOrderIndex ?? 1,
      milestone: storyBeat.milestone ?? null,
      displayIndex: storyBeat.orderIndex ?? index + 1,
    }));
  }

  private storyBeatRowKey(storyBlockId: string, orderIndex: number): string {
    return `${storyBlockId}:${orderIndex}`;
  }

  private setStoryBeatAlternativeIndex(
    storyBeatRow: StoryBeatRowViewModel,
    index: number,
  ): void {
    this.setStoryBeatAlternativeIndexByKey(
      storyBeatRow.key,
      this.clampStoryBeatAlternativeIndex(index, storyBeatRow.beats),
    );
  }

  private setStoryBeatAlternativeIndexByKey(key: string, index: number): void {
    this.storyBeatAlternativeIndexes.update((indexes) => ({
      ...indexes,
      [key]: Math.max(0, index),
    }));
  }

  private applyStoryBeatIndexPathRule(rule: StoryBeatIndexPathRuleModel): void {
    this.storyBlocks.update((storyBlocks) => storyBlocks.map((storyBlock) => (
      storyBlock.storyBlockId === rule.storyBlockId
        ? {
          ...storyBlock,
          beats: storyBlock.beats.map((beat) => (
            beat.orderIndex === rule.orderIndex
              ? { ...beat, indexPathRule: rule }
              : beat
          )),
        }
        : storyBlock
    )));
  }

  private clearStoryBeatIndexPathRule(storyBlockId: string, orderIndex: number): void {
    this.storyBlocks.update((storyBlocks) => storyBlocks.map((storyBlock) => (
      storyBlock.storyBlockId === storyBlockId
        ? {
          ...storyBlock,
          beats: storyBlock.beats.map((beat) => (
            beat.orderIndex === orderIndex
              ? { ...beat, indexPathRule: null }
              : beat
          )),
        }
        : storyBlock
    )));
  }

  private clampStoryBeatAlternativeIndex(
    index: number,
    beats: StoryBeatViewModel[],
  ): number {
    if (beats.length === 0) {
      return 0;
    }

    return Math.min(Math.max(0, index), beats.length - 1);
  }

  private toQuestType(type: CampaignQuestModel['type']): CampaignQuestType {
    if (typeof type === 'number') {
      return type as CampaignQuestType;
    }

    const parsedType = Number(type);

    if (Number.isFinite(parsedType)) {
      return parsedType as CampaignQuestType;
    }

    return CampaignQuestType[type as keyof typeof CampaignQuestType] ?? CampaignQuestType.MainQuest;
  }

  private toStoryBeatType(type: StoryBeatModel['storyBeatType']): StoryBeatType {
    if (typeof type === 'number') {
      return type as StoryBeatType;
    }

    const parsedType = Number(type);

    if (Number.isFinite(parsedType)) {
      return parsedType as StoryBeatType;
    }

    return StoryBeatType[type as keyof typeof StoryBeatType] ?? StoryBeatType.Information;
  }

  private toStoryBeatIndexPathRuleRelationType(
    relationType: StoryBeatIndexPathRuleRelationTypeValue | null | undefined,
  ): StoryBeatIndexPathRuleRelationType | null {
    if (relationType === null || relationType === undefined) {
      return null;
    }

    if (typeof relationType === 'number') {
      return relationType in StoryBeatIndexPathRuleRelationType
        ? relationType as StoryBeatIndexPathRuleRelationType
        : null;
    }

    const parsedRelationType = Number(relationType);

    if (Number.isFinite(parsedRelationType)) {
      return parsedRelationType in StoryBeatIndexPathRuleRelationType
        ? parsedRelationType as StoryBeatIndexPathRuleRelationType
        : null;
    }

    return StoryBeatIndexPathRuleRelationType[
      relationType as keyof typeof StoryBeatIndexPathRuleRelationType
    ] ?? null;
  }

  private toOptionalInformationPlacement(
    placement: StoryBeatOptionalInformationModel['placement'],
  ): StoryBeatOptionalInformationPlacement {
    if (typeof placement === 'number') {
      return placement as StoryBeatOptionalInformationPlacement;
    }

    const parsedPlacement = Number(placement);

    if (Number.isFinite(parsedPlacement)) {
      return parsedPlacement as StoryBeatOptionalInformationPlacement;
    }

    return StoryBeatOptionalInformationPlacement[
      placement as keyof typeof StoryBeatOptionalInformationPlacement
    ] ?? StoryBeatOptionalInformationPlacement.Appended;
  }

  private toSkill(skill: SkillValue | null): Skill | null {
    if (skill === null) {
      return null;
    }

    if (typeof skill === 'number') {
      return skill as Skill;
    }

    const parsedSkill = Number(skill);

    if (Number.isFinite(parsedSkill)) {
      return parsedSkill as Skill;
    }

    return Skill[skill as keyof typeof Skill] ?? null;
  }

  private toAbility(ability: AbilityValue | null): Ability | null {
    if (ability === null) {
      return null;
    }

    if (typeof ability === 'number') {
      return ability as Ability;
    }

    const parsedAbility = Number(ability);

    if (Number.isFinite(parsedAbility)) {
      return parsedAbility as Ability;
    }

    return Ability[ability as keyof typeof Ability] ?? null;
  }

  private toRoleplayingCheckType(
    checkType: StoryBeatRoleplayingInformationModel['checkType'],
  ): StoryBeatRoleplayingCheckType {
    if (typeof checkType === 'number') {
      return checkType as StoryBeatRoleplayingCheckType;
    }

    const parsedCheckType = Number(checkType);

    if (Number.isFinite(parsedCheckType)) {
      return parsedCheckType as StoryBeatRoleplayingCheckType;
    }

    return StoryBeatRoleplayingCheckType[
      checkType as keyof typeof StoryBeatRoleplayingCheckType
    ] ?? StoryBeatRoleplayingCheckType.None;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (this.isApiError(error) || error instanceof Error) {
      return error.message;
    }

    return fallback;
  }

  private getErrorStatus(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private getQuestDeleteBlockers(error: unknown): CampaignQuestDeleteBlockerModel[] {
    if (!this.isApiError(error) || !this.hasApiResponseData(error.details)) {
      return [];
    }

    const data = error.details.data;

    return Array.isArray(data)
      ? data.filter((blocker): blocker is CampaignQuestDeleteBlockerModel => (
        this.isQuestDeleteBlocker(blocker)
      ))
      : [];
  }

  private getQuestDeleteErrorMessages(
    error: unknown,
    blockers: CampaignQuestDeleteBlockerModel[],
  ): string[] {
    const messages = [
      this.getErrorMessage(error, 'Campaign quest could not be deleted.'),
    ];

    if (blockers.length === 0) {
      return messages;
    }

    messages.push('Quest deletion is blocked by:');

    blockers.forEach((blocker, index) => {
      messages.push(`${index + 1}. ${this.questDeleteBlockerMessage(blocker)}`);
      messages.push(`Quest task: ${blocker.questTaskTitle} (${blocker.questTaskId})`);
      messages.push(`Story block: ${this.formatQuestDeleteBlockerEntity(
        blocker.storyBlockTitle,
        blocker.storyBlockId,
        blocker.storyBlockOrderIndex !== null ? `order ${blocker.storyBlockOrderIndex}` : null,
      )}`);
      messages.push(`Story beat: ${this.formatQuestDeleteBlockerEntity(
        blocker.storyBeatTitle,
        blocker.storyBeatId,
        this.questDeleteBlockerStoryBeatIndex(blocker) !== 'Unknown'
          ? `index ${this.questDeleteBlockerStoryBeatIndex(blocker)}`
          : null,
      )}`);
      messages.push(`Location: ${this.questDeleteBlockerLocation(blocker)}`);
    });

    return messages;
  }

  private formatQuestDeleteBlockerEntity(
    title: string | null,
    id: string | null,
    detail: string | null,
  ): string {
    const parts = [
      this.normalizeText(title ?? ''),
      id ? `(${id})` : '',
      detail ?? '',
    ].filter((value) => value.length > 0);

    return parts.length > 0 ? parts.join(' ') : 'Unknown';
  }

  private hasApiResponseData(value: unknown): value is { data: unknown } {
    return typeof value === 'object' &&
      value !== null &&
      'data' in value;
  }

  private isQuestDeleteBlocker(value: unknown): value is CampaignQuestDeleteBlockerModel {
    return typeof value === 'object' &&
      value !== null &&
      'questTaskId' in value &&
      typeof value.questTaskId === 'string' &&
      'questTaskTitle' in value &&
      typeof value.questTaskTitle === 'string';
  }

  private isApiError(error: unknown): error is ApiError {
    return (
      typeof error === 'object' &&
      error !== null &&
      'message' in error &&
      typeof error.message === 'string' &&
      'status' in error &&
      typeof error.status === 'number'
    );
  }
}
