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

  register(request: RegisterUserRequest): Observable<ApiResponse<string>> {
    return this.apiClient.post<ApiResponse<string>, RegisterUserRequest>('/api/auth/register', request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.apiClient.post<AuthResponse, LoginRequest>('/api/auth/login', request).pipe(
      tap((response) => this.tokenStorage.setAuth(response))
    );
  }

  helloWorld(): Observable<ApiResponse<string>> {
    return this.apiClient.get<ApiResponse<string>>('/api/auth/hello-world');
  }

  logout(): void {
    this.tokenStorage.clear();
  }
}
