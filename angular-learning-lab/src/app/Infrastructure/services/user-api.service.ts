import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class UserApiService {
  private readonly apiClient = inject(ApiClient);

  fetchPlayerUsernames(): Observable<ApiResponse<string[]>> {
    return this.apiClient.get<ApiResponse<string[]>>('/api/users/players/usernames');
  }
}
