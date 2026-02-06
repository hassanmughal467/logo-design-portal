import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface Message {
  id: string;
  orderId?: string;
  clientId: string;
  clientName: string;
  subject: string;
  message: string;
  isRead: boolean;
  createdAt: Date;
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

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
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
          this.messages = messages;
          this.unreadCount = messages.filter(m => !m.isRead).length;
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
