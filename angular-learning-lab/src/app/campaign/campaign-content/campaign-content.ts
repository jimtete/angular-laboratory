import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LucideCheck, LucidePencil, LucidePlus, LucideTrash2, LucideX } from '@lucide/angular';
import { catchError, finalize, forkJoin, map, Observable, of, switchMap, timeout } from 'rxjs';

import {
  Ability,
  AbilityValue,
  ApiError,
  CampaignApiService,
  CampaignMilestoneImportance,
  CampaignMilestoneModel,
  CampaignMilestoneRequest,
  CampaignNpcModel,
  CampaignQuestModel,
  CampaignQuestType,
  Skill,
  SkillValue,
  CreateStoryBlockRequest,
  CreateDecisionStoryBeatRequest,
  CreateInformationStoryBeatRequest,
  CreateMilestoneStoryBeatRequest,
  CreateNarrativeStoryBeatRequest,
  CreateRoleplayingStoryBeatRequest,
  CreateCampaignQuestRequest,
  getCampaignMilestoneImportanceLabel,
  getCampaignMilestoneImportanceSlug,
  StoryBeatOptionalInformationModel,
  StoryBeatOptionalInformationPlacement,
  StoryBeatOptionalInformationRequest,
  StoryBeatModel,
  StoryBeatRoleplayingCheckType,
  StoryBeatRoleplayingInformationModel,
  StoryBeatType,
  StoryBlockModel,
  toCampaignMilestoneImportance,
  UpdateDecisionStoryBeatRequest,
  UpdateStoryBlockTitleRequest,
  UpdateRoleplayingStoryBeatRequest,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

type CampaignContentTab =
  'main-story' |
  'campaign-milestones' |
  'quests' |
  'roleplaying-npcs' |
  'combat-npcs';
type QuestCarouselItem = CampaignQuestModel | 'add-quest';
type QuestFormStep = 'details' | 'tasks';

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
  title: string;
  description: string;
}

interface StoryBeatNarrativePart {
  text: string;
  className: string | null;
}

interface QuestTaskDraft {
  draftId: number;
  title: string;
  description: string;
}

@Component({
  selector: 'app-campaign-content',
  imports: [LucideCheck, LucidePencil, LucidePlus, LucideTrash2, LucideX],
  templateUrl: './campaign-content.html',
  styleUrl: './campaign-content.css',
})
export class CampaignContent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignApiService = inject(CampaignApiService);
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
  protected readonly isDeletingMilestone = signal(false);
  protected readonly deletingStoryBlockId = signal<string | null>(null);
  protected readonly deletingStoryBeatId = signal<string | null>(null);
  protected readonly editingMilestone = signal<CampaignMilestoneModel | null>(null);
  protected readonly editingStoryBlock = signal<StoryBlockViewModel | null>(null);
  protected readonly editingStoryBeat = signal<StoryBeatViewModel | null>(null);
  protected readonly storyBeatDialogBlock = signal<StoryBlockViewModel | null>(null);
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
  protected readonly selectedStoryBlock = computed(() => {
    const selectedStoryBlockId = this.selectedStoryBlockId();

    if (!selectedStoryBlockId) {
      return null;
    }

    return this.storyBlocks().find((storyBlock) => (
      storyBlock.storyBlockId === selectedStoryBlockId
    )) ?? null;
  });
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
    this.questTaskDrafts()
      .some((task) => (
        this.normalizeText(task.title).length > 0 &&
        this.normalizeText(task.description).length > 0
      ))
  ));
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
      disabled: true,
    },
    {
      value: StoryBeatType.Transition,
      label: 'Transition',
      disabled: true,
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

  ngOnInit(): void {
    if (this.selectedTab() === 'main-story') {
      this.loadStoryContent();
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
    this.resetStoryBeatDialogState();
    this.storyBeatDialogBlock.set(storyBlock);
    this.isCreateStoryBeatDialogOpen.set(true);
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
        title: choice.title,
        description: choice.description,
      }))
      : [this.createStoryBeatDecisionChoiceDraft()];

    this.storyBeatDecisionChoiceDrafts.set(decisionChoiceDrafts);
    this.activeStoryBeatDecisionChoiceDraftId.set(decisionChoiceDrafts[0]?.draftId ?? null);
    this.isCreateStoryBeatDialogOpen.set(true);
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
    return this.editingStoryBeat() ? 'Edit Story Beat' : 'Create Story Beat';
  }

  protected storyBeatDialogActionText(): string {
    if (this.creatingStoryBeatBlockId()) {
      return 'Creating...';
    }

    if (this.updatingStoryBeatId()) {
      return 'Updating...';
    }

    return this.editingStoryBeat() ? 'Update' : 'Create';
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
            : value === StoryBeatType.Milestone
              ? StoryBeatType.Milestone
              : StoryBeatType.Information,
    );
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

  protected isMilestoneStoryBeatDraft(): boolean {
    return this.storyBeatTypeDraft() === StoryBeatType.Milestone;
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

          const storyBeat = response.data;

          if (!storyBeat) {
            this.loadStoryContent();
            return;
          }

          this.storyBlocks.update((blocks) => blocks.map((block) => (
            block.storyBlockId === storyBlock.storyBlockId
              ? {
                ...block,
                beats: [
                  ...block.beats,
                  {
                    ...storyBeat,
                    milestone: storyBeat.milestone ?? null,
                    displayIndex: block.beats.length + 1,
                  },
                ],
              }
              : block
          )));
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

          this.storyBlocks.update((blocks) => blocks.map((block) => (
            block.storyBlockId === storyBlock.storyBlockId
              ? {
                ...block,
                beats: block.beats.map((beat) => (
                  beat.storyBeatId === storyBeat.storyBeatId
                    ? {
                      ...storyBeat,
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
    return this.toNarrativePreviewParts(this.storyBeatNarrative(storyBeat));
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

  protected storyBeatNarrativeParagraphs(storyBeat: StoryBeatViewModel): string[] {
    return storyBeat.narrative?.paragraphs ?? [];
  }

  protected storyBeatDecisionChoices(storyBeat: StoryBeatViewModel): StoryBeatDecisionChoiceDraft[] {
    return (storyBeat.decision?.decisions ?? []).map((choice) => ({
      draftId: choice.orderIndex,
      title: choice.title,
      description: choice.description,
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
    if (this.isCreatingQuest()) {
      return;
    }

    this.isCreateQuestDialogOpen.set(false);
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

  protected createQuest(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || !this.canCreateQuest()) {
      return;
    }

    const request: CreateCampaignQuestRequest = {
      type: this.questTypeDraft(),
      title: this.normalizeText(this.questTitleDraft()),
      description: this.normalizeText(this.questDescriptionDraft()),
      givenBy: this.normalizeText(this.questGivenByDraft()),
      reward: this.normalizeText(this.questRewardDraft()),
      completedAt: null,
      tasks: this.questTaskDrafts()
        .map((task) => ({
          title: this.normalizeText(task.title),
          description: this.normalizeText(task.description),
          dateCompleted: null,
        }))
        .filter((task) => task.title.length > 0 && task.description.length > 0),
    };

    this.isCreatingQuest.set(true);

    this.campaignApiService
      .createCampaignQuest(campaignId, request)
      .pipe(finalize(() => this.isCreatingQuest.set(false)))
      .subscribe({
        next: (response) => {
          this.isCreateQuestDialogOpen.set(false);
          this.loadQuests(response.data?.questId ?? null);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign quest could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
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

  private getCampaignId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  }

  private resetStoryBeatDialogState(): void {
    this.storyBeatDialogBlock.set(null);
    this.editingStoryBeat.set(null);
    this.storyBeatTitleDraft.set('');
    this.storyBeatTypeDraft.set(StoryBeatType.Information);
    this.storyBeatNarrativeDraft.set('');
    this.storyBeatRoleplayingDraft.set('');
    this.storyBeatNarrativeParagraphDrafts.set([this.createStoryBeatNarrativeParagraphDraft()]);
    this.storyBeatOptionalInformationDrafts.set([]);
    this.storyBeatRoleplayingInformationDrafts.set([]);
    this.storyBeatDecisionDescriptionDraft.set('');
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
      information: {
        narrative,
        optionalInformation: [
          ...inlineInformation,
          ...appendedInformation,
        ],
      },
    };
  }

  private toNarrativeStoryBeatRequest(): CreateNarrativeStoryBeatRequest {
    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
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
      decision: {
        description: this.normalizeText(this.storyBeatDecisionDescriptionDraft()),
        decisions: this.storyBeatDecisionChoiceDrafts().map((draft) => ({
          title: this.normalizeText(draft.title),
          description: this.normalizeText(draft.description),
          isSelected: false,
        })),
      },
    };
  }

  private toMilestoneStoryBeatRequest(): CreateMilestoneStoryBeatRequest {
    return {
      title: this.normalizeText(this.storyBeatTitleDraft()),
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

  private toNarrativePreviewParts(value: string): StoryBeatNarrativePart[] {
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

      parts.push({
        text: match[0],
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

  private createStoryBeatDecisionChoiceDraft(): StoryBeatDecisionChoiceDraft {
    return {
      draftId: this.nextStoryBeatDecisionChoiceDraftId++,
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
    };
  }

  private toStoryBeatViewModels(storyBeats: StoryBeatModel[]): StoryBeatViewModel[] {
    return storyBeats.map((storyBeat, index) => ({
      ...storyBeat,
      milestone: storyBeat.milestone ?? null,
      displayIndex: index + 1,
    }));
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
