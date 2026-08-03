import { Injectable, signal } from '@angular/core';

export type BrowserCacheStorageArea = 'local' | 'session';

export interface BrowserCacheSetOptions {
  ttlMs?: number;
  storageArea?: BrowserCacheStorageArea;
}

export interface BrowserCacheGetOptions {
  storageArea?: BrowserCacheStorageArea;
}

interface BrowserCacheEntry<T> {
  value: T;
  createdAtUtc: number;
  expiresAtUtc: number | null;
}

const browserCacheKeyPrefix = 'learningLab.cache.';

@Injectable({
  providedIn: 'root',
})
export class BrowserCacheService {
  private readonly cacheVersion = signal(0);

  get version(): number {
    return this.cacheVersion();
  }

  get<T>(
    key: string,
    fallback: T | null = null,
    options: BrowserCacheGetOptions = {},
  ): T | null {
    this.cacheVersion();

    const entry = this.getEntry<T>(key, options.storageArea ?? 'local');

    return entry ? entry.value : fallback;
  }

  set<T>(
    key: string,
    value: T,
    options: BrowserCacheSetOptions = {},
  ): void {
    const ttlMs = options.ttlMs;
    const now = Date.now();
    const entry: BrowserCacheEntry<T> = {
      value,
      createdAtUtc: now,
      expiresAtUtc: typeof ttlMs === 'number' && ttlMs > 0
        ? now + ttlMs
        : null,
    };

    try {
      this.getStorage(options.storageArea ?? 'local')?.setItem(
        this.toStorageKey(key),
        JSON.stringify(entry),
      );
      this.bumpVersion();
    } catch {
      this.remove(key, options);
    }
  }

  update<T>(
    key: string,
    updater: (currentValue: T | null) => T,
    options: BrowserCacheSetOptions = {},
  ): T {
    const nextValue = updater(this.get<T>(key, null, options));

    this.set(key, nextValue, options);

    return nextValue;
  }

  has(key: string, options: BrowserCacheGetOptions = {}): boolean {
    return this.getEntry<unknown>(key, options.storageArea ?? 'local') !== null;
  }

  remove(key: string, options: BrowserCacheGetOptions = {}): void {
    try {
      this.getStorage(options.storageArea ?? 'local')?.removeItem(this.toStorageKey(key));
      this.bumpVersion();
    } catch {
      this.bumpVersion();
    }
  }

  clearNamespace(namespace = ''): void {
    this.clearStorageNamespace('local', namespace);
    this.clearStorageNamespace('session', namespace);
    this.bumpVersion();
  }

  clearExpired(): void {
    this.clearExpiredFromStorage('local');
    this.clearExpiredFromStorage('session');
    this.bumpVersion();
  }

  private getEntry<T>(
    key: string,
    storageArea: BrowserCacheStorageArea,
  ): BrowserCacheEntry<T> | null {
    const storage = this.getStorage(storageArea);

    if (!storage) {
      return null;
    }

    const storageKey = this.toStorageKey(key);
    const rawEntry = storage.getItem(storageKey);

    if (!rawEntry) {
      return null;
    }

    try {
      const entry = JSON.parse(rawEntry) as BrowserCacheEntry<T>;

      if (!this.isEntry(entry)) {
        storage.removeItem(storageKey);
        this.bumpVersion();
        return null;
      }

      if (this.isExpired(entry)) {
        storage.removeItem(storageKey);
        this.bumpVersion();
        return null;
      }

      return entry;
    } catch {
      storage.removeItem(storageKey);
      this.bumpVersion();
      return null;
    }
  }

  private clearStorageNamespace(
    storageArea: BrowserCacheStorageArea,
    namespace: string,
  ): void {
    const storage = this.getStorage(storageArea);

    if (!storage) {
      return;
    }

    const namespacePrefix = this.toStorageKey(namespace);

    for (const key of this.getStorageKeys(storage)) {
      if (key.startsWith(namespacePrefix)) {
        storage.removeItem(key);
      }
    }
  }

  private clearExpiredFromStorage(storageArea: BrowserCacheStorageArea): void {
    const storage = this.getStorage(storageArea);

    if (!storage) {
      return;
    }

    for (const key of this.getStorageKeys(storage)) {
      if (!key.startsWith(browserCacheKeyPrefix)) {
        continue;
      }

      const rawEntry = storage.getItem(key);

      if (!rawEntry) {
        continue;
      }

      try {
        const entry = JSON.parse(rawEntry) as BrowserCacheEntry<unknown>;

        if (!this.isEntry(entry) || this.isExpired(entry)) {
          storage.removeItem(key);
        }
      } catch {
        storage.removeItem(key);
      }
    }
  }

  private getStorage(storageArea: BrowserCacheStorageArea): Storage | null {
    try {
      return storageArea === 'session' ? sessionStorage : localStorage;
    } catch {
      return null;
    }
  }

  private getStorageKeys(storage: Storage): string[] {
    return Array.from({ length: storage.length }, (_, index) => storage.key(index))
      .filter((key): key is string => Boolean(key));
  }

  private toStorageKey(key: string): string {
    return `${browserCacheKeyPrefix}${key.trim()}`;
  }

  private isEntry<T>(value: unknown): value is BrowserCacheEntry<T> {
    return typeof value === 'object'
      && value !== null
      && 'value' in value
      && 'createdAtUtc' in value
      && typeof value.createdAtUtc === 'number'
      && 'expiresAtUtc' in value
      && (value.expiresAtUtc === null || typeof value.expiresAtUtc === 'number');
  }

  private isExpired(entry: BrowserCacheEntry<unknown>): boolean {
    return entry.expiresAtUtc !== null && entry.expiresAtUtc <= Date.now();
  }

  private bumpVersion(): void {
    this.cacheVersion.update((version) => version + 1);
  }
}
