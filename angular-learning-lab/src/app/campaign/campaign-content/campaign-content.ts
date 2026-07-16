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
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

type CampaignContentTab = 'main-story' | 'campaign-milestones';

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
  protected readonly isLoadingMilestones = signal(false);
  protected readonly isCreateMilestoneDialogOpen = signal(false);
  protected readonly isCreatingMilestone = signal(false);
  protected readonly isDeletingMilestone = signal(false);
  protected readonly editingMilestone = signal<CampaignMilestoneModel | null>(null);
  protected readonly deleteConfirmationMilestone = signal<CampaignMilestoneModel | null>(null);
  protected readonly milestoneTitleDraft = signal('');
  protected readonly milestoneDescriptionDraft = signal('');
  protected readonly milestoneImportanceDraft = signal<CampaignMilestoneImportance>(
    CampaignMilestoneImportance.Low,
  );
  protected readonly canCreateMilestone = computed(() => (
    this.normalizeText(this.milestoneTitleDraft()).length > 0 &&
    !this.isCreatingMilestone()
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

  ngOnInit(): void {
    if (this.selectedTab() === 'campaign-milestones') {
      this.loadMilestones();
    }
  }

  refreshCampaignPage(): boolean {
    if (this.selectedTab() === 'campaign-milestones') {
      this.loadMilestones();
      return true;
    }

    return false;
  }

  isRefreshingCampaignPage(): boolean {
    return this.isLoadingMilestones();
  }

  protected selectTab(tab: CampaignContentTab): void {
    this.selectedTab.set(tab);

    if (tab === 'campaign-milestones') {
      this.loadMilestones();
    }
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
