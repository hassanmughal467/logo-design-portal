import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import {
  AnalyticsOverview,
  RevenueByCurrencyItem
} from '@core/services/admin-analytics.service';
import {
  DEFAULT_INVOICE_CURRENCY,
  currencyIconClass,
  formatCurrencyAmount
} from '@core/utils/currency-format';
import { getOrderStatusLabel, getOrderStatusSeverity, OrderStatusSeverity } from '@shared/utils/order-status-display';

@Component({
  selector: 'app-dashboard-analytics',
  templateUrl: './dashboard-analytics.component.html',
  styleUrls: ['./dashboard-analytics.component.scss']
})
export class DashboardAnalyticsComponent {
  @Input() overview: AnalyticsOverview | null = null;
  @Input() recentOrders: any[] = [];
  @Input() apiError = false;
  @Input() insights: string[] | undefined = [];

  insightsCollapsed = false;
  kpiCollapsed = false;
  workflowKpiCollapsed = false;
  recentOrdersCollapsed = false;

  constructor(private router: Router) {}

  /** ISO currency code for revenue KPIs (USD when none/mixed). */
  get revenueCurrencyCode(): string {
    return this.overview?.revenueCurrencyCode || DEFAULT_INVOICE_CURRENCY;
  }

  /** True when completed orders span more than one currency — raw sums are not meaningful. */
  get revenueCurrencyMixed(): boolean {
    return !!this.overview?.revenueCurrencyMixed;
  }

  /** Per-currency rollup, sorted by total revenue desc. Empty array when no completed orders. */
  get revenueByCurrency(): RevenueByCurrencyItem[] {
    return this.overview?.revenueByCurrency ?? [];
  }

  /** PrimeIcons class for the revenue stat-card icon (£/€/¥/$). */
  get revenueIconClass(): string {
    return currencyIconClass(this.revenueCurrencyCode);
  }

  /**
   * Formats a revenue amount using the order currency the backend reported.
   * When orders span multiple currencies, returns a "Mixed" label so the UI
   * never shows a misleading raw sum across currencies. The card body then
   * stacks the per-currency rows from {@link revenueByCurrency}.
   */
  formatCurrency(v: number | null | undefined): string {
    if (this.revenueCurrencyMixed) {
      return 'Mixed';
    }
    return formatCurrencyAmount(v ?? 0, this.revenueCurrencyCode);
  }

  /** Format a per-currency line in the mixed-currency KPI rollup. */
  formatCurrencyLine(item: RevenueByCurrencyItem, kind: 'total' | 'monthly' | 'aov'): string {
    const value =
      kind === 'total' ? item.totalRevenue
        : kind === 'monthly' ? item.monthlyRevenue
        : item.averageOrderValue;
    return formatCurrencyAmount(value, item.currencyCode);
  }

  navigateToOrders(): void {
    this.router.navigate(['/orders']);
  }

  navigateToOrder(id: string): void {
    this.router.navigate(['/orders', id]);
  }

  formatStatus(status: string): string {
    return getOrderStatusLabel(status);
  }

  getStatusSeverity(status: string): OrderStatusSeverity {
    return getOrderStatusSeverity(status);
  }

  formatDate(d: Date | string | undefined): string {
    if (!d) return 'N/A';
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
