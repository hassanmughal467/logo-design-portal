import { Component, OnInit, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService } from '@core/services/notification.service';
import { MessageService } from 'primeng/api';
import { Observable, Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Notification } from '@shared/models/notification.model';
import { getNotificationIcon, formatReferenceDisplay, getActionLabel, hasNavigableTarget } from '@shared/utils/notification-helpers';
import { groupNotificationsByDate, NotificationGroup } from '@shared/utils/notification-grouping';

const PAGE_SIZE = 20;
const SEARCH_DEBOUNCE_MS = 300;

export type NotificationFilter = 'all' | 'Order' | 'Message' | 'Invoice' | 'System';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent implements OnInit, OnDestroy {
  groupedNotifications: NotificationGroup[] = [];
  loading = false;
  showUnreadOnly = false;
  currentPage = 1;
  totalCount = 0;
  totalPages = 1;
  pageSize = PAGE_SIZE;

  searchQuery = '';
  activeFilter: NotificationFilter = 'all';

  filters: { value: NotificationFilter; label: string }[] = [
    { value: 'all', label: 'All' },
    { value: 'Order', label: 'Orders' },
    { value: 'Message', label: 'Messages' },
    { value: 'Invoice', label: 'Invoices' },
    { value: 'System', label: 'System' }
  ];

  unreadCount$!: Observable<number>;

  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  constructor(
    private notificationService: NotificationService,
    private messageService: MessageService,
    private router: Router,
    private location: Location
  ) {
    this.unreadCount$ = this.notificationService.unreadCount$;
  }

  ngOnInit(): void {
    this.setupSearchDebounce();
    this.loadNotifications();
    this.subscribeToRealtimeUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearchDebounce(): void {
    this.searchSubject.pipe(
      debounceTime(SEARCH_DEBOUNCE_MS),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(search => {
      this.currentPage = 1;
      this.loadNotifications();
    });
  }

  private subscribeToRealtimeUpdates(): void {
    this.notificationService.refreshRequested$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadNotifications());

    this.notificationService.notificationReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notification => this.mergeRealtimeNotification(notification));
  }

  private mergeRealtimeNotification(notification: Notification): void {
    for (const group of this.groupedNotifications) {
      const idx = group.notifications.findIndex(n => n.id === notification.id);
      if (idx >= 0) {
        group.notifications[idx] = notification;
        return;
      }
    }
  }

  loadNotifications(): void {
    this.loading = true;
    const referenceType = this.activeFilter === 'all' ? undefined : this.activeFilter;
    const search = this.searchQuery?.trim() || undefined;

    this.notificationService.getNotificationsPaginated({
      limit: this.pageSize,
      offset: (this.currentPage - 1) * this.pageSize,
      unreadOnly: this.showUnreadOnly,
      search,
      referenceType
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.totalCount = result.totalCount;
          this.totalPages = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
          this.groupedNotifications = groupNotificationsByDate(result.items);
          this.loading = false;
        },
        error: () => {
          this.groupedNotifications = [];
          this.totalCount = 0;
          this.totalPages = 1;
          this.loading = false;
        }
      });
  }

  onSearchInput(): void {
    this.searchSubject.next(this.searchQuery);
  }

  onFilterChange(filter: NotificationFilter): void {
    this.activeFilter = filter;
    this.currentPage = 1;
    this.loadNotifications();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadNotifications();
  }

  markAsRead(notification: Notification): void {
    if (notification.isRead) return;

    this.notificationService.markAsRead(notification.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          notification.isRead = true;
          notification.readAt = new Date();
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
    this.notificationService.markAllAsRead()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadNotifications();
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
    this.currentPage = 1;
    this.loadNotifications();
  }

  onNotificationClick(notification: Notification): void {
    this.markAsRead(notification);
    this.navigateFromNotification(notification);
  }

  markAsReadOnly(notification: Notification): void {
    this.markAsRead(notification);
  }

  getNotificationIcon(notification: Notification): string {
    return getNotificationIcon(notification);
  }

  getReferenceDisplay(notification: Notification): string | null {
    return formatReferenceDisplay(notification);
  }

  getActionLabel(notification: Notification): string | null {
    return getActionLabel(notification);
  }

  hasNavigableTarget(notification: Notification): boolean {
    return hasNavigableTarget(notification);
  }

  goBack(): void {
    this.location.back();
  }

  navigateFromNotification(notification: Notification): void {
    const url = notification.redirectUrl?.trim();
    if (url) {
      this.router.navigateByUrl(url.startsWith('/') ? url : `/${url}`);
      return;
    }
    const refType = (notification.referenceType ?? 'Order').toLowerCase();
    const refId = notification.referenceId ?? notification.orderId;
    const orderId = notification.orderId ?? (refType === 'order' ? refId : null);
    switch (refType) {
      case 'order':
        if (refId) this.router.navigate(['/orders', refId]);
        break;
      case 'invoice':
        if (refId) this.router.navigate(['/invoices', refId]);
        break;
      case 'message':
        if (orderId) this.router.navigate(['/orders', orderId]);
        else if (refId) this.router.navigate(['/messages']);
        break;
      case 'system':
        this.router.navigate(['/users']);
        break;
      default:
        if (orderId || refId) this.router.navigate(['/orders', orderId ?? refId]);
    }
  }

  get hasNotifications(): boolean {
    return this.groupedNotifications.some(g => g.notifications.length > 0);
  }
}
