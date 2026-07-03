import {
  formatReferenceDisplay,
  formatRelativeTime,
  getActionLabel,
  getNotificationIcon,
  getNotificationLabel,
  hasNavigableTarget
} from './notification-helpers';
import { Notification, NotificationType } from '../models/notification.model';

describe('notification-helpers', () => {
  const base: Notification = {
    id: 'n1',
    type: NotificationType.OrderStatusChange,
    title: 't',
    message: 'm',
    isRead: false,
    createdAt: new Date()
  };

  it('resolves icons by reference type and notification type', () => {
    expect(getNotificationIcon({ ...base, referenceType: 'Invoice' })).toBe('pi pi-file');
    expect(getNotificationIcon({ ...base, referenceType: 'Message' })).toBe('pi pi-comments');
    expect(getNotificationIcon({ ...base, referenceType: 'System' })).toBe('pi pi-exclamation-triangle');
    expect(getNotificationIcon({ ...base, type: NotificationType.Info, referenceType: undefined })).toBe('pi pi-exclamation-triangle');
  });

  it('returns labels with fallback to raw type', () => {
    expect(getNotificationLabel('OrderCreated')).toBe('New Order');
    expect(getNotificationLabel('CustomType')).toBe('CustomType');
  });

  it('builds action labels from reference metadata', () => {
    expect(getActionLabel({ ...base, referenceId: '1', referenceType: 'Order' })).toBe('View Order');
    expect(getActionLabel({ ...base, referenceId: '1', referenceType: 'Invoice' })).toBe('View Invoice');
    expect(getActionLabel({ ...base, referenceId: '1', referenceType: 'Message' })).toBe('Open Conversation');
    expect(getActionLabel({ ...base, orderId: 'o1' })).toBe('View Order');
    expect(getActionLabel({ ...base })).toBeNull();
  });

  it('detects navigable targets', () => {
    expect(hasNavigableTarget({ ...base, redirectUrl: '/orders/1' })).toBe(true);
    expect(hasNavigableTarget({ ...base, referenceId: 'abc' })).toBe(true);
    expect(hasNavigableTarget({ ...base })).toBe(false);
  });

  it('formats reference display ids', () => {
    expect(formatReferenceDisplay({ ...base, referenceId: 'abc-def-ghi-jkl', referenceType: 'Invoice' }))
      .toBe('Invoice #INV-ABCDEFGH');
    expect(formatReferenceDisplay({ ...base })).toBeNull();
  });

  it('formats relative time buckets', () => {
    const now = new Date();
    expect(formatRelativeTime(now)).toBe('Just now');
    expect(formatRelativeTime(new Date(now.getTime() - 5 * 60 * 1000))).toContain('minute');
    expect(formatRelativeTime(new Date(now.getTime() - 2 * 60 * 60 * 1000))).toContain('hour');
    expect(formatRelativeTime(new Date(now.getTime() - 24 * 60 * 60 * 1000))).toBe('Yesterday');
    expect(formatRelativeTime(new Date(now.getTime() - 3 * 24 * 60 * 60 * 1000))).toContain('days ago');
    expect(formatRelativeTime('2020-01-15T12:00:00Z')).toMatch(/Jan|15/);
  });
});
