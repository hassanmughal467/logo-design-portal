import { Injectable, OnDestroy } from '@angular/core';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { of } from 'rxjs';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { RealtimeStatusService } from './realtime-status.service';
import { Notification } from '@shared/models/notification.model';

const POLL_FALLBACK_INTERVAL_MS = 60000; // 60 seconds when SignalR is disconnected

@Injectable({
  providedIn: 'root'
})
export class NotificationService implements OnDestroy {
  private readonly unreadCountSubject = new BehaviorSubject<number>(0);
  readonly unreadCount$ = this.unreadCountSubject.asObservable();

  /** Emits when a realtime notification arrived - use to refresh dropdown if open */
  private readonly refreshRequestedSubject = new Subject<void>();
  readonly refreshRequested$ = this.refreshRequestedSubject.asObservable();

  /** Emits the full notification payload from SignalR for update-in-place (activity feed behavior). */
  private readonly notificationReceivedSubject = new Subject<Notification>();
  readonly notificationReceived$ = this.notificationReceivedSubject.asObservable();

  private pollInterval: ReturnType<typeof setInterval> | null = null;

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private realtimeStatus: RealtimeStatusService
  ) {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.loadUnreadCount();
        this.updatePolling();
      } else {
        this.unreadCountSubject.next(0);
        this.stopPolling();
      }
    });
    this.realtimeStatus.isConnected$.subscribe(() => this.updatePolling());
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  /** Get notifications (simple list - for dropdown). Pass limit for recent only. */
  getNotifications(unreadOnly = false, limit?: number): Observable<Notification[]> {
    let url = `notifications?unreadOnly=${unreadOnly}`;
    if (limit != null && limit > 0) {
      url += `&limit=${limit}`;
    }
    return this.apiService.get<any>(url).pipe(
      map(raw => this.normalizeNotifications(raw)),
      catchError(() => of([]))
    );
  }

  /** Paginated notifications with filters and search. */
  getNotificationsPaginated(params: {
    limit?: number;
    offset?: number;
    unreadOnly?: boolean;
    search?: string;
    referenceType?: string;
    type?: string;
  }): Observable<{ items: Notification[]; totalCount: number; limit: number; offset: number }> {
    const q = new URLSearchParams();
    q.set('unreadOnly', String(params.unreadOnly ?? false));
    const limit = params.limit ?? 20;
    const offset = params.offset ?? 0;
    q.set('limit', String(limit));
    q.set('offset', String(offset));
    if (params.search?.trim()) q.set('search', params.search.trim());
    if (params.referenceType?.trim()) q.set('referenceType', params.referenceType.trim());
    if (params.type?.trim()) q.set('type', params.type.trim());

    return this.apiService.get<any>(`notifications?${q.toString()}`).pipe(
      map(raw => {
        const items = this.normalizeNotifications(raw?.items ?? raw);
        return {
          items,
          totalCount: raw?.totalCount ?? raw?.TotalCount ?? items.length,
          limit: raw?.limit ?? raw?.Limit ?? (params.limit ?? 20),
          offset: raw?.offset ?? raw?.Offset ?? (params.offset ?? 0)
        };
      }),
      catchError(() => of({ items: [], totalCount: 0, limit: params.limit ?? 20, offset: params.offset ?? 0 }))
    );
  }

  /** Normalize API response - handle array, wrapped { data }, and PascalCase/camelCase */
  private normalizeNotifications(raw: any): Notification[] {
    const arr = Array.isArray(raw)
      ? raw
      : raw?.data ?? raw?.notifications ?? raw?.items ?? [];
    if (!arr || !Array.isArray(arr)) return [];
    return arr.map((n: any) => this.normalizeSingle(n));
  }

  /** Called when SignalR receives a notification - emits for update-in-place and refreshes. */
  onRealtimeNotificationReceived(payload: any): void {
    if (payload?.id || payload?.Id) {
      const notification = this.normalizeSingle(payload);
      this.notificationReceivedSubject.next(notification);
    }
    this.loadUnreadCount();
    this.refreshRequestedSubject.next();
  }

  /** Normalize a single notification (used for API and SignalR payloads). */
  private normalizeSingle(n: any): Notification {
    return {
      id: String(n.id ?? n.Id ?? ''),
      orderId: n.orderId ?? n.OrderId ? String(n.orderId ?? n.OrderId) : undefined,
      title: n.title ?? n.Title ?? '',
      message: n.message ?? n.Message ?? '',
      type: n.type ?? n.Type ?? 'Info',
      referenceType: n.referenceType ?? n.ReferenceType,
      referenceId: n.referenceId ?? n.ReferenceId ? String(n.referenceId ?? n.ReferenceId) : undefined,
      isRead: n.isRead ?? n.IsRead ?? false,
      readAt: n.readAt ?? n.ReadAt,
      createdAt: new Date(n.createdAt ?? n.CreatedAt ?? Date.now()),
      aggregationCount: n.aggregationCount ?? n.AggregationCount ?? 1,
      lastOccurrenceAt: n.lastOccurrenceAt ?? n.LastOccurrenceAt ? new Date(n.lastOccurrenceAt ?? n.LastOccurrenceAt) : undefined
    };
  }

  getUnreadCount(): Observable<number> {
    return this.apiService.get<any>('notifications/unread-count').pipe(
      map(response => {
        const count = response?.count ?? response?.Count ?? 0;
        this.unreadCountSubject.next(count);
        return count;
      }),
      catchError(() => {
        this.unreadCountSubject.next(0);
        return of(0);
      })
    );
  }

  loadUnreadCount(): void {
    this.apiService.get<any>('notifications/unread-count').pipe(
      catchError(() => of({ count: 0 }))
    ).subscribe(response => {
      const count = response?.count ?? response?.Count ?? 0;
      this.unreadCountSubject.next(count);
    });
  }

  markAsRead(notificationId: string): Observable<unknown> {
    return this.apiService.put(`notifications/${notificationId}/read`, {}).pipe(
      tap(() => this.loadUnreadCount())
    );
  }

  markAllAsRead(): Observable<unknown> {
    return this.apiService.put('notifications/mark-all-read', {}).pipe(
      tap(() => this.unreadCountSubject.next(0))
    );
  }

  private updatePolling(): void {
    if (!this.authService.getCurrentUser()) {
      this.stopPolling();
      return;
    }
    if (this.realtimeStatus.isConnected()) {
      this.stopPolling();
    } else {
      this.startPolling();
    }
  }

  private startPolling(): void {
    this.stopPolling();
    this.pollInterval = setInterval(() => {
      this.loadUnreadCount();
    }, POLL_FALLBACK_INTERVAL_MS);
  }

  private stopPolling(): void {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
      this.pollInterval = null;
    }
  }

  /** Called by RealtimeNotificationService when SignalR receives a notification - refreshes unread count and dropdown. */
  refreshNotifications(): void {
    this.loadUnreadCount();
    this.refreshRequestedSubject.next();
  }
}
