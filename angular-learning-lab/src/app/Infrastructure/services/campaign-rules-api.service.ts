import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ApiResponse,
  ConditionalRuleModel,
  ConditionalRuleRequest,
  ConditionalTargetType,
  RuleEvaluationRequest,
  RuleEvaluationResult,
} from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class CampaignRulesApiService {
  private readonly apiClient = inject(ApiClient);

  fetchRules(
    campaignId: string,
    targetType: ConditionalTargetType,
    targetId: string,
  ): Observable<ApiResponse<ConditionalRuleModel[]>> {
    return this.apiClient.get<ApiResponse<ConditionalRuleModel[]>>(
      `/api/campaigns/${campaignId}/rules`,
      { params: { targetType, targetId } },
    );
  }

  createRule(
    campaignId: string,
    request: ConditionalRuleRequest,
  ): Observable<ApiResponse<ConditionalRuleModel>> {
    return this.apiClient.post<ApiResponse<ConditionalRuleModel>, ConditionalRuleRequest>(
      `/api/campaigns/${campaignId}/rules`,
      request,
    );
  }

  updateRule(
    campaignId: string,
    ruleId: string,
    request: ConditionalRuleRequest,
  ): Observable<ApiResponse<ConditionalRuleModel>> {
    return this.apiClient.put<ApiResponse<ConditionalRuleModel>, ConditionalRuleRequest>(
      `/api/campaigns/${campaignId}/rules/${ruleId}`,
      request,
    );
  }

  deleteRule(campaignId: string, ruleId: string): Observable<ApiResponse<boolean>> {
    return this.apiClient.delete<ApiResponse<boolean>>(
      `/api/campaigns/${campaignId}/rules/${ruleId}`,
    );
  }

  evaluateRule(
    campaignSessionId: number,
    request: RuleEvaluationRequest,
  ): Observable<ApiResponse<RuleEvaluationResult>> {
    return this.apiClient.post<ApiResponse<RuleEvaluationResult>, RuleEvaluationRequest>(
      `/api/campaign-sessions/${campaignSessionId}/rules/evaluate`,
      request,
    );
  }
}
