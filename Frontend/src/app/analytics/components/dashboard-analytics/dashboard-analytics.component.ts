import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { AnalyticsOverview } from '@core/services/admin-analytics.service';

@Component({
  selector: 'app-dashboard-analytics',
  templateUrl: './dashboard-analytics.component.html',
  styleUrls: ['./dashboard-analytics.component.scss']
})
export class DashboardAnalyticsComponent {
  @Input() overview: AnalyticsOverview | null = null;
  @Input() loading = false;
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
    if (status === 'ClientApproved') return 'Approved';
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  getStatusSeverity(status: string): string {
    const m: Record<string, string> = {
      'Pending': 'warning', 'InProgress': 'info', 'PreviewDelivered': 'info',
      'RevisionRequested': 'warning', 'ClientApproved': 'success', 'Completed': 'success', 'Cancelled': 'danger',
      'WaitingForAdminApproval': 'warning', 'PriceApprovalPending': 'warning'
    };
    return m[status] || 'secondary';
  }

  formatDate(d: Date | string | undefined): string {
    if (!d) return 'N/A';
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
