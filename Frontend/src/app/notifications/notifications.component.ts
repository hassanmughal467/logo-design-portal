import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Notification } from '@shared/models/notification.model';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  filteredNotifications: Notification[] = [];
  loading = false;
  showUnreadOnly = false;
  unreadCount = 0;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
    this.loadUnreadCount();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadNotifications(): void {
    this.loading = true;
    this.apiService.get<Notification[]>(`notifications?unreadOnly=${this.showUnreadOnly}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notifications) => {
          this.notifications = notifications;
          this.filteredNotifications = notifications;
          this.loading = false;
        },
        error: () => {
          this.notifications = [];
          this.filteredNotifications = [];
          this.loading = false;
        }
      });
  }

  loadUnreadCount(): void {
    this.apiService.get<{count: number}>(`notifications/unread-count`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.unreadCount = response.count;
        },
        error: () => {
          this.unreadCount = 0;
        }
      });
  }

  markAsRead(notification: Notification): void {
    if (notification.isRead) return;

    this.apiService.put(`notifications/${notification.id}/read`, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          notification.isRead = true;
          notification.readAt = new Date();
          this.unreadCount = Math.max(0, this.unreadCount - 1);
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to mark notification as read'
          });
        }
      });
  }

  markAllAsRead(): void {
    this.apiService.put(`notifications/mark-all-read`, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notifications.forEach(n => {
            n.isRead = true;
            n.readAt = new Date();
          });
          this.unreadCount = 0;
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'All notifications marked as read'
          });
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to mark all as read'
          });
        }
      });
  }

  toggleUnreadFilter(): void {
    this.showUnreadOnly = !this.showUnreadOnly;
    this.loadNotifications();
  }

  getNotificationSeverity(type: string): string {
    const severityMap: { [key: string]: string } = {
      'Info': 'info',
      'Success': 'success',
      'Warning': 'warn',
      'Error': 'danger',
      'OrderStatusChange': 'info',
      'PriceApproval': 'warn',
      'RevisionRequest': 'warn',
      'FileUpload': 'success'
    };
    return severityMap[type] || 'info';
  }

  getNotificationIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      'Info': 'pi-info-circle',
      'Success': 'pi-check-circle',
      'Warning': 'pi-exclamation-triangle',
      'Error': 'pi-times-circle',
      'OrderStatusChange': 'pi-sync',
      'PriceApproval': 'pi-dollar',
      'RevisionRequest': 'pi-refresh',
      'FileUpload': 'pi-file'
    };
    return iconMap[type] || 'pi-info-circle';
  }
}
