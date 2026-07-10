import { Injectable, signal } from '@angular/core';

import { AuthResponse } from '../models';

const accessTokenKey = 'learningLab.accessToken';
const tokenTypeKey = 'learningLab.tokenType';
const expiresAtUtcKey = 'learningLab.expiresAtUtc';
const roleClaimType = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const permissionClaimType = 'permission';

type JwtPayloadValue = string | string[] | number | boolean | null | undefined;
type JwtPayload = Record<string, JwtPayloadValue>;

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

  getRoles(): string[] {
    const payload = this.getValidTokenPayload();

    return payload ? this.getClaimValues(payload, roleClaimType, 'role', 'roles') : [];
  }

  getPermissions(): string[] {
    const payload = this.getValidTokenPayload();

    return payload ? this.getClaimValues(payload, permissionClaimType, 'permissions') : [];
  }

  hasAnyRole(...roles: string[]): boolean {
    const normalizedRoles = new Set(roles.map((role) => role.toLowerCase()));

    return this.getRoles().some((role) => normalizedRoles.has(role.toLowerCase()));
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
    const payload = this.getTokenPayload(accessToken);
    const expiration = payload?.['exp'];

    return typeof expiration === 'number' ? expiration * 1000 : null;
  }

  private getValidTokenPayload(): JwtPayload | null {
    this.storageVersion();

    if (!this.hasValidAccessToken()) {
      return null;
    }

    const accessToken = localStorage.getItem(accessTokenKey);

    return accessToken ? this.getTokenPayload(accessToken) : null;
  }

  private getTokenPayload(accessToken: string): JwtPayload | null {
    const tokenParts = accessToken.split('.');

    if (tokenParts.length !== 3) {
      return null;
    }

    try {
      const base64 = tokenParts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/')
        .padEnd(Math.ceil(tokenParts[1].length / 4) * 4, '=');
      return JSON.parse(atob(base64)) as JwtPayload;
    } catch {
      return null;
    }
  }

  private getClaimValues(payload: JwtPayload, ...claimTypes: string[]): string[] {
    return claimTypes.flatMap((claimType) => {
      const value = payload[claimType];

      if (Array.isArray(value)) {
        return value.filter((item): item is string => typeof item === 'string');
      }

      return typeof value === 'string' ? [value] : [];
    });
  }
}
