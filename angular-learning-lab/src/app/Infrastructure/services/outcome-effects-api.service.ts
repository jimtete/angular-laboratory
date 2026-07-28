import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse, OutcomeEffectModel, OutcomeEffectRequest } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class OutcomeEffectsApiService {
  private readonly apiClient = inject(ApiClient);

  fetchOutcomeEffects(
    campaignId: string,
    sourceType: string | number,
    sourceId: string,
  ): Observable<ApiResponse<OutcomeEffectModel[]>> {
    return this.apiClient.get<ApiResponse<OutcomeEffectModel[]>>(
      `/api/campaigns/${campaignId}/outcome-effects`,
      { params: { sourceType, sourceId } },
    );
  }

  createOutcomeEffect(
    campaignId: string,
    request: OutcomeEffectRequest,
  ): Observable<ApiResponse<OutcomeEffectModel>> {
    return this.apiClient.post<ApiResponse<OutcomeEffectModel>, OutcomeEffectRequest>(
      `/api/campaigns/${campaignId}/outcome-effects`,
      request,
    );
  }

  deleteOutcomeEffect(campaignId: string, outcomeEffectId: string): Observable<ApiResponse<boolean>> {
    return this.apiClient.delete<ApiResponse<boolean>>(
      `/api/campaigns/${campaignId}/outcome-effects/${outcomeEffectId}`,
    );
  }
}
