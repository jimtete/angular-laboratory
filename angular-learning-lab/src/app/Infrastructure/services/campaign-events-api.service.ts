import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ApiResponse,
  CampaignEventModel,
  CampaignEventOptionModel,
  CampaignEventOptionRequest,
  CampaignEventRequest,
} from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class CampaignEventsApiService {
  private readonly apiClient = inject(ApiClient);

  fetchCampaignEvents(campaignId: string): Observable<ApiResponse<CampaignEventModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignEventModel[]>>(
      `/api/campaigns/${campaignId}/event-definitions`,
    );
  }

  createCampaignEvent(
    campaignId: string,
    request: CampaignEventRequest,
  ): Observable<ApiResponse<CampaignEventModel>> {
    return this.apiClient.post<ApiResponse<CampaignEventModel>, CampaignEventRequest>(
      `/api/campaigns/${campaignId}/event-definitions`,
      request,
    );
  }

  updateCampaignEvent(
    campaignId: string,
    eventId: string,
    request: CampaignEventRequest,
  ): Observable<ApiResponse<CampaignEventModel>> {
    return this.apiClient.put<ApiResponse<CampaignEventModel>, CampaignEventRequest>(
      `/api/campaigns/${campaignId}/event-definitions/${eventId}`,
      request,
    );
  }

  deleteCampaignEvent(campaignId: string, eventId: string): Observable<ApiResponse<boolean>> {
    return this.apiClient.delete<ApiResponse<boolean>>(
      `/api/campaigns/${campaignId}/event-definitions/${eventId}`,
    );
  }

  createCampaignEventOption(
    campaignId: string,
    eventDefinitionId: string,
    request: CampaignEventOptionRequest,
  ): Observable<ApiResponse<CampaignEventOptionModel>> {
    return this.apiClient.post<ApiResponse<CampaignEventOptionModel>, CampaignEventOptionRequest>(
      `/api/campaigns/${campaignId}/event-definitions/${eventDefinitionId}/options`,
      request,
    );
  }

  updateCampaignEventOption(
    campaignId: string,
    eventDefinitionId: string,
    optionId: string,
    request: CampaignEventOptionRequest,
  ): Observable<ApiResponse<CampaignEventOptionModel>> {
    return this.apiClient.put<ApiResponse<CampaignEventOptionModel>, CampaignEventOptionRequest>(
      `/api/campaigns/${campaignId}/event-definitions/${eventDefinitionId}/options/${optionId}`,
      request,
    );
  }

  deleteCampaignEventOption(
    campaignId: string,
    eventDefinitionId: string,
    optionId: string,
  ): Observable<ApiResponse<object>> {
    return this.apiClient.delete<ApiResponse<object>>(
      `/api/campaigns/${campaignId}/event-definitions/${eventDefinitionId}/options/${optionId}`,
    );
  }
}
