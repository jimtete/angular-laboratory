import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ApiResponse,
  CampaignInformationModel,
  CampaignInviteResolutionModel,
  CampaignMemberInformationModel,
  CampaignModel,
  CampaignPendingInviteModel,
  CampaignParticipationInviteModel,
  CampaignSettingsModel,
  CampaignUsernamesModel,
  CreateCampaignParticipationInviteRequest,
  CreateCampaignRequest,
  UpdateCampaignMemberNicknameRequest,
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

  fetchPendingInvites(): Observable<ApiResponse<CampaignPendingInviteModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignPendingInviteModel[]>>(
      '/api/campaigns/invites',
    );
  }

  fetchCampaignUsernames(
    campaignId: string,
  ): Observable<ApiResponse<CampaignUsernamesModel>> {
    return this.apiClient.get<ApiResponse<CampaignUsernamesModel>>(
      `/api/campaigns/${campaignId}/users`,
    );
  }

  fetchCampaignInformation(
    campaignId: string,
  ): Observable<ApiResponse<CampaignInformationModel>> {
    return this.apiClient.get<ApiResponse<CampaignInformationModel>>(
      `/api/campaigns/${campaignId}/settings/information`,
    );
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

  updateCampaignMemberNickname(
    campaignId: string,
    username: string,
    request: UpdateCampaignMemberNicknameRequest,
  ): Observable<ApiResponse<CampaignMemberInformationModel>> {
    return this.apiClient.put<
      ApiResponse<CampaignMemberInformationModel>,
      UpdateCampaignMemberNicknameRequest
    >(
      `/api/campaigns/${campaignId}/settings/members/${encodeURIComponent(username)}/nickname`,
      request,
    );
  }

  acceptInvite(campaignId: string): Observable<ApiResponse<CampaignInviteResolutionModel>> {
    return this.apiClient.post<ApiResponse<CampaignInviteResolutionModel>, null>(
      `/api/campaigns/${campaignId}/invites/accept`,
      null,
    );
  }

  rejectInvite(campaignId: string): Observable<ApiResponse<CampaignInviteResolutionModel>> {
    return this.apiClient.post<ApiResponse<CampaignInviteResolutionModel>, null>(
      `/api/campaigns/${campaignId}/invites/reject`,
      null,
    );
  }
}
