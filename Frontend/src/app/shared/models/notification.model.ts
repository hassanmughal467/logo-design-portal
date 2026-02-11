export interface Notification {
  id: string;
  orderId?: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  readAt?: Date;
  createdAt: Date;
}

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
