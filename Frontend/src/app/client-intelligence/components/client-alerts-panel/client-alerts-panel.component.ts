import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-client-alerts-panel',
  templateUrl: './client-alerts-panel.component.html',
  styleUrls: ['./client-alerts-panel.component.scss']
})
export class ClientAlertsPanelComponent {
  @Input() items: any[] = [];

  getAlertSeverity(alertType: string): string {
    const map: Record<string, string> = {
      Inactive: 'danger',
      RevenueIncrease: 'success',
      Milestone: 'info'
    };
    return map[alertType] || 'info';
  }

  getAlertIcon(alertType: string): string {
    const map: Record<string, string> = {
      Inactive: 'pi-user-minus',
      RevenueIncrease: 'pi-chart-line',
      Milestone: 'pi-star'
    };
    return map[alertType] || 'pi-bell';
  }

  formatDate(value: string | undefined): string {
    if (!value) return '';
    return new Date(value).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
