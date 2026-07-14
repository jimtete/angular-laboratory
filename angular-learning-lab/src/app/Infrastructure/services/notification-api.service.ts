import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse, NotificationModel } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class NotificationApiService {
  private readonly apiClient = inject(ApiClient);

  fetchAvailableNotifications(): Observable<ApiResponse<NotificationModel[]>> {
    return this.apiClient.get<ApiResponse<NotificationModel[]>>('/api/notifications');
  }
}
