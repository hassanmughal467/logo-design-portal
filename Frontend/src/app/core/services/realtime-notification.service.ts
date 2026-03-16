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
      if (user) {
        this.connect(user.id);
      } else {
        this.disconnect();
      }
    });
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  private getHubUrl(): string {
    const base = environment.apiUrl.replace(/\/$/, '');
    return `${base}/hubs/notifications`;
  }

  private async connect(userId: string): Promise<void> {
    const token = this.authService.getAccessToken();
    if (!token) {
      this.realtimeStatus.setConnected(false);
      return;
    }

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.getHubUrl(), {
          accessTokenFactory: () => this.authService.getAccessToken() ?? ''
        })
        .withAutomaticReconnect()
        .build();

      this.connection.on('ReceiveNotification', (payload: unknown) => {
        this.handleNotification(payload);
      });

      // Entity update events for data grid synchronization (separate from notifications)
      this.connection.on('OrderCreated', (data: { orderId: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId });
      });
      this.connection.on('OrderAssigned', (data: { orderId: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId });
      });
      this.connection.on('PreviewUploaded', (data: { orderId: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId });
      });
      this.connection.on('OrderStatusChanged', (data: { orderId: string; status: string; updatedBy?: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId, status: data.status, updatedBy: data.updatedBy });
      });
      this.connection.on('PreviewApproved', (data: { orderId: string; clientName?: string; status: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId, status: data.status, clientName: data.clientName });
      });
      this.connection.on('PreviewRejected', (data: { orderId: string; clientName?: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId });
      });
      this.connection.on('PreviewDelivered', (data: { orderId: string; status: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId, status: data.status });
      });
      this.connection.on('OrderUpdated', (data: { orderId: string; status?: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId, status: data.status });
      });
      this.connection.on('InvoiceGenerated', (data: { orderId: string; invoiceId: string }) => {
        this.orderUpdateSubject.next({ orderId: data.orderId, invoiceId: data.invoiceId });
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
    this.messageService.add({
      severity,
      summary: title,
      detail: message,
      life: 10000,
      data: redirectUrl ? { redirectUrl } : undefined
    });
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
