import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ApiResponse,
  CreateMonsterRequest,
  MonsterFeatureModel,
  MonsterFeatureRequest,
  MonsterListModel,
  MonsterModel,
  MonsterSpellcastingModel,
  MonsterSpellcastingRequest,
  ReorderMonsterFeaturesRequest,
  UpdateMonsterBasicInformationRequest,
  UpdateRemainingSpellSlotsRequest,
} from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class MonsterApiService {
  private readonly apiClient = inject(ApiClient);

  fetchMonsters(): Observable<ApiResponse<MonsterListModel[]>> {
    return this.apiClient.get<ApiResponse<MonsterListModel[]>>('/api/monsters');
  }

  fetchMonster(monsterId: number): Observable<ApiResponse<MonsterModel>> {
    return this.apiClient.get<ApiResponse<MonsterModel>>(`/api/monsters/${monsterId}`);
  }

  createMonster(request: CreateMonsterRequest): Observable<ApiResponse<MonsterModel>> {
    return this.apiClient.post<ApiResponse<MonsterModel>, CreateMonsterRequest>(
      '/api/monsters',
      request,
    );
  }

  updateMonsterBasicInformation(
    monsterId: number,
    request: UpdateMonsterBasicInformationRequest,
  ): Observable<ApiResponse<MonsterModel>> {
    return this.apiClient.put<ApiResponse<MonsterModel>, UpdateMonsterBasicInformationRequest>(
      `/api/monsters/${monsterId}/basic-information`,
      request,
    );
  }

  deleteMonster(monsterId: number): Observable<ApiResponse<boolean>> {
    return this.apiClient.delete<ApiResponse<boolean>>(`/api/monsters/${monsterId}`);
  }

  addMonsterFeature(
    monsterId: number,
    request: MonsterFeatureRequest,
  ): Observable<ApiResponse<MonsterFeatureModel>> {
    return this.apiClient.post<ApiResponse<MonsterFeatureModel>, MonsterFeatureRequest>(
      `/api/monsters/${monsterId}/features`,
      request,
    );
  }

  updateMonsterFeature(
    monsterId: number,
    featureId: number,
    request: MonsterFeatureRequest,
  ): Observable<ApiResponse<MonsterFeatureModel>> {
    return this.apiClient.put<ApiResponse<MonsterFeatureModel>, MonsterFeatureRequest>(
      `/api/monsters/${monsterId}/features/${featureId}`,
      request,
    );
  }

  deleteMonsterFeature(monsterId: number, featureId: number): Observable<ApiResponse<boolean>> {
    return this.apiClient.delete<ApiResponse<boolean>>(
      `/api/monsters/${monsterId}/features/${featureId}`,
    );
  }

  reorderMonsterFeatures(
    monsterId: number,
    request: ReorderMonsterFeaturesRequest,
  ): Observable<ApiResponse<MonsterFeatureModel[]>> {
    return this.apiClient.put<ApiResponse<MonsterFeatureModel[]>, ReorderMonsterFeaturesRequest>(
      `/api/monsters/${monsterId}/features/reorder`,
      request,
    );
  }

  upsertMonsterSpellcasting(
    monsterId: number,
    request: MonsterSpellcastingRequest,
  ): Observable<ApiResponse<MonsterSpellcastingModel>> {
    return this.apiClient.put<ApiResponse<MonsterSpellcastingModel>, MonsterSpellcastingRequest>(
      `/api/monsters/${monsterId}/spellcasting`,
      request,
    );
  }

  removeMonsterSpellcasting(monsterId: number): Observable<ApiResponse<boolean>> {
    return this.apiClient.delete<ApiResponse<boolean>>(`/api/monsters/${monsterId}/spellcasting`);
  }

  updateRemainingSpellSlots(
    monsterId: number,
    spellLevel: number,
    request: UpdateRemainingSpellSlotsRequest,
  ): Observable<ApiResponse<MonsterSpellcastingModel>> {
    return this.apiClient.patch<ApiResponse<MonsterSpellcastingModel>, UpdateRemainingSpellSlotsRequest>(
      `/api/monsters/${monsterId}/spellcasting/slots/${spellLevel}/remaining`,
      request,
    );
  }
}
