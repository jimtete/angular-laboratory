import { DestroyRef, Injectable, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  HttpTransportType,
  LogLevel,
} from '@microsoft/signalr';

import { API_BASE_URL } from '../api.config';
import { GlobalEvents } from '../events/GlobalEvents';
import { NotificationModel } from '../models';
import { TokenStorageService } from './token-storage.service';

const notificationCreatedEvent = 'NotificationCreated';
const notificationDeletedEvent = 'NotificationDeleted';

@Injectable({
  providedIn: 'root',
})
export class NotificationSocketService {
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');
  private readonly destroyRef = inject(DestroyRef);
  private readonly globalEvents = inject(GlobalEvents);
  private readonly tokenStorage = inject(TokenStorageService);
  private connection?: HubConnection;

  constructor() {
    this.globalEvents.successfulLogin$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        void this.connect();
      });

    this.globalEvents.successfulLogout$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        void this.disconnect();
      });

    if (this.canConnect()) {
      void this.connect();
    }
  }

  async connect(): Promise<void> {
    if (!this.canConnect()) {
      return;
    }

    if (
      this.connection &&
      this.connection.state !== HubConnectionState.Disconnected
    ) {
      return;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(`${this.apiBaseUrl}/sockets/notifications`, {
        accessTokenFactory: () => this.tokenStorage.getAccessToken() ?? '',
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on(notificationCreatedEvent, (notification: NotificationModel) => {
      this.globalEvents.emitNotificationCreated(notification);
    });

    this.connection.on(notificationDeletedEvent, (notificationIds: string[]) => {
      this.globalEvents.emitNotificationsDeleted(notificationIds);
    });

    try {
      await this.connection.start();
    } catch {
      await this.disconnect();
    }
  }

  async disconnect(): Promise<void> {
    const connection = this.connection;
    this.connection = undefined;

    if (!connection || connection.state === HubConnectionState.Disconnected) {
      return;
    }

    await connection.stop();
  }

  private canConnect(): boolean {
    return (
      this.tokenStorage.hasValidAccessToken() &&
      this.tokenStorage.hasAnyRole('Master', 'Player')
    );
  }
}
