import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface InvoiceItem {
  id: string;
  orderId?: string;
  orderTitle?: string;
  description: string;
  amount: number;
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  clientId: string;
  clientName: string;
  orderId?: string; // Backward compatibility
  orderIds?: string[]; // New: multiple orders
  amount: number;
  taxAmount: number;
  totalAmount: number;
  billingType: number; // 1=PerLogo, 2=Weekly, 3=Monthly, 4=Manual
  billingTypeDisplay: string;
  status: 'Paid' | 'Pending' | 'Overdue';
  dueDate: Date;
  paidDate?: Date;
  paymentMethod?: string;
  notes?: string;
  items: InvoiceItem[];
  isLocked: boolean;
  createdAt: Date;
}

@Component({
  selector: 'app-invoice-list',
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoiceListComponent implements OnInit, OnDestroy {
  invoices: Invoice[] = [];
  loading = false;
  globalFilter = '';
  selectedStatus: string | null = null;
  first = 0;
  rows = 10;

  statuses = ['Paid', 'Unpaid', 'Overdue'];

  // Generate invoice dialog
  showGenerateDialog = false;
  availableOrders: any[] = [];
  selectedOrderIds: string[] = [];
  selectedBillingType: number = 1; // Default: PerLogo
  billingTypes = [
    { label: 'Per Logo', value: 1 },
    { label: 'Weekly', value: 2 },
    { label: 'Monthly', value: 3 },
    { label: 'Manual', value: 4 }
  ];
  
  // Invoice detail dialog
  showDetailDialog = false;
  selectedInvoice: Invoice | null = null;

  // Statistics
  invoiceStats = {
    total: 0,
    paid: 0,
    unpaid: 0,
    overdue: 0,
    totalAmount: 0,
    paidAmount: 0,
    pendingAmount: 0
  };

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadInvoices();
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.apiService.get<any>('invoices/statistics')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats) => {
          this.invoiceStats = {
            total: stats.totalInvoices || 0,
            paid: stats.paidInvoices || 0,
            unpaid: stats.dueInvoices || 0,
            overdue: stats.overdueInvoices || 0,
            totalAmount: stats.totalAmount || 0,
            paidAmount: stats.paidAmount || 0,
            pendingAmount: (stats.dueAmount || 0) + (stats.overdueAmount || 0)
          };
          this.cdr.markForCheck();
        },
        error: () => {
          // Statistics will be calculated from invoices if endpoint fails
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadInvoices(): void {
    this.loading = true;
    // Try to fetch invoices (will fail gracefully if endpoint doesn't exist)
    this.apiService.get<Invoice[]>('invoices')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (invoices) => {
          this.invoices = invoices;
          this.calculateStats(invoices);
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (error) => {
          console.error('Error loading invoices:', error);
          // Create mock invoices from orders for demo
          this.createMockInvoices();
          this.calculateStats(this.invoices);
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  private createMockInvoices(): void {
    // Mock data for demonstration - will be replaced when backend API is ready
    this.invoices = [];
  }

  downloadInvoice(invoiceId: string): void {
    // Download invoice PDF
    window.open(`http://localhost:5000/api/invoices/${invoiceId}/download`, '_blank');
  }

  openGenerateDialog(): void {
    this.loadAvailableOrders();
    this.selectedOrderIds = [];
    this.selectedBillingType = 1;
    this.showGenerateDialog = true;
  }
  
  openDetailDialog(invoice: Invoice): void {
    this.selectedInvoice = invoice;
    this.showDetailDialog = true;
  }

  loadAvailableOrders(): void {
    this.apiService.get<any[]>('orders')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (orders) => {
          // Filter orders that don't have invoices yet
          this.availableOrders = orders
            .filter(o => o.status === 'Completed')
            .map(o => ({ label: `${o.title} - $${o.price}`, value: o.id }));
        },
        error: () => {
          this.availableOrders = [];
        }
      });
  }

  generateInvoice(): void {
    if (this.selectedOrderIds.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select at least one order'
      });
      return;
    }

    // Support both single order (backward compatibility) and multiple orders
    const request: any = {
      orderIds: this.selectedOrderIds,
      billingType: this.selectedBillingType
    };

    this.apiService.post('invoices', request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice generated successfully'
          });
          this.showGenerateDialog = false;
          this.loadInvoices();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to generate invoice'
          });
        }
      });
  }

  sendInvoice(invoiceId: string): void {
    this.apiService.post(`invoices/${invoiceId}/send`, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice sent successfully'
          });
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to send invoice'
          });
        }
      });
  }

  getStatusSeverity(status: string): string {
    const severityMap: { [key: string]: string } = {
      'Paid': 'success',
      'Unpaid': 'warning',
      'Overdue': 'danger'
    };
    return severityMap[status] || 'secondary';
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  isOverdue(dueDate: Date | string | undefined): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date() && new Date(dueDate).getTime() !== 0;
  }

  getBillingTypeLabel(billingType: number): string {
    const type = this.billingTypes.find(t => t.value === billingType);
    return type?.label || 'Unknown';
  }
  
  getBillingTypeSeverity(billingType: number): string {
    const severityMap: { [key: number]: string } = {
      1: 'info',      // PerLogo
      2: 'warning',   // Weekly
      3: 'success',   // Monthly
      4: 'secondary'  // Manual
    };
    return severityMap[billingType] || 'secondary';
  }

  private calculateStats(invoices: Invoice[]): void {
    this.invoiceStats = {
      total: invoices.length,
      paid: invoices.filter(i => i.status === 'Paid').length,
      unpaid: invoices.filter(i => i.status !== 'Paid' && i.status !== 'Overdue').length,
      overdue: invoices.filter(i => i.status === 'Overdue').length,
      totalAmount: invoices.reduce((sum, i) => sum + (i.totalAmount || i.amount), 0),
      paidAmount: invoices.filter(i => i.status === 'Paid').reduce((sum, i) => sum + (i.totalAmount || i.amount), 0),
      pendingAmount: invoices.filter(i => i.status !== 'Paid').reduce((sum, i) => sum + (i.totalAmount || i.amount), 0)
    };
  }
}
