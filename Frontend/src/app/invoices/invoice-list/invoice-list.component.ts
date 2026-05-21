import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { BillingService, BillingQueueOverview, BillingEligibleOrder } from '@core/services/billing.service';
import { LazyLoadEvent, MessageService } from 'primeng/api';
import { Subject, firstValueFrom, of } from 'rxjs';
import { takeUntil, catchError, finalize, map } from 'rxjs/operators';
import { DEFAULT_INVOICE_CURRENCY, formatCurrencyAmount } from '@core/utils/currency-format';

export interface InvoiceItem {
  id: string;
  orderId?: string;
  orderTitle?: string;
  orderDate?: Date | string;
  description: string;
  amount: number;
  /** From linked logo order; manual lines omit. */
  currencyCode?: string | null;
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
  /** When all order lines share one ISO code (e.g. GBP); manual-only may be absent. */
  currencyCode?: string | null;
}

@Component({
  selector: 'app-invoice-list',
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoiceListComponent implements OnInit, OnDestroy {
  /** Current tab page from the API (server-side pagination). */
  tabInvoices: Invoice[] = [];
  listTotalRecords = 0;
  tableFirst = 0;
  tableRows = 25;
  readonly tablePageSizeOptions = [10, 25, 50, 100];
  isTableLoading = false;

  globalFilter = '';
  selectedStatus: string | null = null;

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

  // Statistics (KPI + period buckets from GET invoices/statistics)
  invoiceStats = {
    total: 0,
    paid: 0,
    unpaid: 0,
    overdue: 0,
    totalAmount: 0,
    paidAmount: 0,
    pendingAmount: 0,
    week: { total: 0, paid: 0, pending: 0 },
    month: { total: 0, paid: 0, pending: 0 }
  };

  // Payment functionality
  selectedInvoices: Invoice[] = [];
  showPaymentDialog = false;
  paymentMethod = '';
  selectedInvoiceTabIndex = 0;

  private destroy$ = new Subject<void>();
  /** Avoid duplicate auto-open when route id unchanged. */
  private prevRouteInvoiceParam: string | null = null;
  /** Skip p-tabView onChange until the first server load finishes (avoids duplicate fetches). */
  private suppressInvoiceTabReload = true;

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

  /** True when any line is $0 — common if client pricing was not configured when the order was placed. */
  invoiceLineItemsNeedAmountReview(invoice: Invoice | null): boolean {
    if (!invoice?.items?.length) return false;
    return invoice.items.some(i => (i.amount ?? 0) <= 0);
  }

  ngOnInit(): void {
    this.loadInvoiceData();
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((pm) => {
      const id = pm.get('id');
      if (id === this.prevRouteInvoiceParam) return;
      this.prevRouteInvoiceParam = id;
      if (this.hasInvoicesLoadedOnce) {
        this.openInvoiceFromRoute();
      }
    });
  }

  onInvoiceTabChange(event: { index: number }): void {
    if (this.suppressInvoiceTabReload) {
      return;
    }
    this.selectedInvoiceTabIndex = event.index;
    this.tableFirst = 0;
    this.selectedInvoices = [];
    this.loadTabInvoicesPage(1, this.tableRows, false);
  }

  onInvoicesLazyLoad(event: LazyLoadEvent): void {
    const rows = event.rows ?? this.tableRows;
    const first = event.first ?? 0;
    const page = Math.floor(first / rows) + 1;
    this.loadTabInvoicesPage(page, rows, false);
  }

  /** API returns PagedResultDto { items, total, ... }; support legacy bare array. */
  private parseInvoicesListResponse(response: unknown): Invoice[] {
    const rows = ApiService.extractItems<any>(response);
    const list =
      rows.length > 0 ? rows : Array.isArray(response) ? (response as any[]) : [];
    return list.map((raw) => this.normalizeInvoiceFromApi(raw));
  }

  private parsePagedInvoicesResponse(response: unknown): { items: Invoice[]; total: number } {
    const items = this.parseInvoicesListResponse(response);
    const meta = ApiService.extractPagedMeta(response);
    return { items, total: meta.total > 0 ? meta.total : items.length };
  }

  /** Tab 0 unpaid (exclude paid), 1 pending, 2 overdue, 3 paid — matches backend InvoiceStatus. */
  private buildInvoicesListQuery(page: number, pageSize: number): string {
    const tab = this.selectedInvoiceTabIndex;
    let filter = '';
    if (tab === 0) {
      filter = '&excludePaid=true';
    } else if (tab === 1) {
      filter = '&status=1';
    } else if (tab === 2) {
      filter = '&status=4';
    } else if (tab === 3) {
      filter = '&status=2';
    }
    return `invoices?page=${page}&pageSize=${pageSize}${filter}`;
  }

  private applyInvoiceStatistics(stats: any | null): void {
    if (!stats) {
      return;
    }
    const ws = stats.weekSummary ?? stats.WeekSummary ?? {};
    const ms = stats.monthSummary ?? stats.MonthSummary ?? {};
    this.invoiceStats = {
      total: stats.totalInvoices ?? 0,
      paid: stats.paidInvoices ?? 0,
      unpaid: stats.dueInvoices ?? 0,
      overdue: stats.overdueInvoices ?? 0,
      totalAmount: stats.totalAmount ?? 0,
      paidAmount: stats.paidAmount ?? 0,
      pendingAmount: (stats.dueAmount ?? 0) + (stats.overdueAmount ?? 0),
      week: {
        total: Number(ws.totalAmount ?? ws.TotalAmount ?? 0),
        paid: Number(ws.paidAmount ?? ws.PaidAmount ?? 0),
        pending: Number(ws.pendingAmount ?? ws.PendingAmount ?? 0)
      },
      month: {
        total: Number(ms.totalAmount ?? ms.TotalAmount ?? 0),
        paid: Number(ms.paidAmount ?? ms.PaidAmount ?? 0),
        pending: Number(ms.pendingAmount ?? ms.PendingAmount ?? 0)
      }
    };
  }

  private loadTabInvoicesPage(page: number, pageSize: number, initialFullLoad: boolean): void {
    const query = this.buildInvoicesListQuery(page, pageSize);
    this.isTableLoading = true;
    this.apiService
      .get<any>(query)
      .pipe(
        takeUntil(this.destroy$),
        catchError((error) => {
          console.error('Error loading invoices:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Failed to Load Invoices',
            detail: error?.error?.error || 'Could not load invoices. Please try again.',
            life: 5000
          });
          return of(null);
        }),
        finalize(() => {
          this.isTableLoading = false;
          if (initialFullLoad) {
            this.isInvoicesLoading = false;
            this.hasInvoicesLoadedOnce = true;
            this.suppressInvoiceTabReload = false;
          } else if (this.hasInvoicesLoadedOnce) {
            this.isInvoicesRefreshing = false;
          }
          this.cdr.markForCheck();
        })
      )
      .subscribe((response) => {
        if (response) {
          const { items, total } = this.parsePagedInvoicesResponse(response);
          this.tabInvoices = items;
          this.listTotalRecords = total;
          this.tableFirst = (page - 1) * pageSize;
          this.tableRows = pageSize;
        } else {
          this.tabInvoices = [];
          this.listTotalRecords = 0;
        }
        this.cdr.markForCheck();
        if (initialFullLoad) {
          this.openInvoiceFromRoute();
        }
      });
  }

  /** Open invoice detail when navigated via /invoices/:id (e.g. from notification click) */
  private openInvoiceFromRoute(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }
    const invoice = this.tabInvoices.find(
      (inv) => inv.id === id || inv.id?.toLowerCase() === id?.toLowerCase()
    );
    if (invoice) {
      this.openDetailDialogFromApi(invoice);
    } else {
      this.apiService
        .get<any>(`invoices/${id}`)
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

  /** Loads statistics then first page of the active tab (server-side pagination). */
  loadInvoiceData(): void {
    if (!this.hasInvoicesLoadedOnce) {
      this.isInvoicesLoading = true;
    } else {
      this.isInvoicesRefreshing = true;
    }

    this.apiService
      .get<any>('invoices/statistics')
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of(null)),
        map((stats) => {
          this.applyInvoiceStatistics(stats);
          return stats;
        })
      )
      .subscribe(() => {
        this.cdr.markForCheck();
        const page = Math.floor(this.tableFirst / this.tableRows) + 1;
        this.loadTabInvoicesPage(page, this.tableRows, !this.hasInvoicesLoadedOnce);
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
            label: `Order #${o.orderNumber} – ${o.title} – ${formatCurrencyAmount(o.price, o.currencyCode)}`,
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
      amount: Number(it.amount ?? it.Amount ?? 0),
      currencyCode: it.currencyCode ?? it.CurrencyCode ?? null
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
      createdAt: new Date(raw.createdAt ?? raw.CreatedAt ?? Date.now()),
      currencyCode: raw.currencyCode ?? raw.CurrencyCode ?? null
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

  /**
   * @param currencyCode ISO code from invoice/order; KPI / cross-invoice aggregates use default (USD).
   */
  formatCurrency(amount: number, currencyCode?: string | null): string {
    return formatCurrencyAmount(amount, currencyCode ?? DEFAULT_INVOICE_CURRENCY);
  }

  /** Dashboard KPIs / week-month stats (amounts may mix currencies server-side). */
  formatStatCurrency(amount: number): string {
    return formatCurrencyAmount(amount, DEFAULT_INVOICE_CURRENCY);
  }

  invoiceDisplayCurrency(invoice: Invoice): string {
    const c = invoice.currencyCode?.trim();
    if (c) return c.toUpperCase();
    const fromItem = invoice.items?.find((i) => i.currencyCode?.trim())?.currencyCode;
    return fromItem?.trim().toUpperCase() || DEFAULT_INVOICE_CURRENCY;
  }

  lineItemDisplayCurrency(item: InvoiceItem, invoice: Invoice | null): string {
    const c = item.currencyCode?.trim();
    if (c) return c.toUpperCase();
    return invoice ? this.invoiceDisplayCurrency(invoice) : DEFAULT_INVOICE_CURRENCY;
  }

  get editInvoiceCurrency(): string {
    return this.editInvoice ? this.invoiceDisplayCurrency(this.editInvoice) : DEFAULT_INVOICE_CURRENCY;
  }

  /** Single currency for bulk-pay total when all selected invoices agree; else default. */
  bulkPaymentDisplayCurrency(): string {
    if (this.selectedInvoices.length === 0) return DEFAULT_INVOICE_CURRENCY;
    const codes = this.selectedInvoices.map((i) => this.invoiceDisplayCurrency(i));
    const uniq = [...new Set(codes)];
    return uniq.length === 1 ? uniq[0] : DEFAULT_INVOICE_CURRENCY;
  }

  selectedInvoicesMixedCurrency(): boolean {
    if (this.selectedInvoices.length < 2) return false;
    const codes = this.selectedInvoices.map((i) => this.invoiceDisplayCurrency(i));
    return new Set(codes).size > 1;
  }

  formatInvoiceAmount(invoice: Invoice, amount: number): string {
    return formatCurrencyAmount(amount, this.invoiceDisplayCurrency(invoice));
  }

  formatLineAmount(item: InvoiceItem, invoice: Invoice): string {
    return formatCurrencyAmount(item.amount, this.lineItemDisplayCurrency(item, invoice));
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

  getSelectedInvoicesTotal(): number {
    return this.selectedInvoices.reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);
  }

  openPaymentDialog(invoice?: Invoice): void {
    if (invoice) {
      this.selectedInvoices = [invoice];
    }
    this.showPaymentDialog = true;
  }

  /** Bulk pay for rows on the current page (current tab / server page). */
  openBulkPayForCurrentPage(): void {
    this.selectedInvoices = [...this.tabInvoices];
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

  // Report download methods
  downloadInvoiceReport(format: 'csv' | 'pdf'): void {
    if (format === 'csv') {
      this.downloadCSVForCurrentTab();
    } else {
      this.downloadPDF();
    }
  }

  /** CSV includes every row for the current tab (all pages). */
  private async downloadCSVForCurrentTab(): Promise<void> {
    const pageSize = 100;
    let page = 1;
    const all: Invoice[] = [];
    let total = Infinity;
    try {
      while (all.length < total) {
        const query = this.buildInvoicesListQuery(page, pageSize);
        const res = await firstValueFrom(this.apiService.get<any>(query));
        const { items, total: t } = this.parsePagedInvoicesResponse(res);
        all.push(...items);
        total = t;
        if (items.length < pageSize || items.length === 0) break;
        page++;
      }
    } catch {
      this.messageService.add({
        severity: 'error',
        summary: 'Export failed',
        detail: 'Could not load invoices for CSV export.'
      });
      return;
    }
    this.downloadCSV(all);
  }

  private downloadCSV(invoices: Invoice[]): void {
    const headers = ['Invoice Number', 'Currency', 'Amount', 'Status', 'Due Date', 'Paid Date'];
    const rows = invoices.map(inv => [
      inv.invoiceNumber,
      this.invoiceDisplayCurrency(inv),
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

  private downloadPDF(): void {
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
