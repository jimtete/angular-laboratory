import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { ApiResponse, AuthResponse, LoginRequest, RegisterUserRequest } from '../models';
import { ApiClient } from './api-client.service';
import { TokenStorageService } from './token-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private readonly apiClient = inject(ApiClient);
  private readonly tokenStorage = inject(TokenStorageService);

  register(request: RegisterUserRequest): Observable<ApiResponse<AuthResponse>> {
    return this.apiClient
      .post<ApiResponse<AuthResponse>, RegisterUserRequest>('/api/auth/register', request)
      .pipe(tap((response) => this.storeAuthentication(response)));
  }

  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.apiClient
      .post<ApiResponse<AuthResponse>, LoginRequest>('/api/auth/login', request)
      .pipe(tap((response) => this.storeAuthentication(response)));
  }

  helloWorld(): Observable<ApiResponse<string>> {
    return this.apiClient.get<ApiResponse<string>>('/api/auth/hello-world');
  }

  logout(): void {
    this.tokenStorage.clear();
  }

  private storeAuthentication(response: ApiResponse<AuthResponse>): void {
    if (!response.data) {
      throw new Error('Authentication response did not contain JWT data.');
    }

    this.tokenStorage.setAuth(response.data);
  }
}
