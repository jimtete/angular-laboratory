import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, finalize, of, tap } from 'rxjs';

import { ApiResponse, CampaignInformationModel, CampaignMemberInformationModel } from '../models';
import { CampaignApiService } from './campaign-api.service';

@Injectable({
  providedIn: 'root',
})
export class CampaignInformationCacheService {
  private readonly campaignApiService = inject(CampaignApiService);
  private loadedCampaignId: string | null = null;

  readonly campaignInformation = signal<CampaignInformationModel | null>(null);
  readonly joinedMembers = computed(
    () => this.campaignInformation()?.joinedMembers ?? [],
  );
  readonly isLoadingInformation = signal(false);

  loadCampaignInformation(
    campaignId: string,
    forceRefresh = false,
  ): Observable<ApiResponse<CampaignInformationModel>> {
    if (this.loadedCampaignId === campaignId && !forceRefresh) {
      return of({
        statusCode: 200,
        message: 'Campaign information loaded from cache.',
        data: this.campaignInformation(),
      });
    }

    this.isLoadingInformation.set(true);

    return this.campaignApiService.fetchCampaignInformation(campaignId).pipe(
      tap((response) => {
        this.campaignInformation.set(response.data);
        this.loadedCampaignId = campaignId;
      }),
      finalize(() => this.isLoadingInformation.set(false)),
    );
  }

  updateJoinedMember(member: CampaignMemberInformationModel): void {
    this.campaignInformation.update((campaignInformation) => {
      if (!campaignInformation) {
        return campaignInformation;
      }

      return {
        ...campaignInformation,
        joinedMembers: campaignInformation.joinedMembers.map((joinedMember) =>
          this.normalizeUsername(joinedMember.username) === this.normalizeUsername(member.username)
            ? member
            : joinedMember,
        ),
      };
    });
  }

  clear(): void {
    this.campaignInformation.set(null);
    this.loadedCampaignId = null;
    this.isLoadingInformation.set(false);
  }

  private normalizeUsername(username: string): string {
    return username.trim().toLowerCase();
  }
}
