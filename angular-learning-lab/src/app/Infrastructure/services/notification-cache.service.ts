import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, finalize, of, tap } from 'rxjs';

import { ApiResponse, NotificationModel } from '../models';
import { GlobalEvents } from '../events/GlobalEvents';
import { NotificationApiService } from './notification-api.service';

@Injectable({
  providedIn: 'root',
})
export class NotificationCacheService {
  private readonly storageKey = 'learning-lab.notifications';
  private readonly globalEvents = inject(GlobalEvents);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationApiService = inject(NotificationApiService);
  private hasLoaded = false;

  readonly notifications = signal<NotificationModel[]>([]);
  readonly isLoading = signal(false);

  constructor() {
    this.restoreFromLocalStorage();

    this.globalEvents.successfulLogin$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.preloadAvailableNotifications();
      });

    this.globalEvents.successfulLogout$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.clear();
      });

    this.globalEvents.notificationCreated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((notification) => {
        this.addNotification(notification);
      });

    this.globalEvents.notificationsDeleted$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((notificationIds) => {
        this.removeNotifications(notificationIds);
      });
  }

  loadAvailableNotifications(forceRefresh = false): Observable<ApiResponse<NotificationModel[]>> {
    if (this.hasLoaded && !forceRefresh) {
      return of({
        statusCode: 200,
        message: 'Notifications loaded from cache.',
        data: this.notifications(),
      });
    }

    this.isLoading.set(true);

    return this.notificationApiService.fetchAvailableNotifications().pipe(
      tap((response) => {
        const notifications = response.data ?? [];

        this.notifications.set(notifications);
        this.hasLoaded = true;
        this.persistToLocalStorage(notifications);
      }),
      finalize(() => this.isLoading.set(false)),
    );
  }

  preloadAvailableNotifications(): void {
    this.loadAvailableNotifications(true).subscribe({
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  clear(): void {
    this.notifications.set([]);
    this.hasLoaded = false;
    this.isLoading.set(false);
    this.removeFromLocalStorage();
  }

  private restoreFromLocalStorage(): void {
    const rawNotifications = localStorage.getItem(this.storageKey);

    if (!rawNotifications) {
      return;
    }

    try {
      const notifications = JSON.parse(rawNotifications) as NotificationModel[];

      if (!Array.isArray(notifications)) {
        this.removeFromLocalStorage();
        return;
      }

      this.notifications.set(notifications);
      this.hasLoaded = true;
    } catch {
      this.removeFromLocalStorage();
    }
  }

  private persistToLocalStorage(notifications: NotificationModel[]): void {
    localStorage.setItem(this.storageKey, JSON.stringify(notifications));
  }

  private addNotification(notification: NotificationModel): void {
    this.notifications.update((notifications) => {
      if (notifications.some((item) => item.notificationId === notification.notificationId)) {
        return notifications;
      }

      const nextNotifications = [notification, ...notifications];
      this.persistToLocalStorage(nextNotifications);
      this.hasLoaded = true;

      return nextNotifications;
    });
  }

  private removeNotifications(notificationIds: string[]): void {
    const notificationIdSet = new Set(notificationIds.map((id) => id.toLowerCase()));

    this.notifications.update((notifications) => {
      const nextNotifications = notifications.filter(
        (notification) => !notificationIdSet.has(notification.notificationId.toLowerCase()),
      );

      if (nextNotifications.length === notifications.length) {
        return notifications;
      }

      this.persistToLocalStorage(nextNotifications);

      return nextNotifications;
    });
  }

  private removeFromLocalStorage(): void {
    localStorage.removeItem(this.storageKey);
  }
}
