import { Component, Input } from '@angular/core';
import { FinancialOverview } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-financial-kpi-cards',
  templateUrl: './financial-kpi-cards.component.html',
  styleUrls: ['./financial-kpi-cards.component.scss']
})
export class FinancialKpiCardsComponent {
  @Input() data: FinancialOverview | null = null;
  @Input() loading = false;

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(v);
  }

  formatPercent(v: number): string {
    return `${v >= 0 ? '+' : ''}${v}%`;
  }
}
