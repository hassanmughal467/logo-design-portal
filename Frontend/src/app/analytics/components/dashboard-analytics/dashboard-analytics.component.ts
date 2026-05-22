import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { AnalyticsOverview } from '@core/services/admin-analytics.service';
import { getOrderStatusLabel, getOrderStatusSeverity } from '@shared/utils/order-status-display';

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

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(v);
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

  getStatusSeverity(status: string): string {
    return getOrderStatusSeverity(status);
  }

  formatDate(d: Date | string | undefined): string {
    if (!d) return 'N/A';
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
