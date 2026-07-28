import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';

import {
  ApiError,
  CampaignApiService,
  CampaignCacheService,
  CampaignInformationCacheService,
  CampaignNpcModel,
  CampaignSettingsModel,
  CampaignSessionModel,
  MonsterApiService,
  MonsterModel,
  StoryBlockModel,
  TokenStorageService,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

@Component({
  selector: 'app-campaign-home',
  templateUrl: './campaign-home.html',
  styleUrl: './campaign-home.css',
})
export class CampaignHome implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly monsterApiService = inject(MonsterApiService);
  private readonly campaignCache = inject(CampaignCacheService);
  private readonly campaignInformationCache = inject(CampaignInformationCacheService);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly sessions = signal<CampaignSessionModel[]>([]);
  protected readonly storyBlocks = signal<StoryBlockModel[]>([]);
  protected readonly combatNpcs = signal<MonsterModel[]>([]);
  protected readonly roleplayingNpcs = signal<CampaignNpcModel[]>([]);
  protected readonly campaignSettings = signal<CampaignSettingsModel | null>(null);
  protected readonly isLoadingSessions = signal(false);
  protected readonly isLoadingStoryBlocks = signal(false);
  protected readonly isLoadingNpcs = signal(false);
  protected readonly isLoadingSettings = signal(false);
  protected readonly isCreatingSession = signal(false);
  protected readonly campaignId = computed(() => {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  });
  protected readonly isMaster = computed(() => this.tokenStorage.hasAnyRole('Master'));
  protected readonly campaignTitle = computed(() => {
    const campaignId = this.campaignId();

    return this.campaignCache.campaigns()
      .find((campaign) => campaign.campaignId === campaignId)
      ?.campaignName ?? '';
  });
  protected readonly currentMembers = computed(
    () => this.campaignInformationCache.joinedMembers().length,
  );
  protected readonly storyBlockCount = computed(() => this.storyBlocks().length);
  protected readonly combatNpcCount = computed(() => this.combatNpcs().length);
  protected readonly roleplayingNpcCount = computed(() => this.roleplayingNpcs().length);
  protected readonly npcCount = computed(() => this.combatNpcCount() + this.roleplayingNpcCount());
  protected readonly maxMembers = computed(() => (
    this.campaignSettings()?.maxNumberOfPlayers ?? 1
  ));
  protected readonly membersRingFill = computed(() => {
    const fillPercentage = (this.currentMembers() / this.maxMembers()) * 100;
    const boundedFillPercentage = Math.max(0, Math.min(100, fillPercentage));

    return `${boundedFillPercentage}%`;
  });
  protected readonly latestSession = computed(() => {
    return [...this.sessions()]
      .sort((first, second) => (
        second.sessionNumber - first.sessionNumber ||
        second.id - first.id
      ))[0] ?? null;
  });
  protected readonly latestSessionText = computed(() => {
    const latestSession = this.latestSession();

    return latestSession ? `Session ${latestSession.sessionNumber}` : 'No Sessions';
  });

  ngOnInit(): void {
    this.campaignCache.loadAvailableCampaigns().subscribe({
      error: () => {},
    });

    this.loadSettings();
    this.loadSessions();
    this.loadStoryBlocks();
    this.loadNpcs();
  }

  refreshCampaignPage(): boolean {
    this.loadSettings(true);
    this.loadSessions();
    this.loadStoryBlocks();
    this.loadNpcs();

    return false;
  }

  isRefreshingCampaignPage(): boolean {
    return this.isLoadingSettings() ||
      this.isLoadingSessions() ||
      this.isLoadingStoryBlocks() ||
      this.isLoadingNpcs();
  }

  protected openLatestSession(): void {
    const campaignId = this.campaignId();
    const latestSession = this.latestSession();

    if (!campaignId || !latestSession) {
      return;
    }

    void this.router.navigate([
      '/campaigns',
      campaignId,
      'campaign-sessions',
      latestSession.sessionNumber,
    ]);
  }

  protected goToStoryBlocks(): void {
    const campaignId = this.campaignId();

    if (!campaignId) {
      return;
    }

    void this.router.navigate(['/campaigns', campaignId, 'campaign-content']);
  }

  protected createSession(): void {
    const campaignId = this.campaignId();

    if (!campaignId || this.isCreatingSession()) {
      return;
    }

    this.isCreatingSession.set(true);

    this.campaignApiService
      .createCampaignSession(campaignId)
      .pipe(finalize(() => this.isCreatingSession.set(false)))
      .subscribe({
        next: (response) => {
          const createdSession = response.data;

          if (!createdSession) {
            this.loadSessions();
            return;
          }

          this.sessions.update((sessions) => (
            [...sessions, createdSession]
              .sort((first, second) => first.sessionNumber - second.sessionNumber)
          ));

          void this.router.navigate([
            '/campaigns',
            campaignId,
            'campaign-sessions',
            createdSession.sessionNumber,
          ]);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign session could not be created.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadSessions(): void {
    const campaignId = this.campaignId();

    if (!campaignId || !this.isMaster()) {
      return;
    }

    this.isLoadingSessions.set(true);

    this.campaignApiService
      .fetchCampaignSessions(campaignId)
      .pipe(finalize(() => this.isLoadingSessions.set(false)))
      .subscribe({
        next: (response) => {
          this.sessions.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign sessions could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadStoryBlocks(): void {
    const campaignId = this.campaignId();

    if (!campaignId || !this.isMaster() || this.isLoadingStoryBlocks()) {
      return;
    }

    this.isLoadingStoryBlocks.set(true);

    this.campaignApiService
      .fetchStoryBlocks(campaignId)
      .pipe(finalize(() => this.isLoadingStoryBlocks.set(false)))
      .subscribe({
        next: (response) => {
          this.storyBlocks.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Story blocks could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadNpcs(): void {
    const campaignId = this.campaignId();

    if (!campaignId || !this.isMaster() || this.isLoadingNpcs()) {
      return;
    }

    this.isLoadingNpcs.set(true);

    forkJoin({
      combatNpcs: this.monsterApiService.fetchCampaignMonsterDetails(campaignId),
      roleplayingNpcs: this.campaignApiService.fetchRoleplayingStoryBeatNpcs(campaignId),
    })
      .pipe(finalize(() => this.isLoadingNpcs.set(false)))
      .subscribe({
        next: ({ combatNpcs, roleplayingNpcs }) => {
          this.combatNpcs.set(combatNpcs.data ?? []);
          this.roleplayingNpcs.set(roleplayingNpcs.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign NPC counts could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadSettings(forceRefresh = false): void {
    const campaignId = this.campaignId();

    if (!campaignId || (this.isLoadingSettings() && !forceRefresh)) {
      return;
    }

    this.isLoadingSettings.set(true);

    this.campaignApiService
      .fetchCampaignSettings(campaignId)
      .pipe(finalize(() => this.isLoadingSettings.set(false)))
      .subscribe({
        next: (response) => {
          this.campaignSettings.set(response.data ?? null);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign settings could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return this.isApiError(error) ? error.message : fallback;
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
