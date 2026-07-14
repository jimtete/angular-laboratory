import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ApiResponse,
  CampaignModel,
  CampaignParticipationInviteModel,
  CampaignSettingsModel,
  CreateCampaignParticipationInviteRequest,
  CreateCampaignRequest,
  UpdateCampaignSettingsRequest,
} from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class CampaignApiService {
  private readonly apiClient = inject(ApiClient);

  fetchAvailableCampaigns(): Observable<ApiResponse<CampaignModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignModel[]>>('/api/campaigns');
  }

  createCampaign(
    request: CreateCampaignRequest,
    campaignPicture: Blob | null = null,
  ): Observable<ApiResponse<CampaignModel>> {
    const formData = new FormData();
    formData.append('CampaignName', request.campaignName);
    formData.append('Version', request.version);

    if (campaignPicture) {
      formData.append('campaignPicture', campaignPicture, 'campaign-picture.jpg');
    }

    return this.apiClient.post<ApiResponse<CampaignModel>, FormData>(
      '/api/campaigns',
      formData,
    );
  }

  fetchCampaignSettings(
    campaignId: string,
  ): Observable<ApiResponse<CampaignSettingsModel>> {
    return this.apiClient.get<ApiResponse<CampaignSettingsModel>>(
      `/api/campaigns/${campaignId}/settings`,
    );
  }

  updateCampaignSettings(
    campaignId: string,
    request: UpdateCampaignSettingsRequest,
  ): Observable<ApiResponse<CampaignSettingsModel>> {
    return this.apiClient.put<
      ApiResponse<CampaignSettingsModel>,
      UpdateCampaignSettingsRequest
    >(`/api/campaigns/${campaignId}/settings`, request);
  }

  invitePlayer(
    campaignId: string,
    request: CreateCampaignParticipationInviteRequest,
  ): Observable<ApiResponse<CampaignParticipationInviteModel>> {
    return this.apiClient.post<
      ApiResponse<CampaignParticipationInviteModel>,
      CreateCampaignParticipationInviteRequest
    >(`/api/campaigns/${campaignId}/invites`, request);
  }
}
