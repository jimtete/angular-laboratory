import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';

import { ApiError, CampaignApiService } from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

@Component({
  selector: 'app-campaign-settings',
  templateUrl: './campaign-settings.html',
  styleUrl: './campaign-settings.css',
})
export class CampaignSettings implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);
  private hasShownLargeCampaignWarning = false;

  protected readonly maxPlayers = signal(1);
  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);

  ngOnInit(): void {
    this.fetchSettings();
  }

  protected setMaxPlayers(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);

    this.maxPlayers.set(this.clampMaxPlayers(value));
  }

  protected warnAboutLargeCampaignIfNeeded(): void {
    if (this.maxPlayers() >= 6) {
      if (!this.hasShownLargeCampaignWarning) {
        this.modalHelper.showWarning(
          "Campaigns with more than 6 players depend highly on the Master's Experience.",
        );
        this.hasShownLargeCampaignWarning = true;
      }

      return;
    }

    this.hasShownLargeCampaignWarning = false;
  }

  protected saveChanges(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isSaving()) {
      return;
    }

    this.isSaving.set(true);

    this.campaignApiService
      .updateCampaignSettings(campaignId, {
        maxNumberOfPlayers: this.maxPlayers(),
      })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.maxPlayers.set(this.clampMaxPlayers(response.data.maxNumberOfPlayers));
          }

          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign settings could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private fetchSettings(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId) {
      this.modalHelper.showError('Campaign id was not found.');
      return;
    }

    this.isLoading.set(true);

    this.campaignApiService
      .fetchCampaignSettings(campaignId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.maxPlayers.set(this.clampMaxPlayers(response.data.maxNumberOfPlayers));
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign settings could not be loaded.'),
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

  private clampMaxPlayers(value: number): number {
    if (!Number.isFinite(value)) {
      return 1;
    }

    return Math.max(1, Math.min(20, Math.round(value)));
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
