import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface Invoice {
  id: string;
  invoiceNumber: string;
  clientId: string;
  clientName: string;
  orderId?: string;
  amount: number;
  status: 'Paid' | 'Unpaid' | 'Overdue';
  dueDate: Date;
  paidDate?: Date;
  createdAt: Date;
}

@Component({
  selector: 'app-invoice-list',
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss']
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
  selectedOrderId: string | null = null;

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
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadInvoices();
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
        },
        error: (error) => {
          console.error('Error loading invoices:', error);
          // Create mock invoices from orders for demo
          this.createMockInvoices();
          this.calculateStats(this.invoices);
          this.loading = false;
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
    this.selectedOrderId = null;
    this.showGenerateDialog = true;
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
    if (!this.selectedOrderId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select an order'
      });
      return;
    }

    this.apiService.post('invoices', { orderId: this.selectedOrderId })
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
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to generate invoice'
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

  private calculateStats(invoices: Invoice[]): void {
    this.invoiceStats = {
      total: invoices.length,
      paid: invoices.filter(i => i.status === 'Paid').length,
      unpaid: invoices.filter(i => i.status === 'Unpaid').length,
      overdue: invoices.filter(i => i.status === 'Overdue').length,
      totalAmount: invoices.reduce((sum, i) => sum + i.amount, 0),
      paidAmount: invoices.filter(i => i.status === 'Paid').reduce((sum, i) => sum + i.amount, 0),
      pendingAmount: invoices.filter(i => i.status !== 'Paid').reduce((sum, i) => sum + i.amount, 0)
    };
  }
}
