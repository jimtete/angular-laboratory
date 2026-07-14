import { NotificationTypes } from './notification-types.model';

export interface NotificationModel {
  notificationId: string;
  userId: string;
  notificationType: NotificationTypes | keyof typeof NotificationTypes | string | number;
  description: string;
  dateCreated: string;
  dateRead: string | null;
}
