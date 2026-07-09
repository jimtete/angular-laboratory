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
  private expirationRefreshTimer: ReturnType<typeof setTimeout> | undefined;

  constructor() {
    this.scheduleExpirationRefresh();
  }

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

    const responseExpiration = Date.parse(expiresAtUtc);
    const tokenExpiration = this.getTokenExpiration(accessToken);
    const now = Date.now();

    return Number.isFinite(responseExpiration)
      && responseExpiration > now
      && tokenExpiration !== null
      && tokenExpiration > now;
  }

  setAuth(response: AuthResponse): void {
    localStorage.setItem(accessTokenKey, response.accessToken);
    localStorage.setItem(tokenTypeKey, response.tokenType);
    localStorage.setItem(expiresAtUtcKey, response.expiresAtUtc);
    this.storageVersion.update((version) => version + 1);
    this.scheduleExpirationRefresh();
  }

  clear(): void {
    if (this.expirationRefreshTimer) {
      clearTimeout(this.expirationRefreshTimer);
      this.expirationRefreshTimer = undefined;
    }

    localStorage.removeItem(accessTokenKey);
    localStorage.removeItem(tokenTypeKey);
    localStorage.removeItem(expiresAtUtcKey);
    this.storageVersion.update((version) => version + 1);
  }

  private scheduleExpirationRefresh(): void {
    if (this.expirationRefreshTimer) {
      clearTimeout(this.expirationRefreshTimer);
    }

    const accessToken = localStorage.getItem(accessTokenKey);
    const expiresAtUtc = localStorage.getItem(expiresAtUtcKey);

    if (!accessToken || !expiresAtUtc) {
      this.expirationRefreshTimer = undefined;
      return;
    }

    const responseExpiration = Date.parse(expiresAtUtc);
    const tokenExpiration = this.getTokenExpiration(accessToken);

    if (!Number.isFinite(responseExpiration) || tokenExpiration === null) {
      this.expirationRefreshTimer = undefined;
      return;
    }

    const expiration = Math.min(responseExpiration, tokenExpiration);

    if (expiration <= Date.now()) {
      this.expirationRefreshTimer = undefined;
      return;
    }

    const delay = Math.max(0, Math.min(expiration - Date.now() + 50, 2_147_483_647));

    this.expirationRefreshTimer = setTimeout(() => {
      this.storageVersion.update((version) => version + 1);
      this.scheduleExpirationRefresh();
    }, delay);
  }

  private getTokenExpiration(accessToken: string): number | null {
    const tokenParts = accessToken.split('.');

    if (tokenParts.length !== 3) {
      return null;
    }

    try {
      const base64 = tokenParts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/')
        .padEnd(Math.ceil(tokenParts[1].length / 4) * 4, '=');
      const payload = JSON.parse(atob(base64)) as { exp?: unknown };

      return typeof payload.exp === 'number' ? payload.exp * 1000 : null;
    } catch {
      return null;
    }
  }
}
