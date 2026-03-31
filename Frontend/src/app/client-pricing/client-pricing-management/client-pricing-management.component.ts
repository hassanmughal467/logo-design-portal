import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { SharedListDataService } from '@core/services/shared-list-data.service';
import { MessageService, OverlayOptions } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DesignCategory, DesignType } from '@shared/models/design-pricing.model';

export interface ClientOption {
  id: string;
  label: string;
  clientProfileId: string;
}

export interface ClientLogoPricing {
  id: string;
  clientId: string;
  clientName?: string;
  designCategory: number;
  designCategoryName: string;
  designType: number;
  designTypeName: string;
  price: number;
  currencyCode: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

@Component({
  selector: 'app-client-pricing-management',
  templateUrl: './client-pricing-management.component.html',
  styleUrls: ['./client-pricing-management.component.scss']
})
export class ClientPricingManagementComponent implements OnInit, OnDestroy {
  clients: ClientOption[] = [];
  selectedClient: ClientOption | null = null;
  pricings: ClientLogoPricing[] = [];
  loading = false;
  loadingPricings = false;
  displayAddDialog = false;
  displayEditDialog = false;
  selectedPricing: ClientLogoPricing | null = null;
  saving = false;

  addForm: FormGroup;
  editForm: FormGroup;

  currencyOptions = [
    { label: 'USD', value: 'USD' },
    { label: 'PKR', value: 'PKR' },
    { label: 'EUR', value: 'EUR' },
    { label: 'GBP', value: 'GBP' }
  ];

  designCategoryOptions = [
    { label: 'Embroidery Digitizing', value: DesignCategory.EmbroideryDigitizing },
    { label: 'Vector / Screen Printing', value: DesignCategory.VectorScreenPrinting },
    { label: 'Custom Patch', value: DesignCategory.CustomPatch }
  ];

  designTypeOptions: { label: string; value: number; category?: number }[] = [
    { label: 'Left Chest', value: DesignType.LeftChest, category: DesignCategory.EmbroideryDigitizing },
    { label: 'Jacket Back', value: DesignType.JacketBack, category: DesignCategory.EmbroideryDigitizing },
    { label: 'Simple Vector', value: DesignType.SimpleVector, category: DesignCategory.VectorScreenPrinting },
    { label: 'Complex Vector', value: DesignType.ComplexVector, category: DesignCategory.VectorScreenPrinting },
    { label: 'Left Chest', value: DesignType.LeftChest, category: DesignCategory.CustomPatch },
    { label: 'Jacket Back', value: DesignType.JacketBack, category: DesignCategory.CustomPatch }
  ];

  filteredDesignTypeOptions: { label: string; value: number }[] = [];

  /**
   * Dropdowns inside p-dialog: append panel to body (not clipped by dialog overflow)
   * and do not close the panel when the dialog content scrolls (PrimeNG scroll listener).
   */
  readonly pricingDialogDropdownOverlay: OverlayOptions = {
    appendTo: 'body',
    baseZIndex: 12000,
    autoZIndex: true,
    listener: (_event, opts) => {
      if (opts?.type === 'scroll') {
        return false;
      }
      return opts?.valid ?? true;
    }
  };

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private fb: FormBuilder,
    private sharedListData: SharedListDataService
  ) {
    this.addForm = this.fb.group({
      clientId: ['', Validators.required],
      designCategory: [null, Validators.required],
      designType: [null, Validators.required],
      price: [0, [Validators.required, Validators.min(0.01)]],
      currencyCode: ['USD', Validators.required],
      isActive: [true]
    });

    this.editForm = this.fb.group({
      price: [0, [Validators.required, Validators.min(0.01)]],
      currencyCode: ['USD', Validators.required],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadClients();
    this.addForm.get('designCategory')?.valueChanges.subscribe(cat => {
      this.updateDesignTypeOptions(cat);
      this.addForm.patchValue({ designType: null });
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateDesignTypeOptions(category: number | null): void {
    if (!category) {
      this.filteredDesignTypeOptions = [];
      return;
    }
    this.filteredDesignTypeOptions = this.designTypeOptions
      .filter(opt => opt.category === category)
      .map(opt => ({ label: opt.label, value: opt.value }));
  }

  loadClients(): void {
    this.loading = true;
    this.sharedListData
      .getAllUsers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (usersRaw) => {
          const users = usersRaw as any[];
          this.clients = users
            .filter(u => (u.role === 'Client' || u.roleName === 'Client') && (u.clientProfile?.id ?? u.clientProfile?.Id))
            .map(u => {
              const profileId = u.clientProfile?.id ?? u.clientProfile?.Id;
              const companyName = u.clientProfile?.companyName ?? u.clientProfile?.CompanyName ?? '';
              const label = `${u.firstName || ''} ${u.lastName || ''}`.trim()
                ? `${(u.firstName || '').trim()} ${(u.lastName || '').trim()} (${companyName || u.email || 'Client'})`.trim()
                : (companyName || u.email || 'Client');
              return {
                id: u.id,
                label,
                clientProfileId: profileId
              };
            });
          this.loading = false;
        },
        error: () => {
          this.clients = [];
          this.loading = false;
        }
      });
  }

  onClientSelect(): void {
    if (this.selectedClient) {
      this.loadPricings(this.selectedClient.clientProfileId);
    } else {
      this.pricings = [];
    }
  }

  loadPricings(clientId: string): void {
    this.loadingPricings = true;
    this.apiService.get<ClientLogoPricing[]>(`client-logo-pricing/client/${clientId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.pricings = data;
          this.loadingPricings = false;
        },
        error: () => {
          this.pricings = [];
          this.loadingPricings = false;
        }
      });
  }

  openAddDialog(): void {
    if (!this.selectedClient) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Select Client',
        detail: 'Please select a client first.'
      });
      return;
    }
    this.addForm.patchValue({
      clientId: this.selectedClient.clientProfileId,
      designCategory: null,
      designType: null,
      price: 0,
      currencyCode: 'USD',
      isActive: true
    });
    this.filteredDesignTypeOptions = [];
    this.displayAddDialog = true;
  }

  saveAdd(): void {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }
    this.saving = true;
    const val = this.addForm.value;
    this.apiService.post<any>('client-logo-pricing', {
      clientId: val.clientId,
      designCategory: val.designCategory,
      designType: val.designType,
      price: val.price,
      currencyCode: val.currencyCode,
      isActive: val.isActive
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Pricing added.' });
        this.displayAddDialog = false;
        this.saving = false;
        if (this.selectedClient) this.loadPricings(this.selectedClient.clientProfileId);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to add pricing.'
        });
        this.saving = false;
      }
    });
  }

  openEditDialog(pricing: ClientLogoPricing): void {
    this.selectedPricing = pricing;
    this.editForm.patchValue({
      price: pricing.price,
      currencyCode: pricing.currencyCode,
      isActive: pricing.isActive
    });
    this.displayEditDialog = true;
  }

  saveEdit(): void {
    if (this.editForm.invalid || !this.selectedPricing) return;
    this.saving = true;
    const val = this.editForm.value;
    this.apiService.put<any>(`client-logo-pricing/${this.selectedPricing.id}`, {
      price: val.price,
      currencyCode: val.currencyCode,
      isActive: val.isActive
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Pricing updated.' });
        this.displayEditDialog = false;
        this.selectedPricing = null;
        this.saving = false;
        if (this.selectedClient) this.loadPricings(this.selectedClient.clientProfileId);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to update pricing.'
        });
        this.saving = false;
      }
    });
  }

  disablePricing(pricing: ClientLogoPricing): void {
    this.apiService.put<any>(`client-logo-pricing/${pricing.id}`, {
      price: pricing.price,
      currencyCode: pricing.currencyCode,
      isActive: false
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Pricing disabled.' });
        if (this.selectedClient) this.loadPricings(this.selectedClient.clientProfileId);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to disable pricing.'
        });
      }
    });
  }

  deletePricing(pricing: ClientLogoPricing): void {
    if (!confirm('Delete this pricing? It will no longer apply to new orders.')) return;
    this.apiService.delete<any>(`client-logo-pricing/${pricing.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Pricing deleted.' });
          if (this.selectedClient) this.loadPricings(this.selectedClient.clientProfileId);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err.error?.error || 'Failed to delete pricing.'
          });
        }
      });
  }

  formatCurrency(amount: number, code: string): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: code || 'USD'
    }).format(amount);
  }
}
