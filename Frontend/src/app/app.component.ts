import { Component } from '@angular/core';
import { RealtimeNotificationService } from '@core/services/realtime-notification.service';

@Component({
  selector: 'app-root',
  template: `
    <router-outlet></router-outlet>
    <p-toast></p-toast>
  `,
  styles: []
})
export class AppComponent {
  title = 'Hawk Merchandising Web Portal';

  constructor(private realtimeNotificationService: RealtimeNotificationService) {
    // Injected to initialize SignalR connection when user logs in
  }
}
