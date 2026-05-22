import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';
import { RealtimeStatusService } from './realtime-status.service';
import { MessageService } from 'primeng/api';
import { environment } from '@environments/environment';

export interface RealtimeNotificationPayload {
  title: string;
  message: string;
  type: string;
  orderId?: string;
}

/** Payload for real-time order entity updates (data grid sync, separate from notifications) */
export interface OrderUpdatePayload {
  orderId: string;
  status?: string;
  updatedBy?: string;
  clientName?: string;
  invoiceId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class RealtimeNotificationService implements OnDestroy {
  private connection: signalR.HubConnection | null = null;
  private readonly orderUpdateSubject = new Subject<OrderUpdatePayload>();

  /** Observable of order entity updates for grid synchronization */
  readonly orderUpdates$: Observable<OrderUpdatePayload> = this.orderUpdateSubject.asObservable();

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private realtimeStatus: RealtimeStatusService,
    private messageService: MessageService
  ) {
    this.authService.currentUser$.subscribe(user => {
      const userId = this.resolveUserId(user);
      if (userId) {
        this.connect(userId);
      } else {
        this.disconnect();
      }
    });
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  private getHubUrl(): string {
    const base = (environment.signalRUrl ?? environment.apiUrl).replace(/\/$/, '');
    return `${base}/hubs/notifications`;
  }

  private async connect(userId: string): Promise<void> {
    const useCookies = !!(environment as { useCookieAuth?: boolean }).useCookieAuth;
    const token = this.authService.getAccessToken();
    if (!useCookies && !token) {
      this.realtimeStatus.setConnected(false);
      return;
    }

    try {
      const hubOptions: signalR.IHttpConnectionOptions = { withCredentials: true };
      if (!useCookies) {
        hubOptions.accessTokenFactory = () => this.authService.getAccessToken() ?? '';
      }

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.getHubUrl(), hubOptions)
        .withAutomaticReconnect()
        .build();

      this.connection.on('ReceiveNotification', (payload: unknown) => {
        this.handleNotification(payload);
      });

      // Entity update events for data grid synchronization (separate from notifications)
      this.connection.on('OrderCreated', (data: { orderId: string }) => {
        const orderId = data?.orderId ?? (data as { OrderId?: string })?.OrderId ?? '';
        if (!orderId) return;
        this.orderUpdateSubject.next({ orderId });
        // Refresh bell/list only — toast comes from ReceiveNotification (avoid duplicate popups)
        this.notificationService.refreshNotifications();
      });
      this.connection.on('OrderAssigned', (data: unknown) => {
        const payload = this.resolveOrderUpdatePayload(data);
        if (payload) this.orderUpdateSubject.next(payload);
      });
      this.connection.on('PreviewUploaded', (data: unknown) => {
        const payload = this.resolveOrderUpdatePayload(data);
        if (payload) this.orderUpdateSubject.next(payload);
      });
      this.connection.on('OrderStatusChanged', (data: unknown) => {
        const payload = this.resolveOrderUpdatePayload(data);
        if (payload) this.orderUpdateSubject.next(payload);
      });
      this.connection.on('PreviewApproved', (data: unknown) => {
        const payload = this.resolveOrderUpdatePayload(data);
        if (payload) this.orderUpdateSubject.next(payload);
      });
      this.connection.on('PreviewRejected', (data: unknown) => {
        const payload = this.resolveOrderUpdatePayload(data);
        if (payload) this.orderUpdateSubject.next(payload);
      });
      this.connection.on('PreviewDelivered', (data: unknown) => {
        const payload = this.resolveOrderUpdatePayload(data);
        if (payload) this.orderUpdateSubject.next(payload);
      });
      this.connection.on('OrderUpdated', (data: unknown) => {
        const payload = this.resolveOrderUpdatePayload(data);
        if (payload) this.orderUpdateSubject.next(payload);
      });
      this.connection.on('InvoiceGenerated', (data: unknown) => {
        const payload = this.resolveOrderUpdatePayload(data);
        if (payload) this.orderUpdateSubject.next(payload);
      });

      this.connection.onreconnecting(() => {
        this.realtimeStatus.setConnected(false);
      });

      this.connection.onreconnected(() => {
        this.realtimeStatus.setConnected(true);
        this.joinUserGroup(userId);
        // Emit synthetic refresh so dashboards recover from missed events during disconnection
        this.orderUpdateSubject.next({ orderId: '**reconnect**' });
      });

      this.connection.onclose(() => {
        this.realtimeStatus.setConnected(false);
      });

      await this.connection.start();
      this.realtimeStatus.setConnected(true);
      await this.joinUserGroup(userId);
    } catch (err) {
      console.warn('SignalR connection failed, using polling fallback:', err);
      this.realtimeStatus.setConnected(false);
    }
  }

  private resolveUserId(user: { id?: string; Id?: string } | null): string | null {
    if (!user) return null;
    const id = String(user.id ?? user.Id ?? '').trim();
    return id || null;
  }

  private async joinUserGroup(userId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinUserGroup', userId);
    }
  }

  private handleNotification(payload: unknown): void {
    const p = payload as Record<string, unknown>;
    this.notificationService.onRealtimeNotificationReceived(payload);
    const type = (p?.['type'] ?? p?.['Type'] ?? '') as string;
    const title = (p?.['title'] ?? p?.['Title'] ?? '') as string;
    const message = (p?.['message'] ?? p?.['Message'] ?? '') as string;
    const severity = this.mapTypeToSeverity(type);
    const redirectUrl = this.buildRedirectUrl(p);
    const notificationId = this.resolveNotificationId(p);
    const toastData =
      redirectUrl || notificationId
        ? { ...(redirectUrl ? { redirectUrl } : {}), ...(notificationId ? { notificationId } : {}) }
        : undefined;
    this.messageService.add({
      severity,
      summary: title,
      detail: message,
      life: 10000,
      data: toastData
    });
  }

  /** Normalize hub payloads (camelCase or PascalCase) for grid/dashboard sync. */
  private resolveOrderUpdatePayload(data: unknown): OrderUpdatePayload | null {
    const d = data as Record<string, unknown> | null | undefined;
    const orderId = String(d?.['orderId'] ?? d?.['OrderId'] ?? '').trim();
    if (!orderId) return null;
    const status = (d?.['status'] ?? d?.['Status']) as string | undefined;
    const updatedBy = (d?.['updatedBy'] ?? d?.['UpdatedBy']) as string | undefined;
    const clientName = (d?.['clientName'] ?? d?.['ClientName']) as string | undefined;
    const invoiceId = String(d?.['invoiceId'] ?? d?.['InvoiceId'] ?? '').trim() || undefined;
    return {
      orderId,
      ...(status ? { status: String(status) } : {}),
      ...(updatedBy ? { updatedBy: String(updatedBy) } : {}),
      ...(clientName ? { clientName: String(clientName) } : {}),
      ...(invoiceId ? { invoiceId } : {})
    };
  }

  private resolveNotificationId(p: Record<string, unknown>): string | undefined {
    const raw = String(p?.['id'] ?? p?.['Id'] ?? '').trim();
    if (!raw || raw === '00000000-0000-0000-0000-000000000000') return undefined;
    return raw;
  }

  /** Build redirect URL for all notification types (Order, Invoice, Message, System). */
  private buildRedirectUrl(p: Record<string, unknown>): string | null {
    const url = (p?.['redirectUrl'] ?? p?.['RedirectUrl'] ?? '') as string;
    if (url?.trim()) return url.trim();
    const refType = ((p?.['referenceType'] ?? p?.['ReferenceType'] ?? 'Order') as string).toLowerCase();
    const refId = (p?.['referenceId'] ?? p?.['ReferenceId'] ?? p?.['orderId'] ?? p?.['OrderId']) as string | undefined;
    const orderId = (p?.['orderId'] ?? p?.['OrderId']) as string | undefined;
    const id = refId ?? orderId;
    if (!id) return null;
    switch (refType) {
      case 'order': return `/orders/${id}`;
      case 'invoice': return `/invoices/${id}`;
      case 'message': return orderId ? `/orders/${orderId}` : '/messages';
      case 'system': return '/users';
      default: return orderId ? `/orders/${orderId}` : id ? `/orders/${id}` : null;
    }
  }

  private mapTypeToSeverity(type: string): 'info' | 'success' | 'warn' | 'error' {
    const t = (type || '').toLowerCase();
    if (t === 'error' || t === 'warning') return 'error';
    if (t === 'success') return 'success';
    if (t === 'warn') return 'warn';
    return 'info';
  }

  private disconnect(): void {
    if (this.connection) {
      this.connection.stop().catch(() => {});
      this.connection = null;
    }
    this.realtimeStatus.setConnected(false);
  }
}
