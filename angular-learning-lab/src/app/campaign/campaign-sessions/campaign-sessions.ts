import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, switchMap, tap } from 'rxjs';

import { ApiError, CampaignApiService, CampaignSessionModel } from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

@Component({
  selector: 'app-campaign-sessions',
  templateUrl: './campaign-sessions.html',
  styleUrl: './campaign-sessions.css',
})
export class CampaignSessions implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly sessions = signal<CampaignSessionModel[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isCreating = signal(false);
  protected readonly hasSessions = computed(() => this.sessions().length > 0);

  ngOnInit(): void {
    this.loadSessions();
  }

  protected openSession(session: CampaignSessionModel): void {
    void this.router.navigate([
      '/campaigns',
      this.getCampaignId(),
      'campaign-sessions',
      session.sessionNumber,
    ]);
  }

  protected createSession(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isCreating()) {
      return;
    }

    let createdSession: CampaignSessionModel | null = null;
    this.isCreating.set(true);

    this.campaignApiService
      .createCampaignSession(campaignId)
      .pipe(
        tap((response) => {
          createdSession = response.data;
        }),
        switchMap(() => this.campaignApiService.fetchCampaignSessions(campaignId)),
        finalize(() => this.isCreating.set(false)),
      )
      .subscribe({
        next: (response) => {
          this.sessions.set(response.data ?? []);

          if (createdSession) {
            void this.router.navigate([
              '/campaigns',
              campaignId,
              'campaign-sessions',
              createdSession.sessionNumber,
            ]);
          }
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
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign sessions could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private getCampaignId(): string | null {
    return (
      this.route.parent?.snapshot.paramMap.get('campaignId') ??
      this.route.snapshot.paramMap.get('campaignId')
    );
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
