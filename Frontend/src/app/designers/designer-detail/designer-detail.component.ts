import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Order } from '@shared/models/order.model';
import { getOrderStatusLabel, getOrderStatusSeverity, OrderStatusSeverity } from '@shared/utils/order-status-display';
import { TagSeverity } from '@shared/types/primeng.types';
import { formatCurrencyAmount } from '@core/utils/currency-format';

export interface DesignerDetail {
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    roleName: string;
    isActive: boolean;
  };
  designerProfile: {
    id: string;
    specialization: string;
    bio: string;
    hourlyRate: number;
    isAvailable: boolean;
    notes?: string;
  };
  assignedOrders: Order[];
  completedOrdersCount: number;
  averageDeliveryTimeDays?: number;
  isAvailable: boolean;
  notes?: string;
}

@Component({
  selector: 'app-designer-detail',
  templateUrl: './designer-detail.component.html',
  styleUrls: ['./designer-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DesignerDetailComponent implements OnInit, OnDestroy {
  designerId: string | null = null;
  designer: DesignerDetail | null = null;
  loadFailed = false;
  errorMessage = '';
  activeTab: number = 0;

  // Tab data
  orders: Order[] = [];
  ordersLoading = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.designerId = this.route.snapshot.paramMap.get('id');
    if (this.designerId) {
      this.loadDesigner();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDesigner(): void {
    this.loadFailed = false;
    this.errorMessage = '';
    this.apiService.get<DesignerDetail>(`users/designers/${this.designerId}/detail`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (designerDetail) => {
          this.designer = designerDetail;
          this.orders = designerDetail.assignedOrders || [];
          this.ordersLoading = false;
          this.loadFailed = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.ordersLoading = false;
          this.loadFailed = true;
          this.errorMessage = err.error?.error || 'Failed to load designer details';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.errorMessage
          });
          this.cdr.markForCheck();
        }
      });
  }

  viewOrder(orderId: string): void {
    this.router.navigate(['/orders', orderId]);
  }

  /** Hourly rate is in PKR; pass order.currencyCode for client order amounts. */
  formatCurrency(amount: number, currencyCode?: string | null): string {
    const code = currencyCode ?? 'PKR';
    return formatCurrencyAmount(amount, code);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatDays(days: number | undefined): string {
    if (!days) return 'N/A';
    return `${days.toFixed(1)} days`;
  }

  formatStatus(status: string): string {
    return getOrderStatusLabel(status);
  }

  getStatusSeverity(status: string): TagSeverity {
    const orderSeverity = getOrderStatusSeverity(status);
    if (orderSeverity !== 'secondary') {
      return orderSeverity;
    }
    const designerSeverityMap: Record<string, TagSeverity> = {
      Assigned: 'info',
      PreviewUploaded: 'info',
      Approved: 'success'
    };
    return designerSeverityMap[status] || 'secondary';
  }

  goBack(): void {
    this.router.navigate(['/designers']);
  }
}
