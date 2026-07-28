import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse, CampaignChoiceModel, CampaignChoiceRequest } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class CampaignChoicesApiService {
  private readonly apiClient = inject(ApiClient);

  fetchChoices(
    campaignId: string,
    targetType: string,
    ownerId: string,
  ): Observable<ApiResponse<CampaignChoiceModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignChoiceModel[]>>(
      `/api/campaigns/${campaignId}/choices`,
      { params: { targetType, targetId: ownerId } },
    );
  }

  createChoice(
    campaignId: string,
    request: CampaignChoiceRequest,
  ): Observable<ApiResponse<CampaignChoiceModel>> {
    return this.apiClient.post<ApiResponse<CampaignChoiceModel>, CampaignChoiceRequest>(
      `/api/campaigns/${campaignId}/choices`,
      request,
    );
  }

  updateChoice(
    campaignId: string,
    choiceId: string,
    request: CampaignChoiceRequest,
  ): Observable<ApiResponse<CampaignChoiceModel>> {
    return this.apiClient.put<ApiResponse<CampaignChoiceModel>, CampaignChoiceRequest>(
      `/api/campaigns/${campaignId}/choices/${choiceId}`,
      request,
    );
  }

  deleteChoice(campaignId: string, choiceId: string): Observable<ApiResponse<boolean>> {
    return this.apiClient.delete<ApiResponse<boolean>>(
      `/api/campaigns/${campaignId}/choices/${choiceId}`,
    );
  }
}
