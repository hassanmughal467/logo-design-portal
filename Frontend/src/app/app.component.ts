import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { RealtimeNotificationService } from '@core/services/realtime-notification.service';

@Component({
  selector: 'app-root',
  template: `
    <router-outlet></router-outlet>
    <p-toast position="top-right">
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
    private router: Router
  ) {}

  onToastClick(message: { data?: { redirectUrl?: string } }): void {
    const url = message.data?.redirectUrl?.trim();
    if (url) {
      this.router.navigateByUrl(url.startsWith('/') ? url : `/${url}`);
    }
  }
}
