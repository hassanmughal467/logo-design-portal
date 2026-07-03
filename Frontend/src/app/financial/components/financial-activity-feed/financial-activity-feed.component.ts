import { Component, Input } from '@angular/core';
import { FinancialActivityItem } from '@core/services/financial-analytics.service';
import { formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-financial-activity-feed',
  templateUrl: './financial-activity-feed.component.html',
  styleUrls: ['./financial-activity-feed.component.scss']
})
export class FinancialActivityFeedComponent {
  @Input() items: FinancialActivityItem[] = [];

  formatCurrency(v: number, currencyCode?: string): string {
    return formatCurrencyAmount(v, currencyCode);
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getSeverity(type: string): string {
    switch (type) {
      case 'RefundIssued':
        return 'warn';
      case 'LargeTransaction':
        return 'info';
      case 'InvoicePaid':
      case 'OrderCompleted':
        return 'success';
      default:
        return 'neutral';
    }
  }

  getIcon(type: string): string {
    switch (type) {
      case 'InvoicePaid':
        return 'pi pi-check-circle';
      case 'NewOrder':
        return 'pi pi-shopping-cart';
      case 'OrderCompleted':
        return 'pi pi-flag';
      case 'RefundIssued':
        return 'pi pi-replay';
      case 'LargeTransaction':
        return 'pi pi-star';
      default:
        return 'pi pi-info-circle';
    }
  }
}
