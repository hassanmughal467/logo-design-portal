import { Component, OnInit } from '@angular/core';
import { BillingService, BillingQueueOverview, BillingEligibleOrder } from '../../../core/services/billing.service';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-billing-queue',
  templateUrl: './billing-queue.component.html',
  styleUrls: ['./billing-queue.component.scss'],
  providers: [ConfirmationService]
})
export class BillingQueueComponent implements OnInit {
  queue: BillingQueueOverview[] = [];
  selectedClient: BillingQueueOverview | null = null;
  eligibleOrders: BillingEligibleOrder[] = [];
  ordersLoading = false;
  selectedOrderIds: Set<string> = new Set();
  orderEditablePrices: Record<string, number> = {};
  createInvoiceLoading = false;
  billingPeriod = '';
  displayCreateInvoiceDialog = false;

  constructor(
    private billingService: BillingService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadQueue();
  }

  loadQueue(): void {
    this.billingService.getBillingQueue().subscribe({
      next: (data) => {
        this.queue = data;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load billing queue.' });
      }
    });
  }

  onClientClick(client: BillingQueueOverview): void {
    this.selectedClient = client;
    this.displayCreateInvoiceDialog = true;
    this.selectedOrderIds = new Set();
    this.billingPeriod = this.getDefaultBillingPeriod();
    this.loadEligibleOrders();
  }

  loadEligibleOrders(): void {
    if (!this.selectedClient) return;
    this.ordersLoading = true;
    this.billingService.getEligibleOrders(this.selectedClient.clientId).subscribe({
      next: (orders) => {
        this.eligibleOrders = orders;
        this.selectedOrderIds = new Set(orders.map(o => o.orderId));
        this.orderEditablePrices = {};
        orders.forEach(o => {
          this.orderEditablePrices[o.orderId] = o.price;
        });
        this.ordersLoading = false;
      },
      error: () => {
        this.ordersLoading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load eligible orders.' });
      }
    });
  }

  toggleOrder(orderId: string): void {
    if (this.selectedOrderIds.has(orderId)) {
      this.selectedOrderIds.delete(orderId);
    } else {
      this.selectedOrderIds.add(orderId);
      const order = this.eligibleOrders.find(o => o.orderId === orderId);
      if (order && this.orderEditablePrices[orderId] === undefined) {
        this.orderEditablePrices[orderId] = order.price;
      }
    }
    this.selectedOrderIds = new Set(this.selectedOrderIds);
  }

  onPriceChange(orderId: string, value: number): void {
    this.orderEditablePrices[orderId] = value ?? 0;
  }

  selectAll(): void {
    this.selectedOrderIds = new Set(this.eligibleOrders.map(o => o.orderId));
  }

  selectNone(): void {
    this.selectedOrderIds = new Set();
  }

  isSelected(orderId: string): boolean {
    return this.selectedOrderIds.has(orderId);
  }

  get selectedTotal(): number {
    return this.eligibleOrders
      .filter(o => this.selectedOrderIds.has(o.orderId))
      .reduce((sum, o) => sum + (this.orderEditablePrices[o.orderId] ?? o.price), 0);
  }

  createInvoice(): void {
    if (!this.selectedClient || this.selectedOrderIds.size === 0) {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Select at least one order.' });
      return;
    }

    this.confirmationService.confirm({
      message: `Create invoice for ${this.selectedOrderIds.size} order(s) totaling $${this.selectedTotal.toFixed(2)}?`,
      header: 'Confirm Invoice Creation',
      icon: 'pi pi-file-edit',
      accept: () => this.doCreateInvoice()
    });
  }

  private doCreateInvoice(): void {
    if (!this.selectedClient) return;
    this.createInvoiceLoading = true;
    const orders = Array.from(this.selectedOrderIds).map(orderId => ({
      orderId,
      price: this.orderEditablePrices[orderId] ?? this.eligibleOrders.find(o => o.orderId === orderId)?.price ?? 0
    }));
    this.billingService.createInvoiceFromOrders(this.selectedClient.clientId, {
      orders,
      billingPeriod: this.billingPeriod || undefined
    }).subscribe({
      next: () => {
        this.createInvoiceLoading = false;
        this.displayCreateInvoiceDialog = false;
        this.selectedClient = null;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Invoice created successfully.' });
        this.loadQueue();
      },
      error: (err) => {
        this.createInvoiceLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to create invoice.'
        });
      }
    });
  }

  closeDialog(): void {
    this.displayCreateInvoiceDialog = false;
    this.selectedClient = null;
    this.eligibleOrders = [];
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  private getDefaultBillingPeriod(): string {
    const now = new Date();
    return now.toLocaleString('default', { month: 'long' }) + ' ' + now.getFullYear();
  }
}
