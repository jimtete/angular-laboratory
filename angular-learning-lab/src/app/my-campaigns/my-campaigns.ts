import { Component, OnInit, computed, inject } from '@angular/core';
import { Router } from '@angular/router';

import {
  API_BASE_URL,
  ApiError,
  CampaignCacheService,
  CampaignModel,
  TokenStorageService,
} from '../Infrastructure';
import { ModalHelper } from '../shared/helpers/modal.helper';

@Component({
  selector: 'app-my-campaigns',
  templateUrl: './my-campaigns.html',
  styleUrl: './my-campaigns.css',
})
export class MyCampaigns implements OnInit {
  private readonly campaignCache = inject(CampaignCacheService);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly router = inject(Router);
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');

  protected readonly campaigns = computed(() => this.campaignCache.campaigns());
  protected readonly isLoading = computed(() => this.campaignCache.isLoading());
  protected readonly isMaster = computed(() => this.tokenStorage.hasAnyRole('Master'));

  ngOnInit(): void {
    if (!this.tokenStorage.hasAnyRole('Master', 'Player')) {
      return;
    }

    this.fetchCampaigns();
  }

  private fetchCampaigns(): void {
    this.campaignCache.loadAvailableCampaigns().subscribe({
      next: () => {},
      error: (error: unknown) => {
        this.modalHelper.showError(this.getErrorMessage(error), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  protected createCampaign(): void {
    void this.router.navigate(['/my-campaigns/create-new-campaign']);
  }

  protected selectCampaign(campaignId: string): void {
    void this.router.navigate(['/campaigns', campaignId]);
  }

  protected getCampaignPictureSource(campaign: CampaignModel): string | null {
    const imageSource = campaign.campaignPictureBase64 ?? campaign.campaignPictureUrl;

    if (!imageSource) {
      return null;
    }

    if (/^(https?:\/\/|data:)/i.test(imageSource)) {
      return imageSource;
    }

    if (this.isLikelyBase64Image(imageSource)) {
      const contentType = campaign.campaignPictureContentType ?? 'image/jpeg';

      return `data:${contentType};base64,${imageSource}`;
    }

    return imageSource.startsWith('/')
      ? `${this.apiBaseUrl}${imageSource}`
      : `${this.apiBaseUrl}/${imageSource}`;
  }

  private getErrorStatusCode(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private isLikelyBase64Image(value: string): boolean {
    return value.length > 128 && /^[A-Za-z0-9+/]+={0,2}$/.test(value);
  }

  private getErrorMessage(error: unknown): string {
    return this.isApiError(error) ? error.message : 'Campaigns could not be fetched.';
  }

  private isApiError(error: unknown): error is ApiError {
    return typeof error === 'object'
      && error !== null
      && 'status' in error
      && 'message' in error
      && typeof error.status === 'number'
      && typeof error.message === 'string';
  }
}
