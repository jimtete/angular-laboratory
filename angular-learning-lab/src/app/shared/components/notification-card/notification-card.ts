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
      default:
        return 'Notification';
    }
  }

  protected get typeClass(): string {
    switch (this.normalizedNotificationType()) {
      case NotificationTypes.CampaignInvite:
        return 'notification-card-campaign-invite';
      default:
        return 'notification-card-default';
    }
  }

  private normalizedNotificationType(): NotificationTypes | null {
    const notificationType = this.notification().notificationType;

    if (typeof notificationType === 'number') {
      return notificationType === NotificationTypes.CampaignInvite
        ? NotificationTypes.CampaignInvite
        : null;
    }

    if (notificationType === 'CampaignInvite') {
      return NotificationTypes.CampaignInvite;
    }

    const parsedType = Number(notificationType);

    return parsedType === NotificationTypes.CampaignInvite
      ? NotificationTypes.CampaignInvite
      : null;
  }
}
