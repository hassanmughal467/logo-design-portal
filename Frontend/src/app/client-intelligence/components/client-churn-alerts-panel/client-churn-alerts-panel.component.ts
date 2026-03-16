import { Component, Input } from '@angular/core';
import { ClientChurnAlertItem } from '@core/services/client-churn-analytics.service';

@Component({
  selector: 'app-client-churn-alerts-panel',
  templateUrl: './client-churn-alerts-panel.component.html',
  styleUrls: ['./client-churn-alerts-panel.component.scss']
})
export class ClientChurnAlertsPanelComponent {
  @Input() items: ClientChurnAlertItem[] = [];

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  }

  formatDate(value: string | undefined): string {
    if (!value) return '—';
    return new Date(value).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getSeverity(days: number): string {
    if (days >= 90) return 'danger';
    if (days >= 60) return 'danger';
    if (days >= 30) return 'warn';
    return 'info';
  }
}
