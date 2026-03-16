import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import {
  DesignerInvoice,
  DesignerInvoiceAdjustment
} from '@shared/models/design-pricing.model';

@Component({
  selector: 'app-invoice-detail',
  templateUrl: './invoice-detail.component.html',
  styleUrls: ['./invoice-detail.component.scss']
})
export class InvoiceDetailComponent implements OnInit {
  invoice: DesignerInvoice | null = null;
  loading = true;
  isAdmin = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    this.isAdmin = user?.role === 'Admin' || user?.role === 'SuperAdmin';
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadInvoice(id);
    } else {
      this.loading = false;
    }
  }

  loadInvoice(id: string): void {
    this.loading = true;
    const endpoint = this.isAdmin ? `designer-invoice/${id}` : `designer-payout/me/invoices/${id}`;
    this.apiService.get<DesignerInvoice>(endpoint).subscribe({
      next: (data) => {
        this.invoice = data;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Invoice not found.'
        });
        this.loading = false;
        this.router.navigate(['/financial/designer-payout']);
      }
    });
  }

  get ordersTotal(): number {
    if (!this.invoice?.items) return 0;
    return this.invoice.items.reduce((sum, i) => sum + i.amount, 0);
  }

  get adjustmentsTotal(): number {
    if (!this.invoice?.adjustments?.length) return 0;
    return this.invoice.adjustments.reduce((sum, a) => sum + a.amount, 0);
  }

  get bonusAdjustments(): DesignerInvoiceAdjustment[] {
    return (this.invoice?.adjustments || []).filter((a) => a.amount > 0);
  }

  get deductionAdjustments(): DesignerInvoiceAdjustment[] {
    return (this.invoice?.adjustments || []).filter((a) => a.amount < 0);
  }

  formatPayout(value: number): string {
    return `PKR ${value.toLocaleString('en-PK', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    })}`;
  }

  getStatusBadgeClass(): string {
    if (!this.invoice) return '';
    return this.invoice.status === 'Paid' ? 'badge-success' : 'badge-warning';
  }

  markAsPaid(): void {
    if (!this.invoice) return;
    this.apiService.put(`designer-invoice/${this.invoice.id}/mark-paid`, {}).subscribe({
      next: () => {
        this.invoice!.status = 'Paid';
        this.invoice!.paidDate = new Date();
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Invoice marked as paid.'
        });
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

  goBack(): void {
    this.router.navigate(['/financial/designer-payout']);
  }
}
