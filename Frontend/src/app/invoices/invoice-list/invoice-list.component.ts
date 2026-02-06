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
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading invoices:', error);
          // Create mock invoices from orders for demo
          this.createMockInvoices();
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
}
