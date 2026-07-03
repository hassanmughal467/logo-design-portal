import { Component, Input } from '@angular/core';
import { ClientRiskScoreItem } from '@core/services/client-churn-analytics.service';
import { formatCurrencyAmount } from '@core/utils/currency-format';

import { TagSeverity } from '@shared/types/primeng.types';

@Component({
  selector: 'app-client-risk-table',
  templateUrl: './client-risk-table.component.html',
  styleUrls: ['./client-risk-table.component.scss']
})
export class ClientRiskTableComponent {
  @Input() items: ClientRiskScoreItem[] = [];

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

  getRiskLevelLabel(level: string): string {
    const map: Record<string, string> = {
      Healthy: 'Healthy',
      Warning: 'Warning',
      HighRisk: 'High Risk',
      ChurnLikely: 'Churn Likely'
    };
    return map[level] || level;
  }

  getRiskSeverity(level: string): TagSeverity {
    const map: Record<string, TagSeverity> = {
      Healthy: 'success',
      Warning: 'warning',
      HighRisk: 'danger',
      ChurnLikely: 'danger'
    };
    return map[level] || 'info';
  }
}
