import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface Message {
  id: string;
  orderId?: string;
  senderId: string;
  senderName: string;
  senderRole?: string;
  recipientId?: string;
  recipientName?: string;
  content: string;
  isRead: boolean;
  readAt?: Date;
  createdAt: Date;
  requiresAdminApproval?: boolean;
  forwardedByAdmin?: boolean;
  originalSenderRole?: string;
  isRejected?: boolean;
}

@Component({
  selector: 'app-message-list',
  templateUrl: './message-list.component.html',
  styleUrls: ['./message-list.component.scss']
})
export class MessageListComponent implements OnInit, OnDestroy {
  messages: Message[] = [];
  loading = false;
  globalFilter = '';
  first = 0;
  rows = 10;
  unreadCount = 0;
  isAdmin = false;
  showForwardDialog = false;
  selectedMessage: Message | null = null;
  forwardTargetUserId = '';
  forwardEditedContent = '';
  availableForwardTargets: { label: string; value: string }[] = [];

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.getCurrentUser()?.role === 'Admin' || this.authService.getCurrentUser()?.role === 'SuperAdmin';
    this.loadMessages();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMessages(): void {
    this.loading = true;
    // Try to fetch messages (will fail gracefully if endpoint doesn't exist)
    this.apiService.get<Message[]>('messages')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (messages) => {
          // Map backend response to frontend interface
          this.messages = messages.map((m: any) => ({
            id: m.id,
            orderId: m.orderId,
            senderId: m.senderId,
            senderName: m.senderName,
            senderRole: m.senderRole,
            recipientId: m.recipientId,
            recipientName: m.recipientName,
            content: m.content,
            isRead: m.isRead,
            readAt: m.readAt,
            createdAt: m.createdAt,
            requiresAdminApproval: m.requiresAdminApproval,
            forwardedByAdmin: m.forwardedByAdmin,
            originalSenderRole: m.originalSenderRole,
            isRejected: m.isRejected
          }));
          this.unreadCount = this.messages.filter(m => !m.isRead).length;
          this.loading = false;
        },
        error: () => {
          this.messages = [];
          this.loading = false;
        }
      });
  }

  markAsRead(messageId: string): void {
    this.apiService.put(`messages/${messageId}/read`, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          const message = this.messages.find(m => m.id === messageId);
          if (message) {
            message.isRead = true;
            this.unreadCount = Math.max(0, this.unreadCount - 1);
          }
        }
      });
  }

  openForwardDialog(msg: Message): void {
    this.selectedMessage = msg;
    this.forwardEditedContent = msg.content;
    this.forwardTargetUserId = '';
    this.loadForwardTargets(msg);
    this.showForwardDialog = true;
  }

  loadForwardTargets(msg: Message): void {
    if (!msg.orderId) {
      this.availableForwardTargets = [];
      return;
    }
    this.apiService.get<any>(`orders/${msg.orderId}`).pipe(takeUntil(this.destroy$)).subscribe({
      next: (order) => {
        const targets: { label: string; value: string }[] = [];
        const d = order.designer || order.Designer;
        const c = order.client || order.Client;
        if (msg.originalSenderRole === 'Client' && d?.userId) {
          const name = `${d.firstName || ''} ${d.lastName || ''}`.trim() || 'Designer';
          targets.push({ label: `Designer: ${name}`, value: d.userId });
        }
        if (msg.originalSenderRole === 'Designer' && c?.userId) {
          const label = c.companyName || `${c.firstName || ''} ${c.lastName || ''}`.trim() || 'Client';
          targets.push({ label: `Client: ${label}`, value: c.userId });
        }
        this.availableForwardTargets = targets;
      },
      error: () => { this.availableForwardTargets = []; }
    });
  }

  forwardMessage(): void {
    if (!this.selectedMessage || !this.forwardTargetUserId) return;
    this.apiService.post('messages/forward', {
      messageId: this.selectedMessage.id,
      targetUserId: this.forwardTargetUserId,
      editedContent: this.forwardEditedContent || undefined
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Message forwarded' });
        this.showForwardDialog = false;
        this.selectedMessage = null;
        this.loadMessages();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to forward' });
      }
    });
  }

  rejectMessage(msg: Message): void {
    this.apiService.post('messages/reject', { messageId: msg.id }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({ severity: 'info', summary: 'Rejected', detail: 'Message rejected' });
        this.loadMessages();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to reject' });
      }
    });
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    const messageDate = new Date(date);
    const now = new Date();
    const diffMs = now.getTime() - messageDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffMins < 1440) return `${Math.floor(diffMins / 60)}h ago`;
    
    return messageDate.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
