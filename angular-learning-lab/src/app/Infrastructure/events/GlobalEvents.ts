import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

import { NotificationModel } from '../models';

@Injectable({
  providedIn: 'root',
})
export class GlobalEvents {
  private readonly successfulLoginSubject = new Subject<void>();
  private readonly successfulLogoutSubject = new Subject<void>();
  private readonly notificationCreatedSubject = new Subject<NotificationModel>();
  private readonly notificationsDeletedSubject = new Subject<string[]>();

  readonly successfulLogin$: Observable<void> = this.successfulLoginSubject.asObservable();
  readonly successfulLogout$: Observable<void> = this.successfulLogoutSubject.asObservable();
  readonly notificationCreated$: Observable<NotificationModel> =
    this.notificationCreatedSubject.asObservable();
  readonly notificationsDeleted$: Observable<string[]> =
    this.notificationsDeletedSubject.asObservable();

  emitSuccessfulLogin(): void {
    this.successfulLoginSubject.next();
  }

  emitSuccessfulLogout(): void {
    this.successfulLogoutSubject.next();
  }

  emitNotificationCreated(notification: NotificationModel): void {
    this.notificationCreatedSubject.next(notification);
  }

  emitNotificationsDeleted(notificationIds: string[]): void {
    this.notificationsDeletedSubject.next(notificationIds);
  }
}
