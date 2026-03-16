import { Component, Input } from '@angular/core';
import { FinancialActivityItem } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-financial-activity-feed',
  templateUrl: './financial-activity-feed.component.html',
  styleUrls: ['./financial-activity-feed.component.scss']
})
export class FinancialActivityFeedComponent {
  @Input() items: FinancialActivityItem[] = [];
  @Input() loading = false;

  getIcon(type: string): string {
    switch (type) {
      case 'InvoicePaid': return 'pi pi-check-circle';
      case 'NewOrder': return 'pi pi-shopping-cart';
      case 'OrderCompleted': return 'pi pi-check';
      case 'RefundIssued': return 'pi pi-undo';
      case 'LargeTransaction': return 'pi pi-bolt';
      default: return 'pi pi-circle';
    }
  }

  getSeverity(type: string): string {
    switch (type) {
      case 'InvoicePaid':
      case 'OrderCompleted': return 'success';
      case 'NewOrder': return 'info';
      case 'RefundIssued': return 'warn';
      case 'LargeTransaction': return 'primary';
      default: return 'secondary';
    }
  }

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 0 }).format(v);
  }

  formatDate(d: string): string {
    const date = new Date(d);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    if (diffMins < 60) return diffMins + 'm ago';
    if (diffHours < 24) return diffHours + 'h ago';
    if (diffDays < 7) return diffDays + 'd ago';
    return date.toLocaleDateString();
  }
}
