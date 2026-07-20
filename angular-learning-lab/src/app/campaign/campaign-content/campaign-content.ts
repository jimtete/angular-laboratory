import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LucidePencil, LucideTrash2 } from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  ApiError,
  CampaignApiService,
  CampaignMilestoneImportance,
  CampaignMilestoneModel,
  CampaignMilestoneRequest,
  CampaignQuestModel,
  CampaignQuestType,
  CreateCampaignQuestRequest,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

type CampaignContentTab = 'main-story' | 'campaign-milestones' | 'quests';
type QuestCarouselItem = CampaignQuestModel | 'add-quest';
type QuestFormStep = 'details' | 'tasks';

interface QuestTaskDraft {
  draftId: number;
  title: string;
  description: string;
}

@Component({
  selector: 'app-campaign-content',
  imports: [LucidePencil, LucideTrash2],
  templateUrl: './campaign-content.html',
  styleUrl: './campaign-content.css',
})
export class CampaignContent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly selectedTab = signal<CampaignContentTab>('main-story');
  protected readonly milestones = signal<CampaignMilestoneModel[]>([]);
  protected readonly quests = signal<CampaignQuestModel[]>([]);
  protected readonly isLoadingMilestones = signal(false);
  protected readonly isLoadingQuests = signal(false);
  protected readonly isCreateMilestoneDialogOpen = signal(false);
  protected readonly isCreateQuestDialogOpen = signal(false);
  protected readonly isCreatingMilestone = signal(false);
  protected readonly isCreatingQuest = signal(false);
  protected readonly isDeletingMilestone = signal(false);
  protected readonly editingMilestone = signal<CampaignMilestoneModel | null>(null);
  protected readonly deleteConfirmationMilestone = signal<CampaignMilestoneModel | null>(null);
  protected readonly questCarouselIndex = signal(0);
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
  protected readonly canMoveQuestCarousel = computed(() => this.questCarouselItems().length > 1);
  protected readonly canContinueQuestDetails = computed(() => (
    this.normalizeText(this.questTitleDraft()).length > 0 &&
    this.normalizeText(this.questDescriptionDraft()).length > 0 &&
    this.normalizeText(this.questGivenByDraft()).length > 0 &&
    this.normalizeText(this.questRewardDraft()).length > 0 &&
    !this.isCreatingQuest()
  ));
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
  private nextQuestTaskDraftId = 1;

  ngOnInit(): void {
    if (this.selectedTab() === 'campaign-milestones') {
      this.loadMilestones();
    } else if (this.selectedTab() === 'quests') {
      this.loadQuests();
    }
  }

  refreshCampaignPage(): boolean {
    if (this.selectedTab() === 'campaign-milestones') {
      this.loadMilestones();
      return true;
    }

    if (this.selectedTab() === 'quests') {
      this.loadQuests();
      return true;
    }

    return false;
  }

  isRefreshingCampaignPage(): boolean {
    return this.isLoadingMilestones() || this.isLoadingQuests();
  }

  protected selectTab(tab: CampaignContentTab): void {
    this.selectedTab.set(tab);

    if (tab === 'campaign-milestones') {
      this.loadMilestones();
      return;
    }

    if (tab === 'quests') {
      this.loadQuests();
    }
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

  private getCampaignId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  }

  private normalizeText(value: string | null | undefined): string {
    return value?.trim() ?? '';
  }

  private toNullableText(value: string): string | null {
    const normalizedValue = this.normalizeText(value);

    return normalizedValue ? normalizedValue : null;
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

    return CampaignMilestoneImportance[importance as keyof typeof CampaignMilestoneImportance] ??
      CampaignMilestoneImportance.Low;
  }

  private createEmptyQuestTaskDraft(): QuestTaskDraft {
    return {
      draftId: this.nextQuestTaskDraftId++,
      title: '',
      description: '',
    };
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
