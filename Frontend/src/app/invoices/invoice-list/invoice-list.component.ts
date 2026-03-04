import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject, firstValueFrom } from 'rxjs';
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

  // Edit invoice dialog
  showEditDialog = false;
  editInvoice: Invoice | null = null;
  editBillingType: number = 1;
  editDueDate: Date | null = null;
  editTaxAmount: number = 0;
  editPaymentMethod: string = '';
  editNotes: string = '';
  editItems: { id: string; description: string; amount: number }[] = [];

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

  // Payment functionality
  selectedInvoices: Invoice[] = [];
  showPaymentDialog = false;
  paymentMethod = '';
  selectedInvoiceTabIndex = 0;

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
    this.apiService.getBlob(`invoices/${invoiceId}/download`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `Invoice-${invoiceId}.pdf`;
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice PDF downloaded successfully'
          });
        },
        error: (error) => {
          console.error('Error downloading invoice PDF:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to download invoice PDF'
          });
        }
      });
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

  openEditDialog(invoice: Invoice): void {
    this.editInvoice = invoice;
    this.editBillingType = invoice.billingType;
    this.editDueDate = invoice.dueDate ? new Date(invoice.dueDate) : null;
    this.editTaxAmount = invoice.taxAmount || 0;
    this.editPaymentMethod = invoice.paymentMethod || '';
    this.editNotes = invoice.notes || '';
    this.editItems = (invoice.items || []).map(item => ({
      id: item.id,
      description: item.description,
      amount: item.amount
    }));
    this.showEditDialog = true;
  }

  saveInvoice(): void {
    if (!this.editInvoice) return;

    const request: any = {
      billingType: this.editBillingType,
      dueDate: this.editDueDate,
      taxAmount: this.editTaxAmount,
      paymentMethod: this.editPaymentMethod,
      notes: this.editNotes
    };

    // Include item updates if any
    if (this.editItems.length > 0) {
      request.items = this.editItems.map(item => ({
        id: item.id,
        description: item.description,
        amount: item.amount
      }));
    }

    this.apiService.put(`invoices/${this.editInvoice.id}`, request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice updated successfully'
          });
          this.showEditDialog = false;
          this.loadInvoices();
          this.loadStatistics();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to update invoice'
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
      unpaid: invoices.filter(i => i.status !== 'Paid').length, // Unpaid includes Overdue
      overdue: invoices.filter(i => i.status === 'Overdue').length,
      totalAmount: invoices.reduce((sum, i) => sum + (i.totalAmount || i.amount), 0),
      paidAmount: invoices.filter(i => i.status === 'Paid').reduce((sum, i) => sum + (i.totalAmount || i.amount), 0),
      pendingAmount: invoices.filter(i => i.status !== 'Paid').reduce((sum, i) => sum + (i.totalAmount || i.amount), 0)
    };
  }

  // Payment methods
  getUnpaidInvoices(): Invoice[] {
    return this.invoices.filter(inv => inv.status === 'Pending' || inv.status === 'Overdue');
  }

  getPendingInvoices(): Invoice[] {
    return this.invoices.filter(inv => inv.status === 'Pending');
  }

  getDueInvoices(): Invoice[] {
    return this.invoices.filter(inv => inv.status === 'Overdue');
  }

  getPaidInvoices(): Invoice[] {
    return this.invoices.filter(inv => inv.status === 'Paid');
  }

  getSelectedInvoicesTotal(): number {
    return this.selectedInvoices.reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);
  }

  openPaymentDialog(invoice?: Invoice): void {
    if (invoice) {
      this.selectedInvoices = [invoice];
    }
    this.showPaymentDialog = true;
  }

  cancelPayment(): void {
    this.showPaymentDialog = false;
    this.selectedInvoices = [];
    this.paymentMethod = '';
  }

  processPayment(): void {
    if (this.selectedInvoices.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select at least one invoice'
      });
      return;
    }

    // Process payments (for multiple invoices or manual payment)
    const paymentPromises = this.selectedInvoices.map(invoice =>
      firstValueFrom(
        this.apiService.put(`invoices/${invoice.id}/mark-paid`, {
          paymentMethod: this.paymentMethod || 'Manual'
        })
      )
    );

    Promise.all(paymentPromises)
      .then(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Payment processed for ${this.selectedInvoices.length} invoice(s)`
        });
        this.showPaymentDialog = false;
        this.selectedInvoices = [];
        this.paymentMethod = '';
        this.loadInvoices();
        this.loadStatistics();
        this.cdr.markForCheck();
      })
      .catch((error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to process payment'
        });
      });
  }

  onPaymentComplete(): void {
    // Called when payment component completes payment
    this.showPaymentDialog = false;
    this.selectedInvoices = [];
    this.paymentMethod = '';
    this.loadInvoices();
    this.loadStatistics();
    this.cdr.markForCheck();
  }

  // Summary methods
  getWeeklySummary(): { total: number; paid: number; pending: number } {
    const now = new Date();
    const weekStart = new Date(now.setDate(now.getDate() - now.getDay()));
    weekStart.setHours(0, 0, 0, 0);

    const weekInvoices = this.invoices.filter(inv => {
      const invDate = new Date(inv.createdAt);
      return invDate >= weekStart;
    });

    return {
      total: weekInvoices.reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0),
      paid: weekInvoices
        .filter(inv => inv.status === 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0),
      pending: weekInvoices
        .filter(inv => inv.status !== 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0)
    };
  }

  getMonthlySummary(): { total: number; paid: number; pending: number } {
    const now = new Date();
    const monthStart = new Date(now.getFullYear(), now.getMonth(), 1);

    const monthInvoices = this.invoices.filter(inv => {
      const invDate = new Date(inv.createdAt);
      return invDate >= monthStart;
    });

    return {
      total: monthInvoices.reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0),
      paid: monthInvoices
        .filter(inv => inv.status === 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0),
      pending: monthInvoices
        .filter(inv => inv.status !== 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0)
    };
  }

  // Report download methods
  downloadInvoiceReport(format: 'csv' | 'pdf'): void {
    const invoices = this.getInvoicesForCurrentTab();
    if (format === 'csv') {
      this.downloadCSV(invoices);
    } else {
      this.downloadPDF(invoices);
    }
  }

  private getInvoicesForCurrentTab(): Invoice[] {
    switch (this.selectedInvoiceTabIndex) {
      case 0:
        return this.getUnpaidInvoices();
      case 1:
        return this.getPendingInvoices();
      case 2:
        return this.getDueInvoices();
      case 3:
        return this.getPaidInvoices();
      default:
        return [];
    }
  }

  private downloadCSV(invoices: Invoice[]): void {
    const headers = ['Invoice Number', 'Amount', 'Status', 'Due Date', 'Paid Date'];
    const rows = invoices.map(inv => [
      inv.invoiceNumber,
      inv.totalAmount || inv.amount,
      inv.status,
      inv.dueDate ? new Date(inv.dueDate).toLocaleDateString() : 'N/A',
      inv.paidDate ? new Date(inv.paidDate).toLocaleDateString() : 'N/A'
    ]);

    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    const tabNames = ['unpaid', 'pending', 'due', 'paid'];
    link.download = `invoices_${tabNames[this.selectedInvoiceTabIndex] || 'all'}_${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  private downloadPDF(invoices: Invoice[]): void {
    // Map tab index to status for the backend filter
    const statusMap: { [key: number]: string } = {
      0: '', // Unpaid = all non-paid
      1: 'Pending',
      2: 'Overdue',
      3: 'Paid'
    };
    const status = statusMap[this.selectedInvoiceTabIndex] ?? '';
    const endpoint = status ? `invoices/report?status=${status}` : 'invoices/report';

    this.apiService.getBlob(endpoint)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          const tabNames = ['unpaid', 'pending', 'due', 'paid'];
          link.download = `invoices_${tabNames[this.selectedInvoiceTabIndex] || 'all'}_${new Date().toISOString().split('T')[0]}.pdf`;
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice report PDF downloaded successfully'
          });
        },
        error: (error) => {
          console.error('Error downloading invoice report PDF:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to download invoice report PDF'
          });
        }
      });
  }
}
