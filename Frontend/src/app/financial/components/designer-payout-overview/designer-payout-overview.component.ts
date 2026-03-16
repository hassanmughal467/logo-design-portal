import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';

export interface DesignerPayoutOverviewItem {
  designerId: string;
  designerName: string;
  completedOrders: number;
  pendingPayoutAmount: number;
}

@Component({
  selector: 'app-designer-payout-overview',
  templateUrl: './designer-payout-overview.component.html',
  styleUrls: ['./designer-payout-overview.component.scss']
})
export class DesignerPayoutOverviewComponent implements OnInit {
  items: DesignerPayoutOverviewItem[] = [];
  loading = false;
  generateLoadingDesignerId: string | null = null;
  showGenerateDialog = false;
  selectedItem: DesignerPayoutOverviewItem | null = null;
  selectedYear = new Date().getFullYear();
  selectedMonth = new Date().getMonth() + 1;

  yearOptions: { label: string; value: number }[] = [];
  monthOptions = [
    { label: 'January', value: 1 }, { label: 'February', value: 2 }, { label: 'March', value: 3 },
    { label: 'April', value: 4 }, { label: 'May', value: 5 }, { label: 'June', value: 6 },
    { label: 'July', value: 7 }, { label: 'August', value: 8 }, { label: 'September', value: 9 },
    { label: 'October', value: 10 }, { label: 'November', value: 11 }, { label: 'December', value: 12 }
  ];

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private router: Router
  ) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear; y >= currentYear - 3; y--) {
      this.yearOptions.push({ label: String(y), value: y });
    }
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.apiService.get<DesignerPayoutOverviewItem[]>('designer-invoice/payout-overview').subscribe({
      next: (data) => {
        this.items = data || [];
        this.loading = false;
      },
      error: () => {
        this.items = [];
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load designer payout overview.' });
      }
    });
  }

  get pendingDesignerCount(): number {
    return this.items.length;
  }

  onBadgeClick(): void {
    this.router.navigate(['/financial/designer-payout']);
  }

  openGenerateDialog(item: DesignerPayoutOverviewItem): void {
    this.selectedItem = item;
    this.selectedYear = new Date().getFullYear();
    this.selectedMonth = new Date().getMonth() + 1;
    this.showGenerateDialog = true;
  }

  closeGenerateDialog(): void {
    this.showGenerateDialog = false;
    this.selectedItem = null;
  }

  generateInvoice(): void {
    if (!this.selectedItem) return;

    this.generateLoadingDesignerId = this.selectedItem.designerId;
    this.apiService.post<any>(
      `designer-invoice/generate?designerId=${this.selectedItem.designerId}&year=${this.selectedYear}&month=${this.selectedMonth}`,
      {}
    ).subscribe({
      next: (invoice) => {
        this.generateLoadingDesignerId = null;
        this.closeGenerateDialog();
        this.messageService.add({ severity: 'success', summary: 'Success', detail: `Invoice ${invoice.invoiceNumber} generated.` });
        this.loadData();
      },
      error: (err) => {
        this.generateLoadingDesignerId = null;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to generate invoice.' });
      }
    });
  }

  formatPayout(value: number): string {
    return `PKR ${value.toLocaleString('en-PK', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
  }
}
