import { Injectable, inject, signal } from '@angular/core';
import { Observable, finalize, forkJoin, map, of, tap } from 'rxjs';

import { ApiResponse, CampaignModel } from '../models';
import { CampaignApiService } from './campaign-api.service';
import { TokenStorageService } from './token-storage.service';

export type CampaignAccessKind = 'created' | 'joined';

export interface CampaignCardModel extends CampaignModel {
  accessKind: CampaignAccessKind;
}

@Injectable({
  providedIn: 'root',
})
export class CampaignCacheService {
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly tokenStorage = inject(TokenStorageService);
  private hasLoaded = false;

  readonly campaigns = signal<CampaignCardModel[]>([]);
  readonly isLoading = signal(false);

  loadAvailableCampaigns(forceRefresh = false): Observable<ApiResponse<CampaignCardModel[]>> {
    if (this.hasLoaded && !forceRefresh) {
      return of({
        statusCode: 200,
        message: 'Campaigns loaded from cache.',
        data: this.campaigns(),
      });
    }

    this.isLoading.set(true);

    const createdCampaignsRequest = this.campaignApiService.fetchAvailableCampaigns();
    const joinedCampaignsRequest = this.tokenStorage.hasAnyRole('Player')
      ? this.campaignApiService.fetchJoinedCampaigns()
      : of({
        statusCode: 200,
        message: 'Joined campaigns skipped.',
        data: [] as CampaignModel[],
      });

    return forkJoin({
      createdCampaigns: createdCampaignsRequest,
      joinedCampaigns: joinedCampaignsRequest,
    }).pipe(
      map(({ createdCampaigns, joinedCampaigns }) => ({
        statusCode: createdCampaigns.statusCode,
        message: createdCampaigns.message,
        data: this.toCampaignCards(
          createdCampaigns.data ?? [],
          joinedCampaigns.data ?? [],
        ),
      })),
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

  private toCampaignCards(
    createdCampaigns: CampaignModel[],
    joinedCampaigns: CampaignModel[],
  ): CampaignCardModel[] {
    const createdCampaignIds = new Set(createdCampaigns.map((campaign) => campaign.campaignId));
    const cards = [
      ...createdCampaigns.map((campaign) => ({
        ...campaign,
        accessKind: 'created' as const,
      })),
      ...joinedCampaigns
        .filter((campaign) => !createdCampaignIds.has(campaign.campaignId))
        .map((campaign) => ({
          ...campaign,
          accessKind: 'joined' as const,
        })),
    ];

    return cards;
  }
}
