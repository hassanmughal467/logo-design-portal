import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import {
  OrderPricingSummary,
  DesignerInvoice,
  DesignerInvoiceItem,
  DesignerInvoiceAdjustment
} from '@shared/models/design-pricing.model';

interface DesignerOption {
  id: string;
  label: string;
}

interface PayoutSummary {
  designersWithPendingPayout: number;
  totalEligibleOrders: number;
  totalPendingPayoutAmount: number;
}

@Component({
  selector: 'app-designer-payout',
  templateUrl: './designer-payout.component.html',
  styleUrls: ['./designer-payout.component.scss']
})
export class DesignerPayoutComponent implements OnInit {
  loading = false;
  isAdmin = false;
  isDesigner = false;
  pendingOrders: OrderPricingSummary[] = [];
  myOrdersPendingApproval: OrderPricingSummary[] = [];
  designers: DesignerOption[] = [];
  selectedDesignerId: string | null = null;
  selectedYear = new Date().getFullYear();
  selectedMonth = new Date().getMonth() + 1;
  designerInvoices: DesignerInvoice[] = [];
  eligibleOrders: OrderPricingSummary[] = [];
  selectedInvoice: DesignerInvoice | null = null;
  showInvoiceDialog = false;
  editingItemId: string | null = null;
  editItemAmount = 0;
  newAdjustmentDesc = '';
  newAdjustmentAmount = 0;

  // Payout summary (admin)
  payoutSummary: PayoutSummary | null = null;
  summaryLoading = false;

  // Preview flow
  previewOrders: OrderPricingSummary[] = [];
  previewLoading = false;
  showPreview = false;

  yearOptions: { label: string; value: number }[] = [];
  monthOptions = [
    { label: 'January', value: 1 }, { label: 'February', value: 2 }, { label: 'March', value: 3 },
    { label: 'April', value: 4 }, { label: 'May', value: 5 }, { label: 'June', value: 6 },
    { label: 'July', value: 7 }, { label: 'August', value: 8 }, { label: 'September', value: 9 },
    { label: 'October', value: 10 }, { label: 'November', value: 11 }, { label: 'December', value: 12 }
  ];

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router
  ) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear; y >= currentYear - 3; y--) {
      this.yearOptions.push({ label: String(y), value: y });
    }
  }

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    this.isAdmin = user?.role === 'Admin' || user?.role === 'SuperAdmin';
    this.isDesigner = user?.role === 'Designer';
    if (this.isAdmin) {
      this.loadPayoutSummary();
      this.loadPendingOrders();
      this.loadDesigners();
    }
    if (this.isDesigner) {
      this.loadMyInvoices();
      this.loadMyEligibleOrders();
      this.loadMyOrdersPendingApproval();
    }
  }

  loadPayoutSummary(): void {
    this.summaryLoading = true;
    this.apiService.get<PayoutSummary>('designer-invoice/payout-summary').subscribe({
      next: (data) => {
        this.payoutSummary = data;
        this.summaryLoading = false;
      },
      error: () => {
        this.payoutSummary = null;
        this.summaryLoading = false;
      }
    });
  }

  loadMyInvoices(): void {
    this.loading = true;
    this.apiService.get<DesignerInvoice[]>('designer-payout/me/invoices').subscribe({
      next: (data) => {
        this.designerInvoices = data || [];
        this.loading = false;
      },
      error: () => {
        this.designerInvoices = [];
        this.loading = false;
      }
    });
  }

  loadMyEligibleOrders(): void {
    this.apiService.get<OrderPricingSummary[]>('designer-payout/me/eligible-orders').subscribe({
      next: (data) => {
        this.eligibleOrders = data || [];
      },
      error: () => {
        this.eligibleOrders = [];
      }
    });
  }

  loadMyOrdersPendingApproval(): void {
    this.apiService.get<OrderPricingSummary[]>('designer-payout/me/orders-pending-approval').subscribe({
      next: (data) => {
        this.myOrdersPendingApproval = data || [];
      },
      error: () => {
        this.myOrdersPendingApproval = [];
      }
    });
  }

  loadPendingOrders(): void {
    this.loading = true;
    this.apiService.get<OrderPricingSummary[]>('designer-payout/orders/pending-approval').subscribe({
      next: (data) => {
        this.pendingOrders = data || [];
        this.loading = false;
      },
      error: () => {
        this.pendingOrders = [];
        this.loading = false;
      }
    });
  }

  loadDesigners(): void {
    this.apiService.get<any[]>('users/designer-profiles').subscribe({
      next: (data) => {
        this.designers = (data || []).map((d: any) => ({
          id: d.id || d.Id,
          label: `${d.userFirstName || d.UserFirstName || ''} ${d.userLastName || d.UserLastName || ''}`.trim() || d.userEmail || d.UserEmail || 'Designer'
        }));
      },
      error: () => {
        this.designers = [];
      }
    });
  }

  loadDesignerInvoices(): void {
    if (!this.selectedDesignerId) return;
    this.apiService.get<DesignerInvoice[]>(`designer-invoice/designer/${this.selectedDesignerId}`).subscribe({
      next: (data) => {
        this.designerInvoices = data || [];
      },
      error: () => {
        this.designerInvoices = [];
      }
    });
  }

  onDesignerSelect(): void {
    this.loadDesignerInvoices();
    this.showPreview = false;
    this.previewOrders = [];
  }

  previewEligibleOrders(): void {
    if (!this.selectedDesignerId) {
      this.messageService.add({ severity: 'warn', summary: 'Required', detail: 'Select a designer first.' });
      return;
    }
    this.previewLoading = true;
    this.showPreview = true;
    this.apiService
      .get<OrderPricingSummary[]>(
        `designer-invoice/eligible-orders?designerId=${this.selectedDesignerId}&year=${this.selectedYear}&month=${this.selectedMonth}`
      )
      .subscribe({
        next: (data) => {
          this.previewOrders = data || [];
          this.previewLoading = false;
          if (this.previewOrders.length === 0) {
            this.messageService.add({
              severity: 'info',
              summary: 'No Orders',
              detail: `No eligible orders found for the selected period.`
            });
          }
        },
        error: (err) => {
          this.previewOrders = [];
          this.previewLoading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err.error?.error || 'Failed to load eligible orders'
          });
        }
      });
  }

  get previewTotalAmount(): number {
    return this.previewOrders.reduce((sum, o) => sum + (o.approvedPrice ?? 0), 0);
  }

  generateInvoice(): void {
    if (!this.selectedDesignerId) {
      this.messageService.add({ severity: 'warn', summary: 'Required', detail: 'Select a designer first.' });
      return;
    }
    this.loading = true;
    this.apiService
      .post<DesignerInvoice>(
        `designer-invoice/generate?designerId=${this.selectedDesignerId}&year=${this.selectedYear}&month=${this.selectedMonth}`,
        {}
      )
      .subscribe({
        next: (invoice) => {
          this.loading = false;
          this.designerInvoices = [invoice, ...this.designerInvoices];
          this.selectedInvoice = invoice;
          this.showInvoiceDialog = true;
          this.showPreview = false;
          this.previewOrders = [];
          this.loadPayoutSummary();
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `Invoice ${invoice.invoiceNumber} generated.`
          });
        },
        error: (err) => {
          this.loading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err.error?.error || 'Failed to generate invoice'
          });
        }
      });
  }

  markInvoicePaid(invoice: DesignerInvoice): void {
    this.apiService.put(`designer-invoice/${invoice.id}/mark-paid`, {}).subscribe({
      next: () => {
        invoice.status = 'Paid';
        invoice.paidDate = new Date();
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Invoice marked as paid.' });
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to mark as paid'
        });
      }
    });
  }

  viewInvoice(invoice: DesignerInvoice): void {
    this.router.navigate(['/financial/designer-payout/invoice', invoice.id]);
  }

  closeInvoiceDialog(): void {
    this.showInvoiceDialog = false;
    this.selectedInvoice = null;
    this.editingItemId = null;
  }

  startEditItem(item: DesignerInvoiceItem): void {
    this.editingItemId = item.id;
    this.editItemAmount = item.amount;
  }

  cancelEditItem(): void {
    this.editingItemId = null;
  }

  saveItemEdit(invoice: DesignerInvoice): void {
    if (!this.editingItemId || this.editItemAmount <= 0) return;
    this.apiService
      .put(`designer-invoice/${invoice.id}/items/${this.editingItemId}`, { amount: this.editItemAmount })
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Item updated.' });
          this.editingItemId = null;
          this.refreshSelectedInvoice(invoice.id);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err.error?.error || 'Failed to update item'
          });
        }
      });
  }

  addAdjustment(invoice: DesignerInvoice): void {
    if (!this.newAdjustmentDesc?.trim()) return;
    this.apiService
      .post(`designer-invoice/${invoice.id}/adjustments`, {
        description: this.newAdjustmentDesc.trim(),
        amount: this.newAdjustmentAmount
      })
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Adjustment added.' });
          this.newAdjustmentDesc = '';
          this.newAdjustmentAmount = 0;
          this.refreshSelectedInvoice(invoice.id);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err.error?.error || 'Failed to add adjustment'
          });
        }
      });
  }

  removeAdjustment(invoice: DesignerInvoice, adj: DesignerInvoiceAdjustment): void {
    this.apiService.delete(`designer-invoice/${invoice.id}/adjustments/${adj.id}`).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Adjustment removed.' });
        this.refreshSelectedInvoice(invoice.id);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to remove adjustment'
        });
      }
    });
  }

  private refreshSelectedInvoice(invoiceId: string): void {
    this.apiService.get<DesignerInvoice>(`designer-invoice/${invoiceId}`).subscribe({
      next: (inv) => {
        this.selectedInvoice = inv;
        const listIdx = this.designerInvoices.findIndex((i) => i.id === invoiceId);
        if (listIdx >= 0) {
          this.designerInvoices = [...this.designerInvoices];
          this.designerInvoices[listIdx] = inv;
        }
      }
    });
  }

  formatPayout(value: number): string {
    return `PKR ${value.toLocaleString('en-PK', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
  }

  getOrdersCount(invoice: DesignerInvoice): number {
    return invoice.items?.length ?? 0;
  }

  getStatusBadgeClass(status: string): string {
    const s = (status || '').toLowerCase();
    if (s === 'paid' || s === 'approved' || s === 'autoapproved') return 'badge-success';
    if (s === 'rejected') return 'badge-danger';
    return 'badge-warning';
  }
}
