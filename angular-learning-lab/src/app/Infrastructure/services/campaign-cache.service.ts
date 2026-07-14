import { Injectable, inject, signal } from '@angular/core';
import { Observable, finalize, of, tap } from 'rxjs';

import { ApiResponse, CampaignModel } from '../models';
import { CampaignApiService } from './campaign-api.service';

@Injectable({
  providedIn: 'root',
})
export class CampaignCacheService {
  private readonly campaignApiService = inject(CampaignApiService);
  private hasLoaded = false;

  readonly campaigns = signal<CampaignModel[]>([]);
  readonly isLoading = signal(false);

  loadAvailableCampaigns(forceRefresh = false): Observable<ApiResponse<CampaignModel[]>> {
    if (this.hasLoaded && !forceRefresh) {
      return of({
        statusCode: 200,
        message: 'Campaigns loaded from cache.',
        data: this.campaigns(),
      });
    }

    this.isLoading.set(true);

    return this.campaignApiService.fetchAvailableCampaigns().pipe(
      tap((response) => {
        this.campaigns.set(response.data ?? []);
        this.hasLoaded = true;
      }),
      finalize(() => this.isLoading.set(false)),
    );
  }

  preloadAvailableCampaigns(): void {
    this.loadAvailableCampaigns().subscribe({
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  clear(): void {
    this.campaigns.set([]);
    this.hasLoaded = false;
    this.isLoading.set(false);
  }
}
