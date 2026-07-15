import { Component, input, output } from '@angular/core';

import { NotificationModel, NotificationTypes } from '../../../Infrastructure';

@Component({
  selector: 'app-notification-card',
  templateUrl: './notification-card.html',
  styleUrl: './notification-card.css',
})
export class NotificationCard {
  readonly notification = input.required<NotificationModel>();
  readonly notificationSelected = output<NotificationModel>();

  protected selectNotification(): void {
    this.notificationSelected.emit(this.notification());
  }

  protected get typeLabel(): string {
    switch (this.normalizedNotificationType()) {
      case NotificationTypes.CampaignInvite:
        return 'Campaign Invite';
      case NotificationTypes.Information:
        return 'Information';
      default:
        return 'Notification';
    }
  }

  protected get typeClass(): string {
    switch (this.normalizedNotificationType()) {
      case NotificationTypes.CampaignInvite:
        return 'notification-card-campaign-invite';
      case NotificationTypes.Information:
        return 'notification-card-information';
      default:
        return 'notification-card-default';
    }
  }

  private normalizedNotificationType(): NotificationTypes | null {
    const notificationType = this.notification().notificationType;

    if (typeof notificationType === 'number') {
      return this.toKnownNotificationType(notificationType);
    }

    if (notificationType === 'CampaignInvite') {
      return NotificationTypes.CampaignInvite;
    }

    if (notificationType === 'Information') {
      return NotificationTypes.Information;
    }

    const parsedType = Number(notificationType);

    return this.toKnownNotificationType(parsedType);
  }

  private toKnownNotificationType(notificationType: number): NotificationTypes | null {
    return notificationType === NotificationTypes.CampaignInvite ||
      notificationType === NotificationTypes.Information
      ? notificationType
      : null;
  }
}
