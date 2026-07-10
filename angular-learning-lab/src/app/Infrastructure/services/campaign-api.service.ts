import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse, CampaignModel, CreateCampaignRequest } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class CampaignApiService {
  private readonly apiClient = inject(ApiClient);

  fetchAvailableCampaigns(): Observable<ApiResponse<CampaignModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignModel[]>>('/api/campaigns');
  }

  createCampaign(request: CreateCampaignRequest): Observable<ApiResponse<CampaignModel>> {
    return this.apiClient.post<ApiResponse<CampaignModel>, CreateCampaignRequest>(
      '/api/campaigns',
      request,
    );
  }
}
