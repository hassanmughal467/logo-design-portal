import { Component, Input } from '@angular/core';
import {
  FinancialOverview,
  FinancialRevenueByCurrencyItem
} from '@core/services/financial-analytics.service';
import {
  DEFAULT_INVOICE_CURRENCY,
  currencyIconClass,
  formatCurrencyAmount
} from '@core/utils/currency-format';

@Component({
  selector: 'app-financial-kpi-cards',
  templateUrl: './financial-kpi-cards.component.html',
  styleUrls: ['./financial-kpi-cards.component.scss']
})
export class FinancialKpiCardsComponent {
  @Input() data: FinancialOverview | null = null;

  get revenueCurrencyCode(): string {
    return this.data?.revenueCurrencyCode || DEFAULT_INVOICE_CURRENCY;
  }

  get revenueCurrencyMixed(): boolean {
    return !!this.data?.revenueCurrencyMixed;
  }

  get revenueByCurrency(): FinancialRevenueByCurrencyItem[] {
    return this.data?.revenueByCurrency ?? [];
  }

  revenueIconClassForCard(): string {
    return currencyIconClass(this.revenueCurrencyCode);
  }

  formatCurrency(v: number | null | undefined): string {
    if (this.revenueCurrencyMixed) {
      return 'Mixed';
    }
    return formatCurrencyAmount(v ?? 0, this.revenueCurrencyCode);
  }

  formatCurrencyLine(item: FinancialRevenueByCurrencyItem, kind: 'total' | 'monthly' | 'aov'): string {
    const value =
      kind === 'total' ? item.totalRevenue
        : kind === 'monthly' ? item.monthlyRevenue
        : item.averageOrderValue;
    return formatCurrencyAmount(value, item.currencyCode);
  }

  formatPercent(v: number): string {
    return `${v >= 0 ? '+' : ''}${v}%`;
  }
}
