import { Notification } from '../models/notification.model';

export type NotificationGroupKey = 'today' | 'yesterday' | 'earlier';

export interface NotificationGroup {
  key: NotificationGroupKey;
  label: string;
  notifications: Notification[];
}

/** Group notifications by date: Today, Yesterday, Earlier */
export function groupNotificationsByDate(notifications: Notification[]): NotificationGroup[] {
  const now = new Date();
  const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const yesterdayStart = new Date(todayStart);
  yesterdayStart.setDate(yesterdayStart.getDate() - 1);

  const groups: Record<NotificationGroupKey, Notification[]> = {
    today: [],
    yesterday: [],
    earlier: []
  };

  for (const n of notifications) {
    const dateVal = n.lastOccurrenceAt ?? n.createdAt;
    const d = dateVal instanceof Date ? dateVal : new Date(dateVal as unknown as string | number);
    if (d >= todayStart) {
      groups.today.push(n);
    } else if (d >= yesterdayStart) {
      groups.yesterday.push(n);
    } else {
      groups.earlier.push(n);
    }
  }

  const result: NotificationGroup[] = [];
  if (groups.today.length > 0) {
    result.push({ key: 'today', label: 'Today', notifications: groups.today });
  }
  if (groups.yesterday.length > 0) {
    result.push({ key: 'yesterday', label: 'Yesterday', notifications: groups.yesterday });
  }
  if (groups.earlier.length > 0) {
    result.push({ key: 'earlier', label: 'Earlier', notifications: groups.earlier });
  }
  return result;
}
