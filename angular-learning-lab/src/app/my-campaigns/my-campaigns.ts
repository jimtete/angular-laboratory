import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { ApiError, CampaignApiService, CampaignModel, TokenStorageService } from '../Infrastructure';
import { ModalHelper } from '../shared/helpers/modal.helper';

@Component({
  selector: 'app-my-campaigns',
  templateUrl: './my-campaigns.html',
  styleUrl: './my-campaigns.css',
})
export class MyCampaigns implements OnInit {
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly router = inject(Router);

  protected readonly campaigns = signal<CampaignModel[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isMaster = computed(() => this.tokenStorage.hasAnyRole('Master'));

  ngOnInit(): void {
    if (!this.tokenStorage.hasAnyRole('Master', 'Player')) {
      return;
    }

    this.fetchCampaigns();
  }

  private fetchCampaigns(): void {
    this.isLoading.set(true);
    this.campaignApiService.fetchAvailableCampaigns().subscribe({
      next: (response) => {
        this.campaigns.set(response.data ?? []);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.isLoading.set(false);
        this.modalHelper.showError(this.getErrorMessage(error), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  protected createCampaign(): void {
    void this.router.navigate(['/my-campaigns/create-new-campaign']);
  }

  private getErrorStatusCode(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
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
