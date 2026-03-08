export interface Notification {
  id: string;
  orderId?: string;
  title: string;
  message: string;
  type: NotificationType;
  referenceType?: NotificationReferenceType;
  referenceId?: string;
  /** Frontend route to navigate when clicked (e.g. /orders/123, /invoices/456). */
  redirectUrl?: string;
  isRead: boolean;
  readAt?: Date;
  createdAt: Date;
  /** Number of similar events aggregated (1 = single event). */
  aggregationCount?: number;
  /** Timestamp of the most recent occurrence when aggregated. */
  lastOccurrenceAt?: Date;
}

export type NotificationReferenceType = 'Order' | 'Invoice' | 'Message' | 'System';

export enum NotificationType {
  Info = 'Info',
  Success = 'Success',
  Warning = 'Warning',
  Error = 'Error',
  OrderStatusChange = 'OrderStatusChange',
  PriceApproval = 'PriceApproval',
  RevisionRequest = 'RevisionRequest',
  FileUpload = 'FileUpload'
}
