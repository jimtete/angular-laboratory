import { Injectable, signal } from '@angular/core';

import { AuthResponse } from '../models';

const accessTokenKey = 'learningLab.accessToken';
const tokenTypeKey = 'learningLab.tokenType';
const expiresAtUtcKey = 'learningLab.expiresAtUtc';

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private readonly storageVersion = signal(0);

  getAccessToken(): string | null {
    this.storageVersion();

    return localStorage.getItem(accessTokenKey);
  }

  hasValidAccessToken(): boolean {
    this.storageVersion();

    const accessToken = localStorage.getItem(accessTokenKey);
    const expiresAtUtc = localStorage.getItem(expiresAtUtcKey);

    if (!accessToken || !expiresAtUtc) {
      return false;
    }

    const expiresAt = Date.parse(expiresAtUtc);

    return Number.isFinite(expiresAt) && expiresAt > Date.now();
  }

  setAuth(response: AuthResponse): void {
    localStorage.setItem(accessTokenKey, response.accessToken);
    localStorage.setItem(tokenTypeKey, response.tokenType);
    localStorage.setItem(expiresAtUtcKey, response.expiresAtUtc);
    this.storageVersion.update((version) => version + 1);
  }

  clear(): void {
    localStorage.removeItem(accessTokenKey);
    localStorage.removeItem(tokenTypeKey);
    localStorage.removeItem(expiresAtUtcKey);
    this.storageVersion.update((version) => version + 1);
  }
}
