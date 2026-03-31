import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { BillingService, BillingQueueOverview, BillingEligibleOrder } from '@core/services/billing.service';
import { MessageService } from 'primeng/api';
import { Subject, firstValueFrom, forkJoin, of } from 'rxjs';
import { takeUntil, catchError, finalize } from 'rxjs/operators';

export interface InvoiceItem {
  id: string;
  orderId?: string;
  orderTitle?: string;
  orderDate?: Date | string;
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
  globalFilter = '';
  selectedStatus: string | null = null;
  first = 0;
  rows = 10;

  statuses = ['Paid', 'Unpaid', 'Overdue'];

  // Generate invoice dialog
  showGenerateDialog = false;
  clientsWithUninvoiced: BillingQueueOverview[] = [];
  selectedClient: BillingQueueOverview | null = null;
  availableOrders: { label: string; value: string }[] = [];
  selectedOrderIds: string[] = [];
  selectedBillingType: number = 1; // Default: PerLogo
  loadingClients = false;
  loadingOrders = false;
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
  editItems: { id?: string; orderId?: string; description: string; amount: number; isNew?: boolean }[] = [];
  removedItemIds: string[] = [];

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

  isInvoicesLoading = false;
  isInvoicesRefreshing = false;
  hasInvoicesLoadedOnce = false;

  get showInvoiceSkeleton(): boolean {
    return this.isInvoicesLoading && !this.hasInvoicesLoadedOnce;
  }

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private billingService: BillingService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  /** Admin/SuperAdmin only: Generate Invoice, Bulk Pay, admin Pay (mark-paid). */
  get canManageInvoices(): boolean {
    const role = this.authService.getCurrentUser()?.role || this.authService.getCurrentUser()?.roleName;
    return role === 'Admin' || role === 'SuperAdmin';
  }

  /** Matches API: PUT invoice / items allowed for Admin and SuperAdmin (not paid / locked). */
  get canEditInvoices(): boolean {
    return this.canManageInvoices;
  }

  ngOnInit(): void {
    this.loadInvoiceData();
  }

  /** Open invoice detail when navigated via /invoices/:id (e.g. from notification click) */
  private openInvoiceFromRoute(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    const invoice = this.invoices.find(inv => inv.id === id || inv.id?.toLowerCase() === id?.toLowerCase());
    if (invoice) {
      this.openDetailDialogFromApi(invoice);
    } else {
      this.apiService.get<any>(`invoices/${id}`)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (raw) => {
            this.openDetailDialog(this.normalizeInvoiceFromApi(raw));
          },
          error: () => this.cdr.markForCheck()
        });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** Loads invoices and statistics together; keeps layout on refresh with a light dim. */
  loadInvoiceData(): void {
    if (!this.hasInvoicesLoadedOnce) {
      this.isInvoicesLoading = true;
    } else {
      this.isInvoicesRefreshing = true;
    }

    forkJoin({
      invoices: this.apiService.get<Invoice[]>('invoices').pipe(
        catchError((error) => {
          console.error('Error loading invoices:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Failed to Load Invoices',
            detail: error?.error?.error || 'Could not load invoices. Please try again.',
            life: 5000
          });
          return of([] as Invoice[]);
        })
      ),
      stats: this.apiService.get<any>('invoices/statistics').pipe(catchError(() => of(null)))
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isInvoicesLoading = false;
          this.isInvoicesRefreshing = false;
          this.hasInvoicesLoadedOnce = true;
          this.cdr.markForCheck();
        })
      )
      .subscribe(({ invoices, stats }) => {
        this.invoices = invoices;
        if (stats) {
          this.invoiceStats = {
            total: stats.totalInvoices || 0,
            paid: stats.paidInvoices || 0,
            unpaid: stats.dueInvoices || 0,
            overdue: stats.overdueInvoices || 0,
            totalAmount: stats.totalAmount || 0,
            paidAmount: stats.paidAmount || 0,
            pendingAmount: (stats.dueAmount || 0) + (stats.overdueAmount || 0)
          };
        } else {
          this.calculateStats(invoices);
        }
        this.cdr.markForCheck();
        this.openInvoiceFromRoute();
      });
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
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to download invoice PDF'
          });
        }
      });
  }

  openGenerateDialog(): void {
    this.selectedClient = null;
    this.availableOrders = [];
    this.selectedOrderIds = [];
    this.selectedBillingType = 1;
    this.loadClientsWithUninvoiced();
    this.showGenerateDialog = true;
  }

  openFlexibleBuilder(): void {
    this.router.navigate(['/invoices/flexible-builder']);
  }

  loadClientsWithUninvoiced(): void {
    this.loadingClients = true;
    this.billingService.getBillingQueue()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (clients) => {
          this.clientsWithUninvoiced = clients;
          this.loadingClients = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.clientsWithUninvoiced = [];
          this.loadingClients = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load clients with uninvoiced orders'
          });
          this.cdr.markForCheck();
        }
      });
  }

  onClientSelected(): void {
    this.selectedOrderIds = [];
    this.availableOrders = [];
    if (!this.selectedClient) return;
    this.loadEligibleOrdersForClient();
  }

  loadEligibleOrdersForClient(): void {
    if (!this.selectedClient) return;
    this.loadingOrders = true;
    this.billingService.getEligibleOrders(this.selectedClient.clientId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (orders: BillingEligibleOrder[]) => {
          this.availableOrders = orders.map(o => ({
            label: `Order #${o.orderNumber} – ${o.title} – $${o.price}`,
            value: o.orderId
          }));
          this.loadingOrders = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.availableOrders = [];
          this.loadingOrders = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load eligible orders'
          });
          this.cdr.markForCheck();
        }
      });
  }
  
  openDetailDialog(invoice: Invoice): void {
    this.selectedInvoice = invoice;
    this.showDetailDialog = true;
    this.cdr.markForCheck();
  }

  /** Load latest invoice (line items, totals) then open detail — useful after list is stale. */
  openDetailDialogFromApi(invoice: Invoice): void {
    this.apiService.get<any>(`invoices/${invoice.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (raw) => {
          this.selectedInvoice = this.normalizeInvoiceFromApi(raw);
          this.showDetailDialog = true;
          this.cdr.markForCheck();
        },
        error: () => {
          this.openDetailDialog(invoice);
        }
      });
  }

  /** Map API DTO to local Invoice shape (partial/case variants). */
  private normalizeInvoiceFromApi(raw: any): Invoice {
    const itemsRaw = raw.items || raw.Items || [];
    const items: InvoiceItem[] = (Array.isArray(itemsRaw) ? itemsRaw : []).map((it: any) => ({
      id: it.id || it.Id,
      orderId: it.orderId ?? it.OrderId,
      orderTitle: it.orderTitle ?? it.OrderTitle,
      orderDate: (it.orderDate ?? it.OrderDate)
        ? new Date(it.orderDate ?? it.OrderDate)
        : undefined,
      description: it.description ?? it.Description ?? '',
      amount: Number(it.amount ?? it.Amount ?? 0)
    }));
    return {
      id: raw.id || raw.Id,
      invoiceNumber: raw.invoiceNumber ?? raw.InvoiceNumber ?? '',
      clientId: raw.clientId ?? raw.ClientId ?? '',
      clientName: raw.clientName ?? raw.ClientName ?? '',
      orderId: raw.orderId ?? raw.OrderId,
      orderIds: raw.orderIds ?? raw.OrderIds,
      amount: Number(raw.amount ?? raw.Amount ?? 0),
      taxAmount: Number(raw.taxAmount ?? raw.TaxAmount ?? 0),
      totalAmount: Number(raw.totalAmount ?? raw.TotalAmount ?? raw.amount ?? 0),
      billingType: raw.billingType ?? raw.BillingType ?? 1,
      billingTypeDisplay: raw.billingTypeDisplay ?? raw.BillingTypeDisplay ?? '',
      status: (raw.status ?? raw.Status ?? 'Pending') as Invoice['status'],
      dueDate: new Date(raw.dueDate ?? raw.DueDate),
      paidDate: (raw.paidDate ?? raw.PaidDate) ? new Date(raw.paidDate ?? raw.PaidDate) : undefined,
      paymentMethod: raw.paymentMethod ?? raw.PaymentMethod,
      notes: raw.notes ?? raw.Notes,
      items,
      isLocked:
        (raw.isLocked ?? raw.IsLocked) === true ||
        String(raw.status ?? raw.Status ?? '')
          .toLowerCase() === 'paid',
      createdAt: new Date(raw.createdAt ?? raw.CreatedAt ?? Date.now())
    };
  }

  generateInvoice(): void {
    if (!this.selectedClient) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a client'
      });
      return;
    }
    if (this.selectedOrderIds.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select at least one order'
      });
      return;
    }

    const billingPeriod = new Date().toLocaleString('default', { month: 'long' }) + ' ' + new Date().getFullYear();
    this.billingService.createInvoiceFromOrders(this.selectedClient.clientId, {
      orderIds: this.selectedOrderIds,
      billingPeriod
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice generated successfully'
          });
          this.showGenerateDialog = false;
          this.loadInvoiceData();
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to generate invoice'
          });
          this.cdr.markForCheck();
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
          this.showDetailDialog = false;
          this.loadInvoiceData();
          this.cdr.markForCheck();
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
    this.showDetailDialog = false;
    this.editInvoice = invoice;
    this.editBillingType = invoice.billingType;
    this.editDueDate = invoice.dueDate ? new Date(invoice.dueDate) : null;
    this.editTaxAmount = invoice.taxAmount || 0;
    this.editPaymentMethod = invoice.paymentMethod || '';
    this.editNotes = invoice.notes || '';
    this.editItems = (invoice.items || []).map(item => ({
      id: item.id,
      orderId: item.orderId,
      description: item.description,
      amount: item.amount
    }));
    this.removedItemIds = [];
    this.showEditDialog = true;
    this.cdr.markForCheck();
  }

  addManualEditItem(): void {
    this.editItems.push({
      description: 'Manual Item',
      amount: 0,
      isNew: true
    });
  }

  removeEditItem(index: number): void {
    const item = this.editItems[index];
    if (item?.id) {
      this.removedItemIds.push(item.id);
    }
    this.editItems.splice(index, 1);
    this.editItems = [...this.editItems];
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
    if (this.editItems.some(x => !!x.id)) {
      request.items = this.editItems.map(item => ({
        id: item.id,
        description: item.description,
        amount: item.amount
      })).filter(x => !!x.id);
    }

    this.apiService.put(`invoices/${this.editInvoice.id}`, request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          const manualItems = this.editItems
            .filter(x => x.isNew && !x.id)
            .map(x => ({ description: x.description, amount: x.amount }));

          if (this.removedItemIds.length > 0 || manualItems.length > 0) {
            this.apiService.put(`invoices/${this.editInvoice!.id}/items`, {
              removeItemIds: this.removedItemIds,
              addManualItems: manualItems
            }).pipe(takeUntil(this.destroy$)).subscribe({
              next: () => {
                this.messageService.add({
                  severity: 'success',
                  summary: 'Success',
                  detail: 'Invoice updated successfully'
                });
                this.showEditDialog = false;
                this.loadInvoiceData();
              },
              error: (error) => {
                this.messageService.add({
                  severity: 'error',
                  summary: 'Error',
                  detail: error.error?.error || 'Failed to update invoice items'
                });
              }
            });
            return;
          }

          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice updated successfully'
          });
          this.showEditDialog = false;
          this.loadInvoiceData();
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
        this.loadInvoiceData();
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
    this.loadInvoiceData();
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
