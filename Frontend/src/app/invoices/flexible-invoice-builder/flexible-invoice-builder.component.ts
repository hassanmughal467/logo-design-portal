import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { MessageService } from 'primeng/api';
import {
  BillingEligibleOrder,
  BillingQueueOverview,
  BillingService,
  BillingQueueResult
} from '@core/services/billing.service';
import { DEFAULT_INVOICE_CURRENCY, formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-flexible-invoice-builder',
  templateUrl: './flexible-invoice-builder.component.html',
  styleUrls: ['./flexible-invoice-builder.component.scss']
})
export class FlexibleInvoiceBuilderComponent implements OnInit {
  clients: BillingQueueOverview[] = [];
  selectedClient: BillingQueueOverview | null = null;
  fromDate: Date | null = null;
  toDate: Date | null = null;
  useManualSelection = false;
  notes = '';

  loadingClients = false;
  loadingOrders = false;
  generating = false;

  orders: BillingEligibleOrder[] = [];
  selectedOrderIds = new Set<string>();
  previewTotal = 0;
  private prefillClientId: string | null = null;
  private prefillOrderId: string | null = null;
  private prefillManualMode = false;

  constructor(
    private billingService: BillingService,
    private messageService: MessageService,
    public router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.prefillClientId = this.route.snapshot.queryParamMap.get('clientId');
    this.prefillOrderId = this.route.snapshot.queryParamMap.get('orderId');
    this.prefillManualMode = this.route.snapshot.queryParamMap.get('mode') === 'manual';
    this.loadClients();
  }

  loadClients(): void {
    this.loadingClients = true;
    this.billingService.getBillingQueue().subscribe({
      next: (clients) => {
        this.clients = clients;
        if (this.prefillClientId) {
          this.selectedClient = this.clients.find(c => c.clientId === this.prefillClientId) || null;
        }
        if (this.prefillManualMode || !!this.prefillOrderId) {
          this.useManualSelection = true;
        }
        this.loadingClients = false;
        if (this.selectedClient) {
          this.loadOrdersPreview();
        }
      },
      error: () => {
        this.loadingClients = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load clients.' });
      }
    });
  }

  onClientChanged(): void {
    this.orders = [];
    this.selectedOrderIds = new Set<string>();
    this.previewTotal = 0;
  }

  loadOrdersPreview(): void {
    if (!this.selectedClient) {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please select a client.' });
      return;
    }

    if (!this.useManualSelection && (!this.fromDate || !this.toDate)) {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please select both from and to dates.' });
      return;
    }

    this.loadingOrders = true;
    this.billingService.getBillingQueueFiltered({
      clientId: this.selectedClient.clientId,
      fromDate: this.useManualSelection ? undefined : this.toIsoDate(this.fromDate),
      toDate: this.useManualSelection ? undefined : this.toIsoDate(this.toDate),
      onlyUninvoiced: true
    }).subscribe({
      next: (result: BillingQueueResult) => {
        this.orders = result.orders ?? [];
        this.previewTotal = result.totalAmountPreview ?? 0;
        this.selectedOrderIds = new Set(this.orders.map(o => o.orderId));
        if (this.prefillOrderId) {
          const exists = this.orders.some(o => o.orderId === this.prefillOrderId);
          if (exists) {
            this.selectedOrderIds = new Set([this.prefillOrderId]);
          }
        }
        this.loadingOrders = false;
      },
      error: () => {
        this.loadingOrders = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load eligible orders.' });
      }
    });
  }

  toggleOrder(orderId: string): void {
    if (this.selectedOrderIds.has(orderId)) {
      this.selectedOrderIds.delete(orderId);
    } else {
      this.selectedOrderIds.add(orderId);
    }
    this.selectedOrderIds = new Set(this.selectedOrderIds);
  }

  isSelected(orderId: string): boolean {
    return this.selectedOrderIds.has(orderId);
  }

  get selectedTotal(): number {
    return this.orders
      .filter(o => this.selectedOrderIds.has(o.orderId))
      .reduce((sum, o) => sum + o.price, 0);
  }

  /** When all loaded orders share one currency; otherwise default (totals may mix currencies). */
  get previewCurrencyCode(): string {
    if (this.orders.length === 0) return DEFAULT_INVOICE_CURRENCY;
    const codes = [
      ...new Set(
        this.orders.map(o => (o.currencyCode || DEFAULT_INVOICE_CURRENCY).trim().toUpperCase())
      )
    ];
    return codes.length === 1 ? codes[0] : DEFAULT_INVOICE_CURRENCY;
  }

  get selectedTotalCurrency(): string {
    const selected = this.orders.filter(o => this.selectedOrderIds.has(o.orderId));
    if (selected.length === 0) return DEFAULT_INVOICE_CURRENCY;
    const codes = [
      ...new Set(
        selected.map(o => (o.currencyCode || DEFAULT_INVOICE_CURRENCY).trim().toUpperCase())
      )
    ];
    return codes.length === 1 ? codes[0] : DEFAULT_INVOICE_CURRENCY;
  }

  generateInvoice(): void {
    if (!this.selectedClient) {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please select a client.' });
      return;
    }

    const selectedIds = Array.from(this.selectedOrderIds);
    const request: any = {
      clientId: this.selectedClient.clientId,
      includeUninvoicedOnly: true,
      notes: this.notes || undefined
    };

    if (this.useManualSelection) {
      if (selectedIds.length === 0) {
        this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Select at least one order.' });
        return;
      }
      request.selectedOrderIds = selectedIds;
    } else {
      if (!this.fromDate || !this.toDate) {
        this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please select from and to dates.' });
        return;
      }
      request.fromDate = this.toIsoDate(this.fromDate);
      request.toDate = this.toIsoDate(this.toDate);
    }

    this.generating = true;
    this.billingService.generateFlexibleInvoice(request).subscribe({
      next: () => {
        this.generating = false;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Flexible invoice generated successfully.' });
        this.router.navigate(['/invoices']);
      },
      error: (err) => {
        this.generating = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to generate flexible invoice.'
        });
      }
    });
  }

  formatCurrency(value: number, currencyCode?: string | null): string {
    return formatCurrencyAmount(value, currencyCode ?? DEFAULT_INVOICE_CURRENCY);
  }

  private toIsoDate(date: Date | null): string | undefined {
    if (!date) return undefined;
    const y = date.getFullYear();
    const m = `${date.getMonth() + 1}`.padStart(2, '0');
    const d = `${date.getDate()}`.padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
