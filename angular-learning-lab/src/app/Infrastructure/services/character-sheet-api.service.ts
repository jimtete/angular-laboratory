import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse, CharacterSheetModel, UpdateCharacterSheetRequest } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class CharacterSheetApiService {
  private readonly apiClient = inject(ApiClient);

  fetchCharacterSheet(): Observable<ApiResponse<CharacterSheetModel>> {
    return this.apiClient.get<ApiResponse<CharacterSheetModel>>('/api/character');
  }

  saveCharacterSheet(
    request: UpdateCharacterSheetRequest,
  ): Observable<ApiResponse<CharacterSheetModel>> {
    return this.apiClient.post<ApiResponse<CharacterSheetModel>, UpdateCharacterSheetRequest>(
      '/api/character',
      request,
    );
  }

  uploadProfilePicture(profilePicture: Blob): Observable<ApiResponse<CharacterSheetModel>> {
    const formData = new FormData();
    formData.append('profilePicture', profilePicture, 'profile-picture.jpg');

    return this.apiClient.post<ApiResponse<CharacterSheetModel>, FormData>(
      '/api/character/profile-picture',
      formData,
    );
  }
}
