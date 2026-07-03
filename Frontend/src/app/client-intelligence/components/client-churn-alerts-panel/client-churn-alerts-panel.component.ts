import { Component, Input } from '@angular/core';
import { ClientChurnAlertItem } from '@core/services/client-churn-analytics.service';
import { formatCurrencyAmount } from '@core/utils/currency-format';

import { TagSeverity } from '@shared/types/primeng.types';

@Component({
  selector: 'app-client-churn-alerts-panel',
  templateUrl: './client-churn-alerts-panel.component.html',
  styleUrls: ['./client-churn-alerts-panel.component.scss']
})
export class ClientChurnAlertsPanelComponent {
  @Input() items: ClientChurnAlertItem[] = [];

  formatCurrency(value: number, currencyCode?: string): string {
    return formatCurrencyAmount(value, currencyCode);
  }

  formatDate(value: string | undefined): string {
    if (!value) return '—';
    return new Date(value).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getSeverity(days: number): TagSeverity {
    if (days >= 90) return 'danger';
    if (days >= 60) return 'danger';
    if (days >= 30) return 'warning';
    return 'info';
  }
}
