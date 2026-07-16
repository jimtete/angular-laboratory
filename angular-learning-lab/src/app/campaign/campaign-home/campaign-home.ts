import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';

import {
  ApiError,
  CampaignApiService,
  CampaignCacheService,
  CampaignInformationCacheService,
  CampaignSessionModel,
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
  private readonly campaignCache = inject(CampaignCacheService);
  private readonly campaignInformationCache = inject(CampaignInformationCacheService);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly sessions = signal<CampaignSessionModel[]>([]);
  protected readonly isLoadingSessions = signal(false);
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
  protected readonly maxMembers = 6;
  protected readonly membersRingFill = computed(() => {
    const fillPercentage = (this.currentMembers() / this.maxMembers) * 100;
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

    this.loadSessions();
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
