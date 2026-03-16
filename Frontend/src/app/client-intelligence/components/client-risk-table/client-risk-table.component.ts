import { Component, Input } from '@angular/core';
import { ClientRiskScoreItem } from '@core/services/client-churn-analytics.service';

@Component({
  selector: 'app-client-risk-table',
  templateUrl: './client-risk-table.component.html',
  styleUrls: ['./client-risk-table.component.scss']
})
export class ClientRiskTableComponent {
  @Input() items: ClientRiskScoreItem[] = [];

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

  getRiskLevelLabel(level: string): string {
    const map: Record<string, string> = {
      Healthy: 'Healthy',
      Warning: 'Warning',
      HighRisk: 'High Risk',
      ChurnLikely: 'Churn Likely'
    };
    return map[level] || level;
  }

  getRiskSeverity(level: string): string {
    const map: Record<string, string> = {
      Healthy: 'success',
      Warning: 'warn',
      HighRisk: 'danger',
      ChurnLikely: 'danger'
    };
    return map[level] || 'info';
  }
}
