import { Notification } from '../models/notification.model';

/** PrimeNG icon classes by notification category */
export const NOTIFICATION_ICONS: Record<string, string> = {
  // Order events
  OrderStatusChange: 'pi pi-shopping-cart',
  OrderCreated: 'pi pi-shopping-cart',
  PreviewDelivered: 'pi pi-shopping-cart',
  FileUpload: 'pi pi-shopping-cart',
  PriceApproval: 'pi pi-shopping-cart',
  RevisionRequest: 'pi pi-shopping-cart',
  // Invoice events
  InvoiceGenerated: 'pi pi-file',
  // Message events (referenceType Message)
  // System alerts
  Info: 'pi pi-exclamation-triangle',
  Success: 'pi pi-exclamation-triangle',
  Warning: 'pi pi-exclamation-triangle',
  Error: 'pi pi-exclamation-triangle',
};

/** User-friendly labels for notification types */
export const NOTIFICATION_LABELS: Record<string, string> = {
  OrderStatusChange: 'Order Update',
  OrderCreated: 'New Order',
  PreviewDelivered: 'Preview Delivered',
  InvoiceGenerated: 'Invoice Created',
  FileUpload: 'File Uploaded',
  PriceApproval: 'Price Approval',
  RevisionRequest: 'Revision Request',
  Info: 'Info',
  Success: 'Success',
  Warning: 'Warning',
  Error: 'Error',
};

export function getNotificationIcon(notification: Notification): string {
  const refType = notification.referenceType?.toLowerCase();
  if (refType === 'invoice') return 'pi pi-file';
  if (refType === 'message') return 'pi pi-comments';
  if (refType === 'system') return 'pi pi-exclamation-triangle';
  const type = notification.type ?? '';
  return NOTIFICATION_ICONS[type] ?? 'pi pi-shopping-cart';
}

export function getNotificationLabel(type: string): string {
  return NOTIFICATION_LABELS[type] ?? type;
}

/** Action label for contextual navigation link */
export function getActionLabel(notification: Notification): string | null {
  const refId = notification.referenceId ?? notification.orderId;
  if (!refId) return null;
  const refType = (notification.referenceType ?? 'Order').toLowerCase();
  switch (refType) {
    case 'order': return 'View Order';
    case 'invoice': return 'View Invoice';
    case 'message': return 'Open Conversation';
    default: return notification.orderId ? 'View Order' : null;
  }
}

/** Whether the notification has a navigable target */
export function hasNavigableTarget(notification: Notification): boolean {
  return !!(notification.referenceId ?? notification.orderId);
}

/** Format reference ID as user-friendly display: Order #ORD-6806CDB7 or Invoice #INV-6806CDB7 */
export function formatReferenceDisplay(notification: Notification): string | null {
  const id = notification.referenceId ?? notification.orderId;
  if (!id) return null;
  const shortId = String(id).replace(/-/g, '').substring(0, 8).toUpperCase();
  const refType = notification.referenceType ?? 'Order';
  const prefix = refType === 'Invoice' ? 'INV' : 'ORD';
  return `${refType} #${prefix}-${shortId}`;
}

/** Format date as relative time: "2 minutes ago", "1 hour ago", "Yesterday", "Mar 6" */
export function formatRelativeTime(date: Date | string): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  const now = new Date();
  const diffMs = now.getTime() - d.getTime();
  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHour = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHour / 24);

  if (diffSec < 60) return 'Just now';
  if (diffMin < 60) return `${diffMin} minute${diffMin === 1 ? '' : 's'} ago`;
  if (diffHour < 24) return `${diffHour} hour${diffHour === 1 ? '' : 's'} ago`;
  if (diffDay === 1) return 'Yesterday';
  if (diffDay < 7) return `${diffDay} days ago`;
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}
