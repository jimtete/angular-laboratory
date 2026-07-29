import { Component, HostListener, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideArrowLeft, LucideCalendarDays, LucideCirclePlay, LucideMap, LucidePackage } from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  ApiError,
  AchieveCampaignMilestoneRequest,
  AssetItemType,
  AssetModel,
  CampaignApiService,
  CampaignMemberInformationModel,
  CampaignMilestoneModel,
  CampaignSessionModel,
  CampaignSessionSocketService,
  getCampaignMilestoneImportanceLabel,
  getCampaignMilestoneImportanceSlug,
  ImportantChoiceSessionNoteRequest,
  LevelUpOrMechanicsChangeSessionNoteRequest,
  PendingChangesComponent,
  PresentationModeSocketService,
  PresentationModeStoryBlockModel,
  SessionNoteChoiceModel,
  SessionNoteMechanicsChangeModel,
  SessionNoteMechanicsChangeRequest,
  SessionNoteModel,
  SessionNoteStoryBeatReferenceType,
  SessionNoteStoryBeatReferenceTypeValue,
  SessionNoteType,
  Skill,
  SkillValue,
  Ability,
  AbilityValue,
  StoryBeatModel,
  StoryBeatDecisionOptionModel,
  StoryBeatOptionalInformationModel,
  StoryBeatOptionalInformationPlacement,
  StoryBeatRoleplayingCheckType,
  StoryBeatRoleplayingInformationModel,
  StoryBeatRoleplayingNpcModel,
  StoryBeatType,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

interface NoteContextMenuState {
  note: SessionNoteModel;
  x: number;
  y: number;
}

interface ImportantChoiceDraft {
  draftId: number;
  choiceText: string;
  isChosen: boolean;
}

interface MechanicsChangeDraft {
  selectedChange: string;
  customText: string;
}

interface RoleplayingTextPart {
  text: string;
  className: string | null;
}

interface DecisionNoteOption {
  id: string;
  title: string;
  description: string;
  isSelected: boolean;
}

interface DecisionNoteContent {
  type: 'storyBeatDecision';
  selectedDecisionId: string;
  decisions: DecisionNoteOption[];
}

interface OptionalInformationSkillAccess {
  playerName: string;
  skillLabel: string;
  skillValue: number;
  proficiencyLabel: string;
  difficultyClass: number;
}

type InformationNarrativePart =
  | { kind: 'text'; text: string }
  | { kind: 'optionalInformation'; information: StoryBeatOptionalInformationModel };

@Component({
  selector: 'app-campaign-session',
  imports: [LucideArrowLeft, LucideCalendarDays, LucideCirclePlay, LucideMap, LucidePackage],
  templateUrl: './campaign-session.html',
  styleUrl: './campaign-session.css',
})
export class CampaignSession implements OnInit, OnDestroy, PendingChangesComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly campaignSessionSocket = inject(CampaignSessionSocketService);
  private readonly presentationModeSocket = inject(PresentationModeSocketService);
  private readonly modalHelper = inject(ModalHelper);
  private pendingDeactivateResolution: ((canDeactivate: boolean) => void) | null = null;

  protected readonly sessions = signal<CampaignSessionModel[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isSavingDate = signal(false);
  protected readonly isSavingDescription = signal(false);
  protected readonly isSavingNote = signal(false);
  protected readonly isDeletingNote = signal(false);
  protected readonly isLoadingAvailableMilestones = signal(false);
  protected readonly isLoadingAvailableItems = signal(false);
  protected readonly isLoadingCampaignMembers = signal(false);
  protected readonly isConnectingPresentationMode = signal(false);
  protected readonly isEnablingPresentationMode = signal(false);
  protected readonly isDisablingPresentationMode = signal(false);
  protected readonly presentingStoryBeatId = signal<string | null>(null);
  protected readonly finishingStoryBeatId = signal<string | null>(null);
  protected readonly isDatePickerOpen = signal(false);
  protected readonly selectedDate = signal('');
  protected readonly isNoteTypeDialogOpen = signal(false);
  protected readonly isNoteEditorOpen = signal(false);
  protected readonly selectedNoteType = signal<SessionNoteType | null>(null);
  protected readonly noteDraft = signal('');
  protected readonly availableMilestones = signal<CampaignMilestoneModel[]>([]);
  protected readonly availableItems = signal<AssetModel[]>([]);
  protected readonly campaignMembers = signal<CampaignMemberInformationModel[]>([]);
  protected readonly selectedMilestoneId = signal<number | null>(null);
  protected readonly selectedItemFoundAssetId = signal<number | null>(null);
  protected readonly importantChoiceDrafts = signal<ImportantChoiceDraft[]>([]);
  protected readonly mechanicsChangeDrafts = signal<Record<string, MechanicsChangeDraft>>({});
  protected readonly descriptionDraft = signal('');
  protected readonly savedDescription = signal('');
  protected readonly isUnsavedChangesDialogOpen = signal(false);
  protected readonly noteContextMenu = signal<NoteContextMenuState | null>(null);
  protected readonly editingNote = signal<SessionNoteModel | null>(null);
  protected readonly deleteConfirmationNote = signal<SessionNoteModel | null>(null);
  protected readonly roleplayingChecklist = signal<Record<string, boolean>>({});
  protected readonly isPresentationModeVisible = signal(false);
  protected readonly isSessionPlayersPanelOpen = signal(false);
  protected readonly markingRoleplayingInformationIds = signal<Set<string>>(new Set());
  protected readonly markingDecisionOptionIds = signal<Set<string>>(new Set());
  private allowNativeContextMenuNoteId: number | null = null;
  private finishStoryBeatTimeout: ReturnType<typeof setTimeout> | null = null;
  protected readonly sessionNumber = computed(() => {
    const sessionNumber = Number(this.route.snapshot.paramMap.get('sessionNumber'));

    return Number.isInteger(sessionNumber) && sessionNumber > 0 ? sessionNumber : null;
  });
  protected readonly campaignId = computed(() => (
    this.route.parent?.snapshot.paramMap.get('campaignId') ?? null
  ));
  protected readonly session = computed(() => {
    const sessionNumber = this.sessionNumber();

    return this.sessions().find((session) => session.sessionNumber === sessionNumber) ?? null;
  });
  protected readonly orderedNotes = computed(() => this.orderNotes(this.campaignSessionSocket.sessionNotes()));
  protected readonly sessionPlayers = computed(() => {
    const socketPlayers = this.campaignSessionSocket.sessionPlayers();

    return socketPlayers.length > 0
      ? socketPlayers
      : this.session()?.players ?? this.campaignMembers();
  });
  protected readonly abilityColumns = [
    Ability.STRENGTH,
    Ability.DEXTERITY,
    Ability.CONSTITUTION,
    Ability.INTELLIGENCE,
    Ability.CHARISMA,
    Ability.WISDOM,
  ];
  protected readonly skillColumns = [
    Skill.Acrobatics,
    Skill.AnimalHandling,
    Skill.Arcana,
    Skill.Athletics,
    Skill.Deception,
    Skill.History,
    Skill.Insight,
    Skill.Intimidation,
    Skill.Investigation,
    Skill.Medicine,
    Skill.Nature,
    Skill.Perception,
    Skill.Performance,
    Skill.Persuasion,
    Skill.Religion,
    Skill.SleightOfHand,
    Skill.Stealth,
    Skill.Survival,
  ];
  protected readonly playedStoryBeatIds = computed(() => new Set(
    this.orderedNotes()
      .filter((note) => this.isFullStoryBeatPlayedNote(note))
      .map((note) => note.storyBeatId)
      .filter((storyBeatId): storyBeatId is string => Boolean(storyBeatId)),
  ));
  protected readonly presentationWorkspace = computed(() => (
    this.isPresentationModeVisible() ? this.presentationModeSocket.workspace() : null
  ));
  protected readonly presentationStoryBlocks = computed(() => this.presentationWorkspace()?.storyBlocks ?? []);
  protected readonly availablePresentationStoryBlocks = computed(() => {
    const playedStoryBeatIds = this.playedStoryBeatIds();

    return this.presentationStoryBlocks().map((block) => ({
      ...block,
      storyBeats: block.storyBeats.filter((storyBeat) => !playedStoryBeatIds.has(storyBeat.storyBeatId)),
      storyBeatQuestTaskLinks: block.storyBeatQuestTaskLinks
        .filter((link) => !playedStoryBeatIds.has(link.storyBeatId)),
    }));
  });
  protected readonly currentPresentationStoryBeatId = computed(() => (
    this.presentationWorkspace()?.presentation.currentStoryBeatId ?? null
  ));
  protected readonly activePresentationStoryBlockId = computed(() => (
    this.presentationWorkspace()?.presentation.activeStoryBlockId ?? null
  ));
  protected readonly currentPresentationStoryBlock = computed(() => {
    const storyBeatId = this.currentPresentationStoryBeatId();

    if (!storyBeatId) {
      return null;
    }

    return this.presentationStoryBlocks()
      .find((block) => block.storyBeats.some((storyBeat) => storyBeat.storyBeatId === storyBeatId)) ?? null;
  });
  protected readonly currentPresentationStoryBeat = computed(() => {
    const storyBeatId = this.currentPresentationStoryBeatId();

    if (!storyBeatId) {
      return null;
    }

    return this.presentationStoryBlocks()
      .flatMap((block) => block.storyBeats)
      .find((storyBeat) => storyBeat.storyBeatId === storyBeatId) ?? null;
  });
  protected readonly currentPresentationQuestTaskTitles = computed(() => {
    const storyBeat = this.currentPresentationStoryBeat();

    return storyBeat ? this.storyBeatQuestTaskTitles(storyBeat) : [];
  });
  protected readonly presentationButtonLabel = computed(() => {
    if (this.isEnablingPresentationMode()) {
      return 'Enabling...';
    }

    if (this.isDisablingPresentationMode()) {
      return 'Disabling...';
    }

    return this.presentationWorkspace() ? 'Presentation Active' : 'Enable Presentation Mode';
  });
  protected readonly selectedMechanicsChanges = computed(() => this.toMechanicsChangeRequests());
  protected readonly headerText = computed(() => {
    const session = this.session();

    if (!session) {
      return 'Session';
    }

    const formattedDate = this.formatDisplayDate(session.sessionDate);

    return formattedDate
      ? `Session ${session.sessionNumber} - ${formattedDate}`
      : `Session ${session.sessionNumber}`;
  });
  protected readonly hasUnsavedDescriptionChanges = computed(() => (
    this.normalizeDescription(this.descriptionDraft()) !==
    this.normalizeDescription(this.savedDescription())
  ));
  protected readonly canAddNote = computed(() => (
    (
      this.isMechanicsChangeEditor()
        ? this.selectedMechanicsChanges().length > 0
        : this.normalizeDescription(this.noteDraft()).length > 0
    ) &&
    !this.isSavingNote() &&
    (
      !this.isImportantChoiceEditor() ||
      this.importantChoiceDrafts()
        .filter((choice) => this.normalizeDescription(choice.choiceText).length > 0)
        .length >= 2
    ) &&
    (
      !this.isCampaignMilestonePicker() ||
      this.selectedMilestoneId() !== null
    ) &&
    (
      !this.isItemFoundPicker() ||
      this.selectedItemFoundAssetId() !== null
    ) &&
    (
      !this.isMechanicsChangeEditor() ||
      this.selectedMechanicsChanges().length > 0
    )
  ));
  protected readonly isImportantChoiceEditor = computed(() => (
    this.selectedNoteType() === SessionNoteType.ImportantChoice
  ));
  protected readonly isCampaignMilestoneEditor = computed(() => (
    this.selectedNoteType() === SessionNoteType.CampaignMilestone
  ));
  protected readonly isCampaignMilestonePicker = computed(() => (
    this.isCampaignMilestoneEditor() && !this.editingNote()
  ));
  protected readonly isItemFoundEditor = computed(() => (
    this.selectedNoteType() === SessionNoteType.ItemFound
  ));
  protected readonly isItemFoundPicker = computed(() => (
    this.isItemFoundEditor() && !this.editingNote()
  ));
  protected readonly isMechanicsChangeEditor = computed(() => (
    this.selectedNoteType() === SessionNoteType.LevelUpOrMechanicsChange
  ));
  protected readonly noteEditorActionText = computed(() => {
    if (this.isSavingNote()) {
      return this.editingNote() ? 'Saving...' : 'Adding...';
    }

    return this.editingNote() ? 'Save Note' : 'Add Note';
  });
  protected readonly noteTypeOptions = [
    {
      type: SessionNoteType.GeneralNotes,
      label: 'Generic Type',
    },
    {
      type: SessionNoteType.ImportantChoice,
      label: 'Important Choice',
    },
    {
      type: SessionNoteType.CampaignMilestone,
      label: 'Campaign Milestone',
    },
    {
      type: SessionNoteType.ItemFound,
      label: 'Item Found',
    },
    {
      type: SessionNoteType.LevelUpOrMechanicsChange,
      label: 'Level Up / Mechanics Change',
    },
  ];
  protected readonly mechanicsChangeOtherOption = 'Other';
  protected readonly levelUpMechanicsOptions = Array.from(
    { length: 19 },
    (_, index) => `Level Up ${index + 1} -> ${index + 2}`,
  );
  private nextChoiceDraftId = 1;
  private readonly syncFinishedStoryBeatSession = effect(() => {
    const played = this.presentationModeSocket.storyBeatPlayed();

    if (!played?.session) {
      return;
    }

    this.upsertSession(played.session);
    this.setSessionNotes(played.session.id, played.session.notes ?? []);
  });
  private readonly syncMarkedStoryBeatReferenceSession = effect(() => {
    const marked = this.presentationModeSocket.storyBeatReferenceMarked();

    if (!marked?.session) {
      return;
    }

    this.upsertSession(marked.session);
    this.setSessionNotes(marked.session.id, marked.session.notes ?? []);
  });
  private readonly clearFinishedStoryBeatState = effect(() => {
    const finishingStoryBeatId = this.finishingStoryBeatId();

    if (!finishingStoryBeatId) {
      return;
    }

    const hasFinishedNote = this.orderedNotes().some((note) => (
      this.isFullStoryBeatPlayedNote(note) &&
      note.storyBeatId === finishingStoryBeatId
    ));

    if (hasFinishedNote) {
      this.clearFinishingStoryBeat(finishingStoryBeatId);
    }
  });

  ngOnInit(): void {
    this.loadSession();
  }

  ngOnDestroy(): void {
    this.clearFinishingStoryBeat();
    void this.campaignSessionSocket.disconnect();
    void this.presentationModeSocket.disconnect();
  }

  canDeactivate(): boolean | Promise<boolean> {
    if (!this.hasUnsavedDescriptionChanges()) {
      return true;
    }

    this.isUnsavedChangesDialogOpen.set(true);

    return new Promise((resolve) => {
      this.pendingDeactivateResolution = resolve;
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  protected warnBeforeBrowserUnload(event: BeforeUnloadEvent): void {
    if (!this.hasUnsavedDescriptionChanges()) {
      return;
    }

    event.preventDefault();
    event.returnValue = '';
  }

  @HostListener('document:click')
  protected closeNoteContextMenu(): void {
    this.noteContextMenu.set(null);
  }

  @HostListener('document:keydown.escape')
  protected closeNoteContextMenuWithEscape(): void {
    this.noteContextMenu.set(null);
  }

  protected goToSessionsPage(): void {
    const campaignId = this.campaignId();

    if (!campaignId) {
      return;
    }

    void this.router.navigate(['/campaigns', campaignId, 'campaign-sessions']);
  }

  protected openDatePicker(): void {
    this.selectedDate.set(this.toDateInputValue(this.session()?.sessionDate));
    this.isDatePickerOpen.set(true);
  }

  protected setSelectedDate(event: Event): void {
    this.selectedDate.set((event.target as HTMLInputElement).value);
  }

  protected saveSelectedDate(): void {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || this.isSavingDate()) {
      return;
    }

    this.isSavingDate.set(true);

    this.campaignSessionSocket
      .updateSessionDate(campaignId, session.id, this.toSessionDateValue(this.selectedDate()))
      .then((updatedSession) => {
        this.upsertSession(updatedSession ?? {
          ...session,
          sessionDate: this.toSessionDateValue(this.selectedDate()),
        });
        this.isDatePickerOpen.set(false);
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Session date could not be saved.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isSavingDate.set(false));
  }

  protected enablePresentationMode(storyBlockId: string | null = null): void {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || this.isEnablingPresentationMode()) {
      return;
    }

    this.isEnablingPresentationMode.set(true);

    this.presentationModeSocket
      .enablePresentationMode(campaignId, session.id, { storyBlockId })
      .then((workspace) => {
        if (workspace) {
          this.isPresentationModeVisible.set(true);
        }
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Presentation mode could not be enabled.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isEnablingPresentationMode.set(false));
  }

  protected togglePresentationMode(): void {
    if (this.presentationWorkspace()) {
      this.disablePresentationMode();
      return;
    }

    this.enablePresentationMode();
  }

  protected disablePresentationMode(): void {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || this.isDisablingPresentationMode()) {
      return;
    }

    this.isDisablingPresentationMode.set(true);

    this.presentationModeSocket
      .disablePresentationMode(campaignId, session.id)
      .then(() => this.isPresentationModeVisible.set(false))
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Presentation mode could not be disabled.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isDisablingPresentationMode.set(false));
  }

  protected selectPresentationStoryBlock(block: PresentationModeStoryBlockModel): void {
    const storyBlockId = block.storyBlock.storyBlockId;

    this.loadPresentationStoryBlock(storyBlockId)
      .finally(() => this.enablePresentationMode(storyBlockId));
  }

  protected presentStoryBeat(storyBeat: StoryBeatModel): void {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || this.presentingStoryBeatId()) {
      return;
    }

    this.presentingStoryBeatId.set(storyBeat.storyBeatId);

    this.presentationModeSocket
      .presentStoryBeat(campaignId, session.id, { storyBeatId: storyBeat.storyBeatId })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Story beat could not be presented.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.presentingStoryBeatId.set(null));
  }

  protected finishStoryBeat(storyBeat: StoryBeatModel): void {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || this.finishingStoryBeatId() || this.hasStoryBeatPlayedNote(storyBeat)) {
      return;
    }

    this.finishingStoryBeatId.set(storyBeat.storyBeatId);
    this.startFinishStoryBeatTimeout(storyBeat.storyBeatId);

    this.campaignSessionSocket
      .createStoryBeatPlayedSessionNote(campaignId, session.id, {
        storyBeatId: storyBeat.storyBeatId,
        content: null,
      })
      .catch((error: unknown) => {
        if (!this.isMissingHubMethodError(error)) {
          throw error;
        }

        return this.presentationModeSocket
          .finishStoryBeat(campaignId, session.id, {
            storyBeatId: storyBeat.storyBeatId,
            content: null,
          })
          .then((result) => result?.session ?? null);
      })
      .then((updatedSession) => {
        if (!updatedSession) {
          throw new Error('Story beat finish did not return an updated session.');
        }

        this.upsertSession(updatedSession);
        this.setSessionNotes(updatedSession.id, updatedSession.notes ?? []);
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Story beat could not be marked as played.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.clearFinishingStoryBeat(storyBeat.storyBeatId));
  }

  protected markRoleplayingInformationGiven(
    storyBeat: StoryBeatModel,
    information: StoryBeatRoleplayingInformationModel,
    checklistKey: string,
    checkbox: HTMLInputElement,
  ): void {
    const campaignId = this.getCampaignId();
    const session = this.session();
    const informationId = information.id;

    if (!campaignId || !session || !informationId) {
      checkbox.checked = false;
      this.modalHelper.showError('This roleplaying information cannot be saved because it has no backend id.');
      return;
    }

    if (this.hasRoleplayingInformationReference(storyBeat, information) ||
      this.markingRoleplayingInformationIds().has(informationId)) {
      checkbox.checked = true;
      return;
    }

    this.roleplayingChecklist.update((items) => ({
      ...items,
      [checklistKey]: true,
    }));
    this.markingRoleplayingInformationIds.update((ids) => new Set(ids).add(informationId));

    this.campaignSessionSocket
      .updateStoryBeatReferenceSessionNote(campaignId, session.id, {
        storyBeatId: storyBeat.storyBeatId,
        referenceType: SessionNoteStoryBeatReferenceType.RoleplayingInformation,
        referenceId: informationId,
        isPlayed: true,
        content: null,
      })
      .then((updatedSession) => {
        if (!updatedSession) {
          return;
        }

        this.upsertSession(updatedSession);
      })
      .catch((error: unknown) => {
        checkbox.checked = false;
        this.roleplayingChecklist.update((items) => ({
          ...items,
          [checklistKey]: false,
        }));
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Roleplaying information could not be added to session notes.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => {
        this.markingRoleplayingInformationIds.update((ids) => {
          const updatedIds = new Set(ids);
          updatedIds.delete(informationId);
          return updatedIds;
        });
      });
  }

  protected isActivePresentationStoryBlock(block: PresentationModeStoryBlockModel): boolean {
    return this.activePresentationStoryBlockId() === block.storyBlock.storyBlockId;
  }

  protected isCurrentPresentationStoryBeat(storyBeat: StoryBeatModel): boolean {
    return this.currentPresentationStoryBeatId() === storyBeat.storyBeatId;
  }

  protected hasPresentedStoryBeat(storyBeat: StoryBeatModel): boolean {
    return this.presentationWorkspace()?.presentation.storyBeatSelections
      .some((selection) => selection.selectedStoryBeatId === storyBeat.storyBeatId) ?? false;
  }

  protected storyBeatTypeLabel(storyBeat: StoryBeatModel): string {
    return StoryBeatType[this.toStoryBeatType(storyBeat.storyBeatType)] ?? 'Story Beat';
  }

  protected storyBeatPreview(storyBeat: StoryBeatModel): string {
    const storyBeatType = this.toStoryBeatType(storyBeat.storyBeatType);

    if (storyBeatType === StoryBeatType.Narrative) {
      return storyBeat.narrative?.paragraphs.find((paragraph) => paragraph.trim().length > 0) ?? 'No narrative yet.';
    }

    if (storyBeatType === StoryBeatType.Roleplaying) {
      return storyBeat.roleplaying?.mainDescription || 'No roleplaying description yet.';
    }

    if (storyBeatType === StoryBeatType.Decision) {
      return storyBeat.decision?.description || 'No decision description yet.';
    }

    if (storyBeatType === StoryBeatType.Combat) {
      return storyBeat.combat?.description || 'No combat description yet.';
    }

    if (storyBeatType === StoryBeatType.Transition) {
      return storyBeat.transition?.description || 'No transition description yet.';
    }

    if (storyBeatType === StoryBeatType.Milestone) {
      return storyBeat.milestone?.description || 'No milestone description yet.';
    }

    return storyBeat.information?.narrative || 'No information yet.';
  }

  protected roleplayingPreviewParts(storyBeat: StoryBeatModel): RoleplayingTextPart[] {
    return this.toRoleplayingPreviewParts(storyBeat.roleplaying?.mainDescription ?? '', storyBeat);
  }

  protected informationNarrativeParts(storyBeat: StoryBeatModel): InformationNarrativePart[] {
    return this.toInformationNarrativeParts(
      storyBeat.information?.narrative ?? '',
      storyBeat.information?.optionalInformation ?? [],
    );
  }

  protected roleplayingInformationNpcLabel(
    storyBeat: StoryBeatModel,
    information: StoryBeatRoleplayingInformationModel,
  ): string {
    return this.roleplayingNpcDisplayName(
      storyBeat,
      information.npcTag,
      information.npcName ?? information.npcTag,
    ) || 'Discovery';
  }

  protected roleplayingInformationCheckLabel(
    information: StoryBeatRoleplayingInformationModel,
  ): string {
    const checkType = this.toRoleplayingCheckType(information.checkType);

    if (checkType === StoryBeatRoleplayingCheckType.None) {
      return 'No check';
    }

    const difficultyClass = information.difficultyClass ?? 10;

    if (checkType === StoryBeatRoleplayingCheckType.Ability) {
      return `${this.abilityLabel(information.ability)} DC ${difficultyClass}`;
    }

    return `${this.skillLabel(information.skill)} DC ${difficultyClass}`;
  }

  protected roleplayingChecklistKey(storyBeat: StoryBeatModel, type: string, key: string | undefined | null): string {
    return `${storyBeat.storyBeatId}:${type}:${this.normalizeText(key)}`;
  }

  protected roleplayingInformationChecklistKey(
    storyBeat: StoryBeatModel,
    information: StoryBeatRoleplayingInformationModel,
    index: number,
  ): string {
    return this.roleplayingChecklistKey(
      storyBeat,
      'info',
      `${index}:${information.id ?? ''}:${information.npcTag ?? ''}:${information.information ?? ''}`,
    );
  }

  protected isRoleplayingInformationChecked(
    storyBeat: StoryBeatModel,
    information: StoryBeatRoleplayingInformationModel,
    index: number,
  ): boolean {
    return this.hasRoleplayingInformationReference(storyBeat, information) ||
      this.isRoleplayingChecklistChecked(this.roleplayingInformationChecklistKey(storyBeat, information, index));
  }

  protected isRoleplayingInformationSaving(information: StoryBeatRoleplayingInformationModel): boolean {
    return Boolean(information.id && this.markingRoleplayingInformationIds().has(information.id));
  }

  protected setRoleplayingInformationChecked(
    storyBeat: StoryBeatModel,
    information: StoryBeatRoleplayingInformationModel,
    index: number,
    event: Event,
  ): void {
    const checkbox = event.target as HTMLInputElement;
    const key = this.roleplayingInformationChecklistKey(storyBeat, information, index);

    if (!checkbox.checked) {
      const campaignId = this.getCampaignId();
      const session = this.session();
      const informationId = information.id;
      const hadReference = this.hasRoleplayingInformationReference(storyBeat, information);

      this.roleplayingChecklist.update((items) => ({
        ...items,
        [key]: false,
      }));

      if (!hadReference) {
        return;
      }

      if (!campaignId || !session || !informationId || this.markingRoleplayingInformationIds().has(informationId)) {
        checkbox.checked = true;
        this.roleplayingChecklist.update((items) => ({
          ...items,
          [key]: true,
        }));
        return;
      }

      this.markingRoleplayingInformationIds.update((ids) => new Set(ids).add(informationId));
      this.campaignSessionSocket
        .updateStoryBeatReferenceSessionNote(campaignId, session.id, {
          storyBeatId: storyBeat.storyBeatId,
          referenceType: SessionNoteStoryBeatReferenceType.RoleplayingInformation,
          referenceId: informationId,
          isPlayed: false,
          content: null,
        })
        .then((updatedSession) => {
          if (updatedSession) {
            this.upsertSession(updatedSession);
          }
        })
        .catch((error: unknown) => {
          checkbox.checked = true;
          this.roleplayingChecklist.update((items) => ({
            ...items,
            [key]: true,
          }));
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Roleplaying information could not be removed from session notes.'),
            { statusCode: this.getErrorStatus(error) },
          );
        })
        .finally(() => {
          this.markingRoleplayingInformationIds.update((ids) => {
            const updatedIds = new Set(ids);
            updatedIds.delete(informationId);
            return updatedIds;
          });
        });
      return;
    }

    this.markRoleplayingInformationGiven(storyBeat, information, key, checkbox);
  }

  protected isRoleplayingChecklistChecked(key: string): boolean {
    return this.roleplayingChecklist()[key] ?? false;
  }

  protected setRoleplayingChecklistChecked(key: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;

    this.roleplayingChecklist.update((items) => ({
      ...items,
      [key]: checked,
    }));
  }

  protected isDecisionOptionSelected(
    storyBeat: StoryBeatModel,
    decision: StoryBeatDecisionOptionModel,
  ): boolean {
    return this.findDecisionOptionReferences(storyBeat)
      .some(({ reference }) => reference.referenceId?.toLowerCase() === decision.id.toLowerCase());
  }

  protected isDecisionOptionSaving(decision: StoryBeatDecisionOptionModel): boolean {
    return this.markingDecisionOptionIds().has(decision.id);
  }

  protected chooseDecisionOption(
    storyBeat: StoryBeatModel,
    decision: StoryBeatDecisionOptionModel,
  ): void {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || !storyBeat.decision || !decision.id || this.isDecisionOptionSaving(decision)) {
      return;
    }

    const existingReferences = this.findDecisionOptionReferences(storyBeat)
      .filter(({ reference }) => reference.referenceId?.toLowerCase() !== decision.id.toLowerCase());

    this.markingDecisionOptionIds.update((ids) => new Set(ids).add(decision.id));

    Promise.all(existingReferences.map(({ reference }) => (
      this.campaignSessionSocket.updateStoryBeatReferenceSessionNote(campaignId, session.id, {
        storyBeatId: storyBeat.storyBeatId,
        referenceType: SessionNoteStoryBeatReferenceType.DecisionOption,
        referenceId: reference.referenceId,
        isPlayed: false,
        content: null,
      })
    )))
      .then(() => this.campaignSessionSocket.updateStoryBeatReferenceSessionNote(campaignId, session.id, {
        storyBeatId: storyBeat.storyBeatId,
        referenceType: SessionNoteStoryBeatReferenceType.DecisionOption,
        referenceId: decision.id,
        isPlayed: true,
        content: this.buildDecisionNoteContent(storyBeat, decision.id),
      }))
      .then((updatedSession) => {
        if (updatedSession) {
          this.upsertSession(updatedSession);
          this.setSessionNotes(updatedSession.id, updatedSession.notes ?? []);
        }
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Decision could not be saved to session notes.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => {
        this.markingDecisionOptionIds.update((ids) => {
          const updatedIds = new Set(ids);
          updatedIds.delete(decision.id);
          return updatedIds;
        });
      });
  }

  protected storyBeatQuestTaskTitles(storyBeat: StoryBeatModel): string[] {
    const workspace = this.presentationWorkspace();

    if (!workspace) {
      return [];
    }

    const storyBlockLinks = workspace.storyBlocks
      .find((block) => block.storyBlock.storyBlockId === storyBeat.storyBlockId)
      ?.storyBeatQuestTaskLinks ?? [];
    const linkTitles = [
      ...workspace.storyBeatQuestTaskLinks,
      ...storyBlockLinks,
    ]
      .filter((link) => link.storyBeatId === storyBeat.storyBeatId)
      .map((link) => link.title);

    return Array.from(new Set(linkTitles));
  }

  protected combatRewards(storyBeat: StoryBeatModel): string[] {
    const rewards = storyBeat.combat?.rewards as unknown;

    if (Array.isArray(rewards)) {
      return rewards
        .filter((reward): reward is string => typeof reward === 'string')
        .map((reward) => reward.trim())
        .filter((reward) => reward.length > 0);
    }

    if (typeof rewards === 'string') {
      return rewards
        .split(/\r?\n|,/)
        .map((reward) => reward.trim())
        .filter((reward) => reward.length > 0);
    }

    return [];
  }

  protected hasStoryBeatPlayedNote(storyBeat: StoryBeatModel): boolean {
    return this.orderedNotes().some((note) => (
      this.isFullStoryBeatPlayedNote(note) &&
      note.storyBeatId === storyBeat.storyBeatId
    ));
  }

  protected isStoryBeatPlayedNote(note: SessionNoteModel): boolean {
    return this.toSessionNoteType(note.type) === SessionNoteType.StoryBeatPlayed;
  }

  protected isFullStoryBeatPlayedNote(note: SessionNoteModel): boolean {
    return this.isStoryBeatPlayedNote(note) && (note.storyBeatReferences?.length ?? 0) === 0;
  }

  protected isStoryBeatReferenceNote(note: SessionNoteModel): boolean {
    return this.isStoryBeatPlayedNote(note) && (note.storyBeatReferences?.length ?? 0) > 0;
  }

  protected isDecisionReferenceNote(note: SessionNoteModel): boolean {
    return this.getPrimaryStoryBeatReferenceType(note) === SessionNoteStoryBeatReferenceType.DecisionOption;
  }

  protected canClickEditNote(note: SessionNoteModel): boolean {
    return Boolean(note.storyBeatId);
  }

  protected getStoryBeatPlayedTitle(note: SessionNoteModel): string {
    return note.storyBeat?.title ?? note.content;
  }

  protected getStoryBeatPlayedSubtitle(note: SessionNoteModel): string {
    const referenceType = this.getPrimaryStoryBeatReferenceType(note);
    if (referenceType === SessionNoteStoryBeatReferenceType.RoleplayingInformation) {
      return 'Roleplaying Information';
    }

    if (referenceType === SessionNoteStoryBeatReferenceType.RoleplayingNpcInteraction) {
      return 'Roleplaying NPC Interaction';
    }

    if (referenceType === SessionNoteStoryBeatReferenceType.DecisionOption) {
      return 'Decision Option';
    }

    if (!note.storyBeat) {
      return 'Story Beat Played';
    }

    return `Story Beat ${note.storyBeat.orderIndex + 1}.${note.storyBeat.secondaryOrderIndex + 1} - ${
      this.storyBeatTypeValueLabel(note.storyBeat.storyBeatType)
    }`;
  }

  protected getStoryBeatReferenceText(
    note: SessionNoteModel,
    reference: SessionNoteModel['storyBeatReferences'][number],
  ): string {
    return reference.contentSnapshot || note.content;
  }

  protected decisionNoteOptions(note: SessionNoteModel): DecisionNoteOption[] {
    const parsedContent = this.parseDecisionNoteContent(note.content);

    if (parsedContent) {
      return parsedContent.decisions;
    }

    return note.storyBeatReferences.map((reference) => ({
      id: reference.referenceId ?? `${reference.id}`,
      title: reference.contentSnapshot || 'Decision',
      description: reference.contentSnapshot || note.content,
      isSelected: true,
    }));
  }

  protected decisionNoteOptionTooltip(decision: DecisionNoteOption): string {
    return `${decision.isSelected ? 'Chosen' : 'Not chosen'}: ${decision.title}\n${decision.description}`;
  }

  protected openStoryBeatNoteEditor(note: SessionNoteModel): void {
    if (this.canClickEditNote(note)) {
      this.editNote(note);
    }
  }

  protected setDescriptionDraft(event: Event): void {
    this.descriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected saveDescriptionIfDirty(): void {
    if (this.hasUnsavedDescriptionChanges()) {
      void this.saveDescription();
    }
  }

  protected saveAndLeave(): void {
    void this.saveDescription().then((saved) => {
      if (saved) {
        this.resolvePendingDeactivate(true);
      }
    });
  }

  protected discardAndLeave(): void {
    this.descriptionDraft.set(this.savedDescription());
    this.resolvePendingDeactivate(true);
  }

  protected cancelLeave(): void {
    this.resolvePendingDeactivate(false);
  }

  protected openNoteTypeDialog(): void {
    this.isNoteTypeDialogOpen.set(true);
  }

  protected closeNoteTypeDialog(): void {
    this.isNoteTypeDialogOpen.set(false);
    this.isNoteEditorOpen.set(false);
    this.editingNote.set(null);
    this.noteDraft.set('');
    this.selectedMilestoneId.set(null);
    this.selectedItemFoundAssetId.set(null);
    this.availableMilestones.set([]);
    this.availableItems.set([]);
    this.importantChoiceDrafts.set([]);
    this.mechanicsChangeDrafts.set({});
  }

  protected selectNoteType(noteType: SessionNoteType): void {
    this.selectedNoteType.set(noteType);

    if (
      noteType === SessionNoteType.GeneralNotes ||
      noteType === SessionNoteType.ImportantChoice ||
      noteType === SessionNoteType.CampaignMilestone ||
      noteType === SessionNoteType.ItemFound ||
      noteType === SessionNoteType.LevelUpOrMechanicsChange
    ) {
      this.noteDraft.set('');
      this.selectedMilestoneId.set(null);
      this.selectedItemFoundAssetId.set(null);
      this.importantChoiceDrafts.set([]);
      this.mechanicsChangeDrafts.set({});
      this.isNoteEditorOpen.set(true);

      if (noteType === SessionNoteType.CampaignMilestone) {
        this.loadAvailableMilestones();
      }

      if (noteType === SessionNoteType.ItemFound) {
        this.loadAvailableItems();
      }

      if (noteType === SessionNoteType.LevelUpOrMechanicsChange) {
        this.loadCampaignMembers();
      }

      return;
    }

    this.closeNoteTypeDialog();
  }

  protected setNoteDraft(event: Event): void {
    this.noteDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected isImportantChoiceNote(note: SessionNoteModel): boolean {
    return this.toSessionNoteType(note.type) === SessionNoteType.ImportantChoice;
  }

  protected isCampaignMilestoneNote(note: SessionNoteModel): boolean {
    return this.toSessionNoteType(note.type) === SessionNoteType.CampaignMilestone;
  }

  protected isItemFoundNote(note: SessionNoteModel): boolean {
    return this.toSessionNoteType(note.type) === SessionNoteType.ItemFound;
  }

  protected isMechanicsChangeNote(note: SessionNoteModel): boolean {
    return this.toSessionNoteType(note.type) === SessionNoteType.LevelUpOrMechanicsChange;
  }

  protected addImportantChoiceOption(): void {
    this.importantChoiceDrafts.update((choices) => [
      ...choices,
      {
        draftId: this.nextChoiceDraftId++,
        choiceText: '',
        isChosen: false,
      },
    ]);
  }

  protected setImportantChoiceText(choiceId: number, event: Event): void {
    const choiceText = (event.target as HTMLTextAreaElement).value;

    this.importantChoiceDrafts.update((choices) => choices.map((choice) => (
      choice.draftId === choiceId ? { ...choice, choiceText } : choice
    )));
  }

  protected setImportantChoiceChosen(choiceId: number, event: Event): void {
    const isChosen = (event.target as HTMLInputElement).checked;

    this.importantChoiceDrafts.update((choices) => choices.map((choice) => (
      choice.draftId === choiceId ? { ...choice, isChosen } : choice
    )));
  }

  protected removeImportantChoiceOption(choiceId: number): void {
    this.importantChoiceDrafts.update((choices) => choices.filter((choice) => choice.draftId !== choiceId));
  }

  protected selectCampaignMilestone(milestone: CampaignMilestoneModel): void {
    this.selectedMilestoneId.set(milestone.id);
    this.noteDraft.set(milestone.title);
  }

  protected milestoneImportanceLabel(milestone: CampaignMilestoneModel): string {
    return getCampaignMilestoneImportanceLabel(milestone.importance);
  }

  protected campaignMilestoneImportanceClass(milestone: CampaignMilestoneModel): string {
    return `campaign-milestone-importance-${getCampaignMilestoneImportanceSlug(milestone.importance)}`;
  }

  protected selectItemFoundAsset(item: AssetModel): void {
    this.selectedItemFoundAssetId.set(item.id);
    const itemType = this.getAssetItemTypeLabel(item);

    this.noteDraft.set(item.description ? `${item.name}\n${itemType}\n${item.description}` : `${item.name}\n${itemType}`);
  }

  protected getMemberDisplayName(member: CampaignMemberInformationModel): string {
    const fullName = `${member.firstName} ${member.lastName}`.trim();

    return member.nickname || fullName || member.username;
  }

  protected toggleSessionPlayersPanel(): void {
    this.isSessionPlayersPanelOpen.update((isOpen) => !isOpen);

    if (this.isSessionPlayersPanelOpen()) {
      this.loadSessionPlayers();
    }
  }

  protected abilityColumnLabel(ability: Ability): string {
    return this.abilityLabel(ability);
  }

  protected skillColumnLabel(skill: Skill): string {
    return this.skillLabel(skill);
  }

  protected abilityValue(member: CampaignMemberInformationModel, ability: Ability): string {
    const value = member.abilityValues
      .find((candidate) => this.toAbility(candidate.ability) === ability)
      ?.value;

    return typeof value === 'number' ? `${value}` : '-';
  }

  protected skillValue(member: CampaignMemberInformationModel, skill: Skill): string {
    const value = member.skillValues
      .find((candidate) => this.toSkill(candidate.skill) === skill)
      ?.value;

    return typeof value === 'number' ? `${value}` : '-';
  }

  protected skillProficiencyLevel(member: CampaignMemberInformationModel, skill: Skill): 'none' | 'half' | 'full' | 'expertise' {
    if (this.hasSkill(member.expertiseSkills, skill)) {
      return 'expertise';
    }

    if (this.hasSkill(member.proficientSkills, skill)) {
      return 'full';
    }

    return this.hasSkill(member.halfProficientSkills, skill) ? 'half' : 'none';
  }

  protected optionalInformationQualifiedPlayerCount(information: StoryBeatOptionalInformationModel): number {
    return this.optionalInformationQualifiedPlayers(information).length;
  }

  protected optionalInformationAccessTooltip(information: StoryBeatOptionalInformationModel): string {
    const qualifiedPlayers = this.optionalInformationQualifiedPlayers(information);

    if (qualifiedPlayers.length === 0) {
      return `No player met the hidden check.\nCould have learned: ${information.information}`;
    }

    return qualifiedPlayers
      .map((player) => (
        `${player.playerName}: ${player.skillLabel} ${player.skillValue} (${player.proficiencyLabel}), DC ${player.difficultyClass}`
      ))
      .join('\n');
  }

  protected getMechanicsChangeSelection(member: CampaignMemberInformationModel): string {
    return this.mechanicsChangeDrafts()[member.userId]?.selectedChange ?? '';
  }

  protected isMechanicsChangeOtherSelected(member: CampaignMemberInformationModel): boolean {
    return this.getMechanicsChangeSelection(member) === this.mechanicsChangeOtherOption;
  }

  protected getMechanicsChangeCustomText(member: CampaignMemberInformationModel): string {
    return this.mechanicsChangeDrafts()[member.userId]?.customText ?? '';
  }

  protected setMechanicsChangeOption(member: CampaignMemberInformationModel, event: Event): void {
    const selectedChange = (event.target as HTMLSelectElement).value;

    this.mechanicsChangeDrafts.update((drafts) => {
      const nextDrafts = { ...drafts };

      if (!selectedChange) {
        delete nextDrafts[member.userId];
        return nextDrafts;
      }

      nextDrafts[member.userId] = {
        selectedChange,
        customText: drafts[member.userId]?.customText ?? '',
      };

      return nextDrafts;
    });
    this.syncMechanicsChangeNoteDraft();
  }

  protected setMechanicsChangeCustomText(member: CampaignMemberInformationModel, event: Event): void {
    const customText = (event.target as HTMLTextAreaElement).value;

    this.mechanicsChangeDrafts.update((drafts) => ({
      ...drafts,
      [member.userId]: {
        selectedChange: this.mechanicsChangeOtherOption,
        customText,
      },
    }));
    this.syncMechanicsChangeNoteDraft();
  }

  protected getMechanicsChangePlayerName(playerId: string): string {
    const member = this.campaignMembers().find((campaignMember) => campaignMember.userId === playerId);

    return member ? this.getMemberDisplayName(member) : playerId;
  }

  protected getAssetItemTypeLabel(item: AssetModel): string {
    const itemType = this.toAssetItemType(item.itemType);

    return itemType === null
      ? 'Item'
      : AssetItemType[itemType] ?? 'Item';
  }

  protected getItemFoundTitle(note: SessionNoteModel): string {
    return this.getItemFoundLines(note)[0] ?? note.content;
  }

  protected getItemFoundType(note: SessionNoteModel): string {
    const lines = this.getItemFoundLines(note);

    return lines.length > 2 ? lines[1] : 'Item';
  }

  protected getItemFoundDescription(note: SessionNoteModel): string {
    const lines = this.getItemFoundLines(note);

    return lines.length > 2 ? lines.slice(2).join('\n') : lines.slice(1).join('\n');
  }

  protected addNote(): void {
    const campaignId = this.getCampaignId();
    const session = this.session();
    const content = this.isMechanicsChangeEditor()
      ? this.buildMechanicsChangeContent()
      : this.normalizeDescription(this.noteDraft());
    const editingNote = this.editingNote();

    if (!campaignId || !session || !content || this.isSavingNote()) {
      return;
    }

    this.isSavingNote.set(true);

    const saveNote = this.isCampaignMilestonePicker()
      ? this.saveCampaignMilestoneNote(campaignId, session.id, content)
      : this.isItemFoundPicker()
      ? this.campaignSessionSocket.createItemFoundSessionNote(campaignId, session.id, content)
      : this.isMechanicsChangeEditor()
      ? this.saveMechanicsChangeNote(campaignId, session.id, content, editingNote)
      : this.isImportantChoiceEditor()
      ? this.saveImportantChoiceNote(campaignId, session.id, content, editingNote)
      : editingNote
        ? this.campaignSessionSocket.updateSessionNote(campaignId, session.id, editingNote.id, content)
        : this.campaignSessionSocket.createGenericSessionNote(campaignId, session.id, content);

    saveNote
      .then((updatedSession) => {
        if (updatedSession) {
          this.upsertSession(updatedSession);
        }

        this.closeNoteTypeDialog();
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Session note could not be added.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isSavingNote.set(false));
  }

  protected openNoteContextMenu(event: MouseEvent, note: SessionNoteModel): void {
    if (this.allowNativeContextMenuNoteId === note.id) {
      this.allowNativeContextMenuNoteId = null;
      this.noteContextMenu.set(null);
      return;
    }

    event.preventDefault();
    event.stopPropagation();

    this.noteContextMenu.set({
      note,
      x: Math.max(8, Math.min(event.clientX, window.innerWidth - 190)),
      y: Math.max(8, Math.min(event.clientY, window.innerHeight - 96)),
    });
  }

  protected editNote(note: SessionNoteModel): void {
    this.noteContextMenu.set(null);
    this.editingNote.set(note);
    this.selectedNoteType.set(this.toSessionNoteType(note.type));
    this.noteDraft.set(note.content);
    this.selectedMilestoneId.set(null);
    this.selectedItemFoundAssetId.set(null);
    this.availableMilestones.set([]);
    this.availableItems.set([]);
    this.importantChoiceDrafts.set(this.toImportantChoiceDrafts(note.choices ?? []));
    this.mechanicsChangeDrafts.set(this.toMechanicsChangeDrafts(note.mechanicsChanges ?? []));
    if (this.isMechanicsChangeEditor()) {
      this.loadCampaignMembers();
      this.syncMechanicsChangeNoteDraft();
    }
    this.isNoteTypeDialogOpen.set(true);
    this.isNoteEditorOpen.set(true);
  }

  protected confirmDeleteNote(note: SessionNoteModel): void {
    this.noteContextMenu.set(null);
    this.deleteConfirmationNote.set(note);
  }

  protected cancelDeleteNote(): void {
    if (this.isDeletingNote()) {
      return;
    }

    this.deleteConfirmationNote.set(null);
  }

  protected deleteNote(): void {
    const campaignId = this.getCampaignId();
    const session = this.session();
    const note = this.deleteConfirmationNote();

    if (!campaignId || !session || !note || this.isDeletingNote()) {
      return;
    }

    this.isDeletingNote.set(true);

    this.campaignSessionSocket
      .deleteSessionNote(campaignId, session.id, note.id)
      .then((updatedSession) => {
        if (updatedSession) {
          this.upsertSession(updatedSession);
        }

        this.deleteConfirmationNote.set(null);
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Session note could not be deleted.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isDeletingNote.set(false));
  }

  protected allowMoreNoteOptions(note: SessionNoteModel): void {
    this.allowNativeContextMenuNoteId = note.id;
    this.noteContextMenu.set(null);

    window.setTimeout(() => {
      if (this.allowNativeContextMenuNoteId === note.id) {
        this.allowNativeContextMenuNoteId = null;
      }
    }, 4000);
  }

  private loadSession(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId) {
      this.modalHelper.showError('Campaign id was not found.');
      return;
    }

    this.isLoading.set(true);

    this.campaignApiService
      .fetchCampaignSessions(campaignId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.sessions.set(response.data ?? []);
          this.syncDraftFromSession();
          this.loadCampaignMembers();
          this.connectPresentationMode(campaignId);
          void this.campaignSessionSocket
            .connect(campaignId)
            .then(() => {
              this.loadSessionNotes(campaignId);
              this.loadSessionPlayers();
            })
            .catch((error: unknown) => {
              this.modalHelper.showError(
                this.getErrorMessage(error, 'Campaign session socket could not connect.'),
                { statusCode: this.getErrorStatus(error) },
              );
            });
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign session could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private async saveDescription(): Promise<boolean> {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || this.isSavingDescription()) {
      return false;
    }

    this.isSavingDescription.set(true);

    try {
      const description = this.toNullableDescription(this.descriptionDraft());
      const updatedSession = await this.campaignSessionSocket.updateSessionDescription(
        campaignId,
        session.id,
        description,
      );

      this.upsertSession(updatedSession ?? {
        ...session,
        description,
      });
      this.savedDescription.set(description ?? '');
      this.descriptionDraft.set(description ?? '');

      return true;
    } catch (error: unknown) {
      this.modalHelper.showError(
        this.getErrorMessage(error, 'Session description could not be saved.'),
        { statusCode: this.getErrorStatus(error) },
      );

      return false;
    } finally {
      this.isSavingDescription.set(false);
    }
  }

  private syncDraftFromSession(): void {
    const description = this.session()?.description ?? '';

    this.savedDescription.set(description);
    this.descriptionDraft.set(description);
  }

  private loadSessionNotes(campaignId: string): void {
    const session = this.session();

    if (!session) {
      return;
    }

    void this.campaignSessionSocket
      .getSessionNotes(campaignId, session.id)
      .then((notes) => this.setSessionNotes(session.id, notes))
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Session notes could not be loaded.'),
          { statusCode: this.getErrorStatus(error) },
        );
      });
  }

  private connectPresentationMode(campaignId: string): void {
    const session = this.session();

    if (!session || this.isConnectingPresentationMode()) {
      return;
    }

    this.isConnectingPresentationMode.set(true);

    this.presentationModeSocket
      .connect(campaignId, session.id)
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Presentation mode socket could not connect.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isConnectingPresentationMode.set(false));
  }

  private loadPresentationStoryBlock(storyBlockId: string): Promise<void> {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session) {
      return Promise.resolve();
    }

    return this.presentationModeSocket
      .getPresentationModeStoryBlock(campaignId, session.id, storyBlockId)
      .then(() => undefined)
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Presentation story block could not be loaded.'),
          { statusCode: this.getErrorStatus(error) },
        );
      });
  }

  private loadAvailableMilestones(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingAvailableMilestones()) {
      return;
    }

    this.isLoadingAvailableMilestones.set(true);

    this.campaignApiService
      .fetchUnachievedCampaignMilestones(campaignId)
      .pipe(finalize(() => this.isLoadingAvailableMilestones.set(false)))
      .subscribe({
        next: (response) => {
          this.availableMilestones.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign milestones could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadAvailableItems(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingAvailableItems()) {
      return;
    }

    this.isLoadingAvailableItems.set(true);

    this.campaignApiService
      .fetchAvailableCampaignItems(campaignId)
      .pipe(finalize(() => this.isLoadingAvailableItems.set(false)))
      .subscribe({
        next: (response) => {
          this.availableItems.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign items could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadCampaignMembers(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingCampaignMembers()) {
      return;
    }

    this.isLoadingCampaignMembers.set(true);

    this.campaignApiService
      .fetchCampaignUsernames(campaignId)
      .pipe(finalize(() => this.isLoadingCampaignMembers.set(false)))
      .subscribe({
        next: (response) => {
          this.campaignMembers.set((response.data?.joinedMembers ?? [])
            .filter((member) => Boolean(member.userId)));
          this.syncMechanicsChangeNoteDraft();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign members could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadSessionPlayers(): void {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session) {
      return;
    }

    this.campaignSessionSocket
      .getSessionPlayers(campaignId, session.id)
      .then((players) => {
        if (players.length > 0) {
          this.campaignMembers.set(players.filter((member) => Boolean(member.userId)));
          this.syncMechanicsChangeNoteDraft();
        }
      })
      .catch((error: unknown) => {
        if (!this.isMissingHubMethodError(error)) {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Session players could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        }
      });
  }

  private setSessionNotes(sessionId: number, notes: SessionNoteModel[]): void {
    const session = this.sessions().find((existingSession) => existingSession.id === sessionId);

    if (!session) {
      return;
    }

    this.upsertSession({
      ...session,
      notes: this.orderNotes(notes),
    });
  }

  private upsertSession(session: CampaignSessionModel): void {
    this.sessions.update((sessions) => sessions.map((existingSession) => (
      existingSession.id === session.id ? session : existingSession
    )));
    this.campaignSessionSocket.upsertSession(session);
  }

  private orderNotes(notes: SessionNoteModel[]): SessionNoteModel[] {
    return [...notes].sort((first, second) => (
      first.order - second.order ||
      first.id - second.id
    ));
  }

  private saveImportantChoiceNote(
    campaignId: string,
    sessionId: number,
    content: string,
    editingNote: SessionNoteModel | null,
  ): Promise<CampaignSessionModel | null> {
    const request: ImportantChoiceSessionNoteRequest = {
      content,
      choices: this.importantChoiceDrafts()
        .map((choice) => ({
          choiceText: this.normalizeDescription(choice.choiceText),
          isChosen: choice.isChosen,
        }))
        .filter((choice) => choice.choiceText.length > 0),
    };

    return editingNote
      ? this.campaignSessionSocket.updateImportantChoiceSessionNote(
        campaignId,
        sessionId,
        editingNote.id,
        request,
      )
      : this.campaignSessionSocket.createImportantChoiceSessionNote(
        campaignId,
        sessionId,
        request,
      );
  }

  private saveCampaignMilestoneNote(
    campaignId: string,
    sessionId: number,
    content: string,
  ): Promise<CampaignSessionModel | null> {
    const milestoneId = this.selectedMilestoneId();

    if (!milestoneId) {
      return Promise.resolve(null);
    }

    const request: AchieveCampaignMilestoneRequest = {
      milestoneId,
      content,
    };

    return this.campaignSessionSocket.achieveCampaignMilestone(
      campaignId,
      sessionId,
      request,
    );
  }

  private saveMechanicsChangeNote(
    campaignId: string,
    sessionId: number,
    content: string,
    editingNote: SessionNoteModel | null,
  ): Promise<CampaignSessionModel | null> {
    const request: LevelUpOrMechanicsChangeSessionNoteRequest = {
      content,
      mechanicsChanges: this.selectedMechanicsChanges(),
    };

    return editingNote
      ? this.campaignSessionSocket.updateLevelUpOrMechanicsChangeSessionNote(
        campaignId,
        sessionId,
        editingNote.id,
        request,
      )
      : this.campaignSessionSocket.createLevelUpOrMechanicsChangeSessionNote(
        campaignId,
        sessionId,
        request,
      );
  }

  private toImportantChoiceDrafts(choices: SessionNoteChoiceModel[]): ImportantChoiceDraft[] {
    return [...choices]
      .sort((first, second) => first.order - second.order || first.id - second.id)
      .map((choice) => ({
        draftId: this.nextChoiceDraftId++,
        choiceText: choice.choiceText,
        isChosen: choice.isChosen,
      }));
  }

  private toMechanicsChangeDrafts(changes: SessionNoteMechanicsChangeModel[]): Record<string, MechanicsChangeDraft> {
    return changes.reduce<Record<string, MechanicsChangeDraft>>((drafts, change) => {
      const changeText = change.changeText?.trim() ?? '';
      const isLevelUpOption = this.levelUpMechanicsOptions.includes(changeText);

      return {
        ...drafts,
        [change.playerId]: {
          selectedChange: isLevelUpOption ? changeText : this.mechanicsChangeOtherOption,
          customText: isLevelUpOption ? '' : changeText,
        },
      };
    }, {});
  }

  private toMechanicsChangeRequests(): SessionNoteMechanicsChangeRequest[] {
    return Object.entries(this.mechanicsChangeDrafts())
      .map(([playerId, draft]) => ({
        playerId,
        changeText: draft.selectedChange === this.mechanicsChangeOtherOption
          ? this.normalizeDescription(draft.customText)
          : draft.selectedChange,
      }))
      .filter((change) => change.changeText.length > 0);
  }

  private buildMechanicsChangeContent(): string {
    return this.selectedMechanicsChanges()
      .map((change) => `${this.getMechanicsChangePlayerName(change.playerId)}: ${change.changeText}`)
      .join('\n');
  }

  private syncMechanicsChangeNoteDraft(): void {
    if (this.isMechanicsChangeEditor()) {
      this.noteDraft.set(this.buildMechanicsChangeContent());
    }
  }

  private toSessionNoteType(noteType: SessionNoteModel['type']): SessionNoteType | null {
    if (typeof noteType === 'number') {
      return noteType in SessionNoteType ? noteType as SessionNoteType : null;
    }

    return SessionNoteType[noteType as keyof typeof SessionNoteType] ?? null;
  }

  private toSessionNoteStoryBeatReferenceType(
    referenceType: SessionNoteStoryBeatReferenceTypeValue,
  ): SessionNoteStoryBeatReferenceType | null {
    if (typeof referenceType === 'number') {
      return referenceType in SessionNoteStoryBeatReferenceType
        ? referenceType as SessionNoteStoryBeatReferenceType
        : null;
    }

    return SessionNoteStoryBeatReferenceType[referenceType as keyof typeof SessionNoteStoryBeatReferenceType] ?? null;
  }

  private getPrimaryStoryBeatReferenceType(note: SessionNoteModel): SessionNoteStoryBeatReferenceType | null {
    const reference = note.storyBeatReferences?.[0];

    return reference ? this.toSessionNoteStoryBeatReferenceType(reference.referenceType) : null;
  }

  private startFinishStoryBeatTimeout(storyBeatId: string): void {
    this.clearFinishingStoryBeat();
    this.finishingStoryBeatId.set(storyBeatId);
    this.finishStoryBeatTimeout = setTimeout(() => {
      if (this.finishingStoryBeatId() !== storyBeatId) {
        return;
      }

      this.clearFinishingStoryBeat(storyBeatId);
      this.modalHelper.showError('Story beat finish is taking too long. The note may have been saved; refresh if the state looks stale.');
    }, 15000);
  }

  private clearFinishingStoryBeat(storyBeatId: string | null = null): void {
    if (storyBeatId && this.finishingStoryBeatId() !== storyBeatId) {
      return;
    }

    if (this.finishStoryBeatTimeout) {
      clearTimeout(this.finishStoryBeatTimeout);
      this.finishStoryBeatTimeout = null;
    }

    this.finishingStoryBeatId.set(null);
  }

  private isMissingHubMethodError(error: unknown): boolean {
    const message = error instanceof Error
      ? error.message
      : typeof error === 'string'
        ? error
        : '';

    return message.includes('Method does not exist');
  }

  private buildDecisionNoteContent(storyBeat: StoryBeatModel, selectedDecisionId: string): string {
    const content: DecisionNoteContent = {
      type: 'storyBeatDecision',
      selectedDecisionId,
      decisions: (storyBeat.decision?.decisions ?? []).map((decision) => ({
        id: decision.id,
        title: decision.title || 'Option',
        description: decision.description || '',
        isSelected: decision.id === selectedDecisionId,
      })),
    };

    return JSON.stringify(content);
  }

  private parseDecisionNoteContent(content: string): DecisionNoteContent | null {
    try {
      const value = JSON.parse(content) as Partial<DecisionNoteContent>;

      if (value.type !== 'storyBeatDecision' ||
        typeof value.selectedDecisionId !== 'string' ||
        !Array.isArray(value.decisions)) {
        return null;
      }

      return {
        type: 'storyBeatDecision',
        selectedDecisionId: value.selectedDecisionId,
        decisions: value.decisions
          .filter((decision): decision is DecisionNoteOption => (
            typeof decision === 'object' &&
            decision !== null &&
            typeof decision.id === 'string' &&
            typeof decision.title === 'string' &&
            typeof decision.description === 'string' &&
            typeof decision.isSelected === 'boolean'
          ))
          .map((decision) => ({
            ...decision,
            isSelected: decision.id === value.selectedDecisionId,
          })),
      };
    } catch {
      return null;
    }
  }

  private findDecisionOptionReferences(storyBeat: StoryBeatModel): {
    note: SessionNoteModel;
    reference: SessionNoteModel['storyBeatReferences'][number];
  }[] {
    const storyBeatId = storyBeat.storyBeatId.toLowerCase();

    return this.orderedNotes().flatMap((note) => (
      (note.storyBeatReferences ?? [])
        .filter((reference) => (
          note.storyBeatId?.toLowerCase() === storyBeatId &&
          reference.storyBeatId.toLowerCase() === storyBeatId &&
          this.toSessionNoteStoryBeatReferenceType(reference.referenceType) ===
            SessionNoteStoryBeatReferenceType.DecisionOption
        ))
        .map((reference) => ({ note, reference }))
    ));
  }

  private hasRoleplayingInformationReference(
    storyBeat: StoryBeatModel,
    information: StoryBeatRoleplayingInformationModel,
  ): boolean {
    return this.findRoleplayingInformationReferenceNote(storyBeat, information) !== null;
  }

  private findRoleplayingInformationReferenceNote(
    storyBeat: StoryBeatModel,
    information: StoryBeatRoleplayingInformationModel,
  ): SessionNoteModel | null {
    if (!information.id) {
      return null;
    }

    const storyBeatId = storyBeat.storyBeatId.toLowerCase();
    const informationId = information.id.toLowerCase();

    return this.orderedNotes().find((note) => (
      note.storyBeatId?.toLowerCase() === storyBeatId &&
      (note.storyBeatReferences ?? []).some((reference) => (
        reference.storyBeatId.toLowerCase() === storyBeatId &&
        reference.referenceId?.toLowerCase() === informationId &&
        this.toSessionNoteStoryBeatReferenceType(reference.referenceType) ===
          SessionNoteStoryBeatReferenceType.RoleplayingInformation
      ))
    )) ?? null;
  }

  private getItemFoundLines(note: SessionNoteModel): string[] {
    return note.content
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter((line) => line.length > 0);
  }

  private toAssetItemType(itemType: AssetModel['itemType']): AssetItemType | null {
    if (itemType === null) {
      return null;
    }

    if (typeof itemType === 'number') {
      return itemType in AssetItemType ? itemType as AssetItemType : null;
    }

    return AssetItemType[itemType as keyof typeof AssetItemType] ?? null;
  }

  private toStoryBeatType(storyBeatType: StoryBeatModel['storyBeatType']): StoryBeatType {
    if (typeof storyBeatType === 'number') {
      return storyBeatType in StoryBeatType ? storyBeatType as StoryBeatType : StoryBeatType.Information;
    }

    return StoryBeatType[storyBeatType as keyof typeof StoryBeatType] ?? StoryBeatType.Information;
  }

  private storyBeatTypeValueLabel(storyBeatType: StoryBeatModel['storyBeatType']): string {
    return StoryBeatType[this.toStoryBeatType(storyBeatType)] ?? 'Story Beat';
  }

  private toRoleplayingPreviewParts(value: string, storyBeat: StoryBeatModel): RoleplayingTextPart[] {
    const parts: RoleplayingTextPart[] = [];
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
        text: this.roleplayingNpcDisplayName(storyBeat, match[1], match[2]),
        className: 'campaign-session-roleplaying-npc-token',
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

  private toRoleplayingNpcOptions(value: string): { key: string; name: string }[] {
    const npcOptions: { key: string; name: string }[] = [];
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

  private roleplayingNpcDisplayName(
    storyBeat: StoryBeatModel,
    npcTag: string | null | undefined,
    fallbackName: string | null | undefined,
  ): string {
    const normalizedTag = this.normalizeText(npcTag).toLowerCase();
    const fallback = this.normalizeText(fallbackName);
    const roleplaying = storyBeat.roleplaying;

    if (!roleplaying) {
      return fallback;
    }

    const npc = (roleplaying.npcs ?? []).find((candidate: StoryBeatRoleplayingNpcModel) => (
      candidate.tag.toLowerCase() === normalizedTag ||
      (!normalizedTag && this.normalizeText(candidate.name).toLowerCase() === fallback.toLowerCase())
    ));

    if (npc) {
      return this.normalizeText(npc.name) || this.normalizeText(npc.tag) || fallback;
    }

    return this.toRoleplayingNpcOptions(roleplaying.mainDescription ?? '')
      .find((option) => option.key.toLowerCase() === normalizedTag)?.name ??
      fallback;
  }

  private toRoleplayingCheckType(
    checkType: StoryBeatRoleplayingInformationModel['checkType'],
  ): StoryBeatRoleplayingCheckType {
    if (typeof checkType === 'number') {
      return checkType in StoryBeatRoleplayingCheckType
        ? checkType as StoryBeatRoleplayingCheckType
        : StoryBeatRoleplayingCheckType.None;
    }

    return StoryBeatRoleplayingCheckType[checkType as keyof typeof StoryBeatRoleplayingCheckType] ??
      StoryBeatRoleplayingCheckType.None;
  }

  private skillLabel(skill: SkillValue | null): string {
    const skillValue = this.toSkill(skill);

    return skillValue === null ? 'Skill' : this.formatEnumLabel(Skill[skillValue]);
  }

  private abilityLabel(ability: AbilityValue | null): string {
    const abilityValue = this.toAbility(ability);

    return abilityValue === null ? 'Ability' : this.formatEnumLabel(Ability[abilityValue]);
  }

  private toSkill(skill: SkillValue | null): Skill | null {
    if (skill === null) {
      return null;
    }

    if (typeof skill === 'number') {
      return skill in Skill ? skill as Skill : null;
    }

    const parsedSkill = Number(skill);

    if (Number.isFinite(parsedSkill)) {
      return parsedSkill in Skill ? parsedSkill as Skill : null;
    }

    return Skill[skill as keyof typeof Skill] as Skill | undefined ?? null;
  }

  private hasSkill(skills: SkillValue[] | null | undefined, skill: Skill): boolean {
    return (skills ?? []).some((candidate) => this.toSkill(candidate) === skill);
  }

  private optionalInformationQualifiedPlayers(
    information: StoryBeatOptionalInformationModel,
  ): OptionalInformationSkillAccess[] {
    const skill = this.toSkill(information.skill);

    if (skill === null) {
      return [];
    }

    const difficultyClass = information.difficultyClass ?? 10;
    const skillLabel = this.skillLabel(skill);

    return this.sessionPlayers()
      .map((player) => {
        const skillValue = player.skillValues
          .find((candidate) => this.toSkill(candidate.skill) === skill)
          ?.value;

        if (typeof skillValue !== 'number' || skillValue < difficultyClass) {
          return null;
        }

        return {
          playerName: this.getMemberDisplayName(player),
          skillLabel,
          skillValue,
          proficiencyLabel: this.skillProficiencyLabel(this.skillProficiencyLevel(player, skill)),
          difficultyClass,
        };
      })
      .filter((player): player is OptionalInformationSkillAccess => player !== null);
  }

  private toInformationNarrativeParts(
    narrative: string,
    optionalInformation: StoryBeatOptionalInformationModel[],
  ): InformationNarrativePart[] {
    if (!narrative.trim()) {
      return [{ kind: 'text', text: 'No narrative yet.' }];
    }

    const parts: InformationNarrativePart[] = [];
    const tokenExpression = /\[([A-Za-z ]+)-(\d{1,2}): ([^\]]+)\]/g;
    let lastIndex = 0;

    for (const match of narrative.matchAll(tokenExpression)) {
      const matchIndex = match.index ?? 0;

      if (matchIndex > lastIndex) {
        parts.push({ kind: 'text', text: narrative.slice(lastIndex, matchIndex) });
      }

      parts.push({
        kind: 'optionalInformation',
        information: this.resolveInlineOptionalInformation(match, optionalInformation, matchIndex),
      });

      lastIndex = matchIndex + match[0].length;
    }

    if (lastIndex < narrative.length) {
      parts.push({ kind: 'text', text: narrative.slice(lastIndex) });
    }

    return parts.length > 0 ? parts : [{ kind: 'text', text: narrative }];
  }

  private resolveInlineOptionalInformation(
    match: RegExpMatchArray,
    optionalInformation: StoryBeatOptionalInformationModel[],
    matchIndex: number,
  ): StoryBeatOptionalInformationModel {
    const skill = this.toSkill(match[1]);
    const difficultyClass = Number(match[2]);
    const information = match[3];
    const existingInformation = optionalInformation.find((candidate) => (
      candidate.narrativeOffset === matchIndex
    )) ?? optionalInformation.find((candidate) => (
      this.toSkill(candidate.skill) === skill &&
      candidate.difficultyClass === difficultyClass &&
      candidate.information === information
    ));

    return existingInformation ?? {
      skill: skill ?? match[1],
      difficultyClass,
      information,
      placement: StoryBeatOptionalInformationPlacement.Inline,
      narrativeOffset: matchIndex,
    };
  }

  private skillProficiencyLabel(level: 'none' | 'half' | 'full' | 'expertise'): string {
    switch (level) {
      case 'half':
        return 'Half proficient';
      case 'full':
        return 'Proficient';
      case 'expertise':
        return 'Expertise';
      case 'none':
        return 'No proficiency';
    }
  }

  private toAbility(ability: AbilityValue | null): Ability | null {
    if (ability === null) {
      return null;
    }

    if (typeof ability === 'number') {
      return ability in Ability ? ability as Ability : null;
    }

    return Ability[ability as keyof typeof Ability] as Ability | undefined ?? null;
  }

  private formatEnumLabel(value: string | undefined): string {
    if (!value) {
      return '';
    }

    if (value === value.toUpperCase()) {
      return value.charAt(0) + value.slice(1).toLowerCase();
    }

    return value.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  private getCampaignId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  }

  private resolvePendingDeactivate(canDeactivate: boolean): void {
    this.isUnsavedChangesDialogOpen.set(false);
    this.pendingDeactivateResolution?.(canDeactivate);
    this.pendingDeactivateResolution = null;
  }

  private formatDisplayDate(sessionDate: string | null): string {
    if (!sessionDate) {
      return '';
    }

    const date = new Date(sessionDate);

    if (Number.isNaN(date.getTime())) {
      return '';
    }

    return new Intl.DateTimeFormat('en-GB', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    }).format(date);
  }

  private toDateInputValue(sessionDate: string | null | undefined): string {
    if (!sessionDate) {
      return '';
    }

    const date = new Date(sessionDate);

    return Number.isNaN(date.getTime())
      ? ''
      : date.toISOString().slice(0, 10);
  }

  private toSessionDateValue(date: string): string | null {
    return date ? `${date}T00:00:00.000Z` : null;
  }

  private normalizeDescription(description: string | null | undefined): string {
    return description?.trim() ?? '';
  }

  private normalizeText(value: unknown): string {
    return typeof value === 'string' ? value.trim() : '';
  }

  private toNullableDescription(description: string): string | null {
    const normalizedDescription = this.normalizeDescription(description);

    return normalizedDescription ? normalizedDescription : null;
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
