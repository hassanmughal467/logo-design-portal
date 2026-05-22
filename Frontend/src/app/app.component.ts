import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { RealtimeNotificationService } from '@core/services/realtime-notification.service';
import { NotificationService } from '@core/services/notification.service';
import { LoadingService } from '@core/services/loading.service';

@Component({
  selector: 'app-root',
  template: `
    <div class="critical-loading-bar" *ngIf="loading.criticalLoading$ | async" aria-hidden="true"></div>
    <router-outlet></router-outlet>
    <p-toast position="top-right" [showTransitionOptions]="'300ms'" [hideTransitionOptions]="'500ms'">
      <ng-template let-message pTemplate="message">
        <div
          class="toast-message-wrapper"
          [class.clickable]="message.data?.redirectUrl"
          (click)="onToastClick(message)">
          <strong>{{ message.summary }}</strong>
          <p class="toast-detail">{{ message.detail }}</p>
          <span *ngIf="message.data?.redirectUrl" class="toast-view-link">View <i class="pi pi-arrow-right"></i></span>
        </div>
      </ng-template>
    </p-toast>
  `,
  styles: [`
    .critical-loading-bar {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      height: 3px;
      z-index: 11000;
      pointer-events: none;
      background: linear-gradient(90deg, var(--primary-color, #0d47a1), var(--primary-color-light, #1976d2), var(--primary-color, #0d47a1));
      background-size: 200% 100%;
      animation: app-critical-bar-shimmer 1.1s ease-in-out infinite;
    }
    @keyframes app-critical-bar-shimmer {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
    .toast-message-wrapper { min-width: 200px; }
    .toast-message-wrapper.clickable { cursor: pointer; }
    .toast-detail { margin: 0.25rem 0 0 0; }
    .toast-view-link { font-size: 0.85rem; opacity: 0.9; display: inline-block; margin-top: 0.25rem; }
  `]
})
export class AppComponent {
  title = 'Hawk Merchandising Web Portal';

  constructor(
    private realtimeNotificationService: RealtimeNotificationService,
    private notificationService: NotificationService,
    private router: Router,
    private messageService: MessageService,
    readonly loading: LoadingService
  ) {}

  onToastClick(message: { data?: { redirectUrl?: string; notificationId?: string } }): void {
    const notificationId = message.data?.notificationId?.trim();
    if (notificationId) {
      this.notificationService.markAsRead(notificationId).subscribe({
        next: () => this.notificationService.refreshNotifications()
      });
    }
    const url = message.data?.redirectUrl?.trim();
    if (url) {
      this.router.navigateByUrl(url.startsWith('/') ? url : `/${url}`);
      this.messageService.clear();
    }
  }
}
